import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = __ENV.PRODUCT_SERVICE_URL || __ENV.BASE_URL || 'http://host.docker.internal:5002';
const PRODUCTS_PATH = __ENV.PRODUCTS_PATH || '/api/products';

export const options = {
    vus: Number(__ENV.K6_VUS) || 100,
    duration: __ENV.K6_DURATION || '1m',

    thresholds: {
        http_req_failed: ['rate<0.01'],
        http_req_duration: ['p(95)<500'],
    },
};

export default function () {
    const url = `${BASE_URL}${PRODUCTS_PATH}`;
    const response = http.get(url);

    if (response.status !== 200) {
        try {
            console.error(`GET ${url} -> ${response.status}`);
            // print first 512 chars of body for debugging
            console.error(response.body ? response.body.substring(0, 512) : '<no body>');
        } catch (e) {
            console.error('Error logging response body', e);
        }
    }

    check(response, {
        'status is 200': (r) => r.status === 200,
    });

    sleep(1);
}