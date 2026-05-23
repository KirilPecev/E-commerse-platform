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

    return JSON.stringify({
        customerId: __ENV.TEST_CUSTOMER_ID || '01517BA1-4550-43B3-BDEA-08DEB8B30E07',
        productId: "4a5eeb96-b40b-4aff-b003-601297565fac",
        quantity: 1,
        productVariantId: "4c5782c1-db34-43be-8500-3986496352e7",
        productName: "Dell",
        price: 1800,
        currency: "EUR",
        quantity: 1
    });
}

export default function () {
    // attempt to get a product id from products endpoint if not provided
    let productId = "4a5eeb96-b40b-4aff-b003-601297565fac";
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
    const payloadObj = makeOrderPayload();

    const params = { headers: { 'Content-Type': 'application/json' } };

    // optionally login to identity service to obtain bearer token
    try {
        const loginRes = http.post(`${IDENTITY_BASE_URL}${IDENTITY_LOGIN}`, JSON.stringify({ Email: 'loadtest+95229640@example.com', Password: 'StrongPass123!' }), { headers: { 'Content-Type': 'application/json' } });
        if (loginRes && loginRes.status === 200) {
            const auth = JSON.parse(loginRes.body || '{}');
            if (auth && auth.token) {
                params.headers.Authorization = `Bearer ${auth.token}`;
                console.log(`[k6] Obtained auth token, will include Authorization header`);
            } else {
                console.error(`[k6] Login succeeded but no token returned: ${loginRes.body}`);
            }
        } else {
            console.error(`[k6] Login failed: ${loginRes ? loginRes.status : 'no response'}`);
        }
    } catch (e) {
        console.error(`[k6] Login exception: ${e.message}`);
    }

    const url = `${BASE_URL}${ORDERS_PATH}`;
    const response = http.post(url, payloadObj, params);

    const createdOk = check(response, { 'status is 2xx': (r) => r.status >= 200 && r.status < 300 });

    function logRes(method, url, res) {
        if (!res) {
            console.error(`[k6] ${method} ${url} - no response`);
            return;
        }

        const ok = res.status >= 200 && res.status < 300;
        const status = res.status;
        const timings = res.timings ? res.timings.duration : 'n/a';
        const headers = res.headers ? JSON.stringify(res.headers) : 'n/a';
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

        if (!ok) {
            console.error(`[k6] Request FAILED: ${method} ${url}`);
            console.error(`[k6] Status: ${status}, Time: ${timings} ms`);
            console.error(`[k6] Headers: ${headers}`);
            console.error(`[k6] Body (truncated): ${bodyPreview}`);
        } else {
            console.log(`[k6] Request OK: ${method} ${url} - ${status} - ${timings} ms`);
        }
    }

    // log the create response
    logRes('POST', url, response);

    // if created, optionally GET the new order to validate
    try {
        if (response && response.status >= 200 && response.status < 300 && response.body) {
            const created = JSON.parse(response.body || '{}');
            const orderId = created.id || created.orderId || created.orderID;
            if (orderId) {
                const getUrl = `${BASE_URL}${ORDERS_PATH}/${orderId}`;
                const getRes = http.get(getUrl, params);
                const getOk = check(getRes, { 'get order 200': (r) => r.status === 200 });
                logRes('GET', getUrl, getRes);
                if (!getOk) {
                    // additional context when GET fails
                    console.error(`[k6] Failed to validate created order ${orderId}`);
                }
            }
        }
    } catch (e) {
        console.error(`[k6] Exception validating created order: ${e.message}`);
    }

    sleep(1);
}