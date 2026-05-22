import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = __ENV.IDENTITY_SERVICE_URL || __ENV.BASE_URL || 'http://host.docker.internal:5001';
const IDENTITY_PATH = __ENV.IDENTITY_PATH || '/api/identity/register';

export const options = {
    vus: Number(__ENV.K6_VUS) || 50,
    duration: __ENV.K6_DURATION || '1m',
    thresholds: {
        http_req_failed: ['rate<0.01'],
        http_req_duration: ['p(95)<500'],
    },
};

export default function () {
    const url = `${BASE_URL}${IDENTITY_PATH}`;

    const email = `loadtest+${Math.floor(Math.random() * 1e9)}@example.com`;
    const payload = JSON.stringify({
        Email: email,
        Password: __ENV.TEST_PASSWORD || 'StrongPass123!'
    });

    const params = { headers: { 'Content-Type': 'application/json' } };

    const res = http.post(url, payload, params);

    check(res, {
        'register succeeded': (r) => r.status === 200,
    });

    sleep(1);
}
