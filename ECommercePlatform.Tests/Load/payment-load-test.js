import http from 'k6/http';
import { check, sleep } from 'k6';

const BASE_URL = __ENV.PAYMENT_SERVICE_URL || __ENV.BASE_URL || 'http://host.docker.internal:5005';
const PAYMENT_PATH = __ENV.PAYMENT_PATH || '/api/payment/pay';
const ORDERS_PATH = __ENV.ORDERS_PATH || '/api/orders';
const PRODUCTS_PATH = __ENV.PRODUCTS_PATH || '/api/products';
const PRODUCT_SERVICE_URL = __ENV.PRODUCT_SERVICE_URL || 'http://host.docker.internal:5002';
const ORDER_SERVICE_URL = __ENV.ORDER_SERVICE_URL || 'http://host.docker.internal:5004';
const IDENTITY_BASE_URL = __ENV.IDENTITY_SERVICE_URL || 'http://host.docker.internal:5001';
const IDENTITY_LOGIN = __ENV.IDENTITY_LOGIN_PATH || '/api/identity/login';

const DEBUG = (__ENV.DEBUG || 'false').toLowerCase() === 'true';
const SLOW_REQUEST_MS = Number(__ENV.SLOW_REQUEST_MS || 1000);

function log(msg, data) {
    if (DEBUG) {
        console.log(`[VU ${__VU}] [ITER ${__ITER}] ${msg}`);
        if (data) console.log(JSON.stringify(data, null, 2));
    }
}

function logError(msg, res, err) {
    console.error(`[VU ${__VU}] [ITER ${__ITER}] ERROR: ${msg}`);
    if (res) {
        console.error(JSON.stringify({
            status: res.status,
            body: res.body,
            timings: res.timings
        }, null, 2));
    }
    if (err) console.error(err);
}

function logSlow(name, res) {
    if (res && res.timings && res.timings.duration > SLOW_REQUEST_MS) {
        console.warn(`[VU ${__VU}] Slow ${name}: ${res.timings.duration}ms`);
    }
}

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

    // LOGIN
    try {
        log('Login attempt');

        const loginRes = http.post(
            `${IDENTITY_BASE_URL}${IDENTITY_LOGIN}`,
            JSON.stringify({
                Email: 'loadtest+95229640@example.com',
                Password: 'StrongPass123!'
            }),
            { headers: { 'Content-Type': 'application/json' } }
        );

        logSlow('login', loginRes);

        if (loginRes && loginRes.status === 200) {
            const auth = JSON.parse(loginRes.body || '{}');

            if (auth && auth.token) {
                params.headers.Authorization = `Bearer ${auth.token}`;
                log('Login success');
            } else {
                logError('Login missing token', loginRes);
            }
        } else {
            logError('Login failed', loginRes);
        }
    } catch (e) {
        logError('Login exception', null, e);
    }

    let orderId = __ENV.TEST_ORDER_ID;

    if (!orderId) {
        try {
            log('Fetching products');

            const prodRes = http.get(`${PRODUCT_SERVICE_URL}${PRODUCTS_PATH}`);

            logSlow('get-products', prodRes);

            if (prodRes && prodRes.status === 200) {
                const list = JSON.parse(prodRes.body || '[]');
                const productId = Array.isArray(list) && list.length > 0
                    ? (list[0].id || list[0].productId)
                    : null;

                if (productId) {
                    log('Creating order');

                    const orderPayload = JSON.stringify({
                        customerId: __ENV.TEST_CUSTOMER_ID || '923079b5-d98d-4acf-c2ad-08deb8b30e07',
                        productId: "4a5eeb96-b40b-4aff-b003-601297565fac",
                        quantity: 1,
                        productVariantId: "4c5782c1-db34-43be-8500-3986496352e7",
                        productName: "Dell",
                        price: 1800,
                        currency: "EUR",
                        quantity: 1
                    });

                    const orderRes = http.post(`${ORDER_SERVICE_URL}${ORDERS_PATH}`, orderPayload, params);

                    logSlow('create-order', orderRes);

                    if (orderRes && orderRes.status >= 200 && orderRes.status < 300) {
                        const created = JSON.parse(orderRes.body || '{}');
                        orderId = created.id || created.orderId || created.orderID;
                    } else {
                        logError('Order creation failed', orderRes);
                    }

                    const orderAddress = http.post(
                        `${ORDER_SERVICE_URL}/${orderId}/address`,
                        JSON.stringify({
                            street: "Ivan Vazov",
                            city: "Petrich",
                            state: "Petrich",
                            zipCode: "2800",
                            country: "BG"
                        }),
                        params
                    );

                    logSlow('order-address', orderAddress);

                    const orderFinalize = http.post(
                        `${ORDER_SERVICE_URL}/${orderId}/finalize`,
                        JSON.stringify({}),
                        params
                    );

                    logSlow('order-finalize', orderFinalize);
                }
            } else {
                logError('Product fetch failed', prodRes);
            }
        } catch (e) {
            logError('Order flow exception', null, e);
        }
    }

    if (!orderId) {
        orderId = __ENV.TEST_ORDER_ID || '87581EC4-C9FA-4AE9-94AC-001F3EAE91D9';
    }

    const payload = JSON.stringify({
        paymentId: "3944B705-2083-43B5-A961-4049A97FABBA",
        cardNumber: "4539128745631029",
        cardHolder: "Alex Johnson",
        expiry: "08/27",
        cvv: "482"
    });

    const url = `${BASE_URL}${PAYMENT_PATH}`;

    log('Payment request');

    const res = http.post(url, payload, params);

    logSlow('payment', res);

    const ok = check(res, {
        'payment status is 2xx': (r) => r.status >= 200 && r.status < 300
    });

    if (!ok) {
        logError('Payment failed', res);
    } else {
        log('Payment success');
    }

    sleep(1);
}