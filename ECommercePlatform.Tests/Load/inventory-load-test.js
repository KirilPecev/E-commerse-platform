import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = __ENV.INVENTORY_SERVICE_URL || __ENV.BASE_URL || 'http://host.docker.internal:5003';
const INVENTORY_PATH = __ENV.INVENTORY_PATH || '/api/inventory';

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
    const res = http.get(url);

    check(res, {
        'inventory status is 200': (r) => r.status === 200,
    });

    sleep(1);
}
