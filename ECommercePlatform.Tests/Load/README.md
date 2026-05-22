Load tests (k6)
=================

Quick guide to run the k6 load tests locally or in Docker.

Environment variables:
- `BASE_URL` - base URL for the API. Defaults to `http://host.docker.internal:*` for local dev or use `http://api-gateway:8080` in Docker.
- `K6_VUS` - number of virtual users (default `50`).
- `K6_DURATION` - test duration (default `1m`).

Examples:

Run a test locally with k6 installed:
```bash
k6 run product-load-test.js
```

Run using Docker Compose (from repo root) so k6 joins the same network as the services:
```bash
BASE_URL=http://api-gateway:8080 K6_VUS=100 K6_DURATION=2m docker-compose -f docker-compose.yml -f docker-compose.load.yml up --abort-on-container-exit
```

Files:
- `product-load-test.js` - catalog/product GET test
- `order-load-test.js` - create order POST test
- `identity-load-test.js` - register user POST test (exercises registration endpoint)
- `inventory-load-test.js` - inventory GET test
- `payment-load-test.js` - payment POST test

Tips:
- Customize paths using env vars like `PRODUCTS_PATH`, `ORDERS_PATH`, `PAYMENT_PATH`, etc.
- When running in Docker, set `BASE_URL` to the API gateway (`http://api-gateway:8080`) so tests go through Ocelot.
