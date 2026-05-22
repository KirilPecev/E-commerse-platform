import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = __ENV.ORDER_SERVICE_URL || __ENV.BASE_URL || 'http://host.docker.internal:5004';
const ORDERS_PATH = __ENV.ORDERS_PATH || '/api/orders';
const PRODUCTS_PATH = __ENV.PRODUCTS_PATH || '/api/products';
const PRODUCT_SERVICE_URL = __ENV.PRODUCT_SERVICE_URL || 'http://host.docker.internal:5002';
const IDENTITY_BASE_URL = __ENV.IDENTITY_SERVICE_URL || 'http://host.docker.internal:5001';
const IDENTITY_LOGIN = __ENV.IDENTITY_LOGIN_PATH || '/api/identity/login';

export const options = {
    scenarios: {
        constant_load: {
            executor: 'constant-vus',
            vus: Number(__ENV.K6_VUS) || 100,
            duration: __ENV.K6_DURATION || '2m',
        },
    },

    thresholds: {
        http_req_failed: ['rate<0.01'],
        http_req_duration: ['p(95)<500'],
    },
};

function makeOrderPayload() {
    const productId = __ENV.TEST_PRODUCT_ID || null;

    return JSON.stringify({
        customerId: __ENV.TEST_CUSTOMER_ID || '11111111-1111-1111-1111-111111111111',
        items: [
            {
                productId: productId,
                quantity: 1,
            },
        ],
    });
}

export default function () {
    // attempt to get a product id from products endpoint if not provided
    let productId = __ENV.TEST_PRODUCT_ID;
    if (!productId) {
        try {
            const prodRes = http.get(`${PRODUCT_SERVICE_URL}${PRODUCTS_PATH}`);
            if (prodRes && prodRes.status === 200) {
                const list = JSON.parse(prodRes.body || '[]');
                if (Array.isArray(list) && list.length > 0) {
                    productId = list[Math.floor(Math.random() * list.length)].id || list[0].id;
                }
            }
        } catch (e) {
            // ignore and fall back to env product id
        }
    }

    // build payload with chosen product id
    const payloadObj = {
        customerId: __ENV.TEST_CUSTOMER_ID || '11111111-1111-1111-1111-111111111111',
        items: [ { productId: productId || __ENV.TEST_PRODUCT_ID || '22222222-2222-2222-2222-222222222222', quantity: 1 } ]
    };

    const params = { headers: { 'Content-Type': 'application/json' } };

    // optionally login to identity service to obtain bearer token
    if (__ENV.TEST_USER_EMAIL && __ENV.TEST_USER_PASSWORD) {
        try {
            const loginRes = http.post(`${IDENTITY_BASE_URL}${IDENTITY_LOGIN}`, JSON.stringify({ Email: __ENV.TEST_USER_EMAIL, Password: __ENV.TEST_USER_PASSWORD }), { headers: { 'Content-Type': 'application/json' } });
            if (loginRes && loginRes.status === 200) {
                const auth = JSON.parse(loginRes.body || '{}');
                if (auth && auth.token) {
                    params.headers.Authorization = `Bearer ${auth.token}`;
                }
            }
        } catch (e) {}
    }

    const url = `${BASE_URL}${ORDERS_PATH}`;
    const response = http.post(url, JSON.stringify(payloadObj), params);

    check(response, { 'status is 2xx': (r) => r.status >= 200 && r.status < 300 });

    // if created, optionally GET the new order to validate
    try {
        if (response && response.status >= 200 && response.status < 300 && response.body) {
            const created = JSON.parse(response.body || '{}');
            const orderId = created.id || created.orderId || created.orderID;
            if (orderId) {
                const getRes = http.get(`${BASE_URL}${ORDERS_PATH}/${orderId}`, params);
                check(getRes, { 'get order 200': (r) => r.status === 200 });
            }
        }
    } catch (e) {}

    sleep(1);
}