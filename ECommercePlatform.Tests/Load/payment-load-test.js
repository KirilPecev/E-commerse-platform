import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = __ENV.PAYMENT_SERVICE_URL || __ENV.BASE_URL || 'http://host.docker.internal:5005';
const PAYMENT_PATH = __ENV.PAYMENT_PATH || '/api/payments';
const ORDERS_PATH = __ENV.ORDERS_PATH || '/api/orders';
const PRODUCTS_PATH = __ENV.PRODUCTS_PATH || '/api/products';
const PRODUCT_SERVICE_URL = __ENV.PRODUCT_SERVICE_URL || 'http://host.docker.internal:5002';
const ORDER_SERVICE_URL = __ENV.ORDER_SERVICE_URL || 'http://host.docker.internal:5004';
const IDENTITY_BASE_URL = __ENV.IDENTITY_SERVICE_URL || 'http://host.docker.internal:5001';
const IDENTITY_LOGIN = __ENV.IDENTITY_LOGIN_PATH || '/api/identity/login';

export const options = {
    vus: Number(__ENV.K6_VUS) || 40,
    duration: __ENV.K6_DURATION || '1m',
    thresholds: {
        http_req_failed: ['rate<0.01'],
        http_req_duration: ['p(95)<500'],
    },
};

function makePaymentPayload() {
    return JSON.stringify({
        orderId: __ENV.TEST_ORDER_ID || null,
        amount: Number(__ENV.TEST_AMOUNT) || 9.99,
        method: 'test',
    });
}

export default function () {
    const params = { headers: { 'Content-Type': 'application/json' } };

    // optionally login to get bearer token
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

    // Ensure we have an orderId: try env, else create an order
    let orderId = __ENV.TEST_ORDER_ID;
    if (!orderId) {
        // find a product
        try {
            const prodRes = http.get(`${PRODUCT_SERVICE_URL}${PRODUCTS_PATH}`);
            if (prodRes && prodRes.status === 200) {
                const list = JSON.parse(prodRes.body || '[]');
                const productId = Array.isArray(list) && list.length>0 ? (list[0].id || list[0].productId) : null;
                if (productId) {
                    const orderPayload = JSON.stringify({ customerId: __ENV.TEST_CUSTOMER_ID || '11111111-1111-1111-1111-111111111111', items: [{ productId: productId, quantity: 1 }] });
                    const orderRes = http.post(`${ORDER_SERVICE_URL}${ORDERS_PATH}`, orderPayload, params);
                    if (orderRes && orderRes.status >= 200 && orderRes.status < 300) {
                        const created = JSON.parse(orderRes.body || '{}');
                        orderId = created.id || created.orderId || created.orderID;
                    }
                }
            }
        } catch (e) {}
    }

    if (!orderId) {
        // fallback to env or a dummy id
        orderId = __ENV.TEST_ORDER_ID || '33333333-3333-3333-3333-333333333333';
    }

    const payload = JSON.stringify({ orderId: orderId, amount: Number(__ENV.TEST_AMOUNT) || 9.99, method: 'test' });
    const url = `${BASE_URL}${PAYMENT_PATH}`;

    const res = http.post(url, payload, params);

    check(res, { 'payment status is 2xx': (r) => r.status >= 200 && r.status < 300 });

    sleep(1);
}
