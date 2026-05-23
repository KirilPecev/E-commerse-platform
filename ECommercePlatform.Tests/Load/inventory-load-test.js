import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = __ENV.INVENTORY_SERVICE_URL || __ENV.BASE_URL || 'http://host.docker.internal:5003';
const INVENTORY_PATH = __ENV.INVENTORY_PATH || '/api/inventory/a4b6a6cd-8ade-431b-90ac-9f1e183173d4';
const IDENTITY_BASE_URL = __ENV.IDENTITY_SERVICE_URL || 'http://host.docker.internal:5001';
const IDENTITY_LOGIN = __ENV.IDENTITY_LOGIN_PATH || '/api/identity/login';

export const options = {
    vus: Number(__ENV.K6_VUS) || 80,
    duration: __ENV.K6_DURATION || '1m',
    thresholds: {
        http_req_failed: ['rate<0.01'],
        http_req_duration: ['p(95)<500'],
    },
};

export default function () {
    const url = `${BASE_URL}${INVENTORY_PATH}`;

    const params = { headers: { 'Content-Type': 'application/json' } };

    // optionally login to identity service to obtain bearer token
    try {
        const loginRes = http.post(`${IDENTITY_BASE_URL}${IDENTITY_LOGIN}`, JSON.stringify({ Email: 'loadtest+95229640@example.com', Password: 'StrongPass123!' }), { headers: { 'Content-Type': 'application/json' } });
        if (loginRes && loginRes.status === 200) {
            const auth = JSON.parse(loginRes.body || '{}');
            if (auth && auth.token) {
                params.headers.Authorization = `Bearer ${auth.token}`;
            }
        }
    } catch (e) { }


    const res = http.get(url, params);


    const ok = check(res, {
        'inventory status is 200': (r) => r.status === 200,
    });

    // Log details when a request fails so we can see why in the load test output
    if (!ok) {
        const status = res.status;
        const timings = res.timings ? res.timings.duration : 'n/a';
        const headers = res.headers ? JSON.stringify(res.headers) : 'n/a';
        const error = res.error || 'none';
        // Truncate large bodies to avoid flooding the logs
        let bodyPreview = '';
        try {
            if (typeof res.body === 'string') {
                bodyPreview = res.body.substring(0, 2000);
            } else {
                bodyPreview = JSON.stringify(res.body).substring(0, 2000);
            }
        } catch (e) {
            bodyPreview = `unable to read body: ${e.message}`;
        }

        console.error(`[k6] Request FAILED: GET ${url}`);
        console.error(`[k6] Status: ${status}, Time: ${timings} ms, Error: ${error}`);
        console.error(`[k6] Headers: ${headers}`);
        console.error(`[k6] Body (truncated): ${bodyPreview}`);
    } else {
        // Short success log to give visibility during long runs
        const timings = res.timings ? res.timings.duration : 'n/a';
        console.log(`[k6] Request OK: GET ${url} - ${res.status} - ${timings} ms`);
    }

    sleep(1);
}
