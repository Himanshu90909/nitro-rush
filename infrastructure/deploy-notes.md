# NITRO RUSH — Local Infrastructure & Deployment Guide

This document explains how to deploy and run the **NITRO RUSH** backend stack locally using Docker Compose, how to access monitoring dashboards, and how to verify system health.

---

## 🚀 Quickstart Guide

### 1. Prerequisites
Ensure you have the following installed on your machine:
- [Docker Engine](https://docs.docker.com/get-docker/) (v20.10 or higher)
- [Docker Compose](https://docs.docker.com/compose/install/) (v2.0 or higher)

### 2. Environment Setup
Copy the example environment file to `.env` inside the `infrastructure` directory:

```bash
cd infrastructure
cp .env.example .env
```

Review `.env` and adjust passwords if needed:
- `JWT_SECRET`: Must be a secret string of at least 32 characters.
- `POSTGRES_PASSWORD`: Database user password for user `nitro`.
- `GRAFANA_ADMIN_PASSWORD`: Admin login password for Grafana UI.

---

## 🛠️ Running the Service Stack

Start all services (PostgreSQL, Redis, Spring Boot Backend, Prometheus, Grafana) in detached mode:

```bash
docker compose up -d
```

### Checking Stack Status

Verify that all containers are running and healthy:

```bash
docker compose ps
```

You should see 5 healthy containers:
- `nitro-rush-backend` (Port 8080)
- `nitro-rush-postgres` (Port 5432)
- `nitro-rush-redis` (Port 6379)
- `nitro-rush-prometheus` (Port 9090)
- `nitro-rush-grafana` (Port 3000)

View live logs for the backend:

```bash
docker compose logs -f backend
```

To shut down the stack (preserving data volumes):

```bash
docker compose down
```

To shut down and wipe persistent database and redis volumes:

```bash
docker compose down -v
```

---

## 🌐 Exposed Services & Endpoints

| Service | Endpoint / URL | Default Credentials / Auth | Description |
| :--- | :--- | :--- | :--- |
| **Backend REST API** | `http://localhost:8080` | JWT Bearer Token | Main game API gateway & endpoints |
| **Backend Health Check** | `http://localhost:8080/actuator/health` | None | Spring Boot Actuator status |
| **Prometheus Metrics** | `http://localhost:8080/actuator/prometheus` | None | Scrape endpoint for metrics |
| **Prometheus Server** | `http://localhost:9090` | None | Metric collection & PromQL console |
| **Grafana Dashboards** | `http://localhost:3000` | User: `admin`<br>Pass: `${GRAFANA_ADMIN_PASSWORD}` | Pre-configured metrics visualization |

---

## 🔑 Seeded Demo Account

The database migration scripts initialize a pre-configured demo account for testing:

- **Username / Email**: `player@nitrorush.game`
- **Password**: `NitroRush2026!`
- **Initial State**:
  - Currency: 10,000 Soft Credits, 50 Hard Gems
  - Garage: Starter Car (ID: `car_apex_racer_v1`)
  - Level: 1 (XP: 0)

To authenticate via cURL:

```bash
curl -X POST http://localhost:8080/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "player@nitrorush.game",
    "password": "NitroRush2026!"
  }'
```

---

## 📊 Monitoring & Observability

1. Navigate to Grafana at `http://localhost:3000`.
2. Log in with `admin` and the password set in your `.env`.
3. Open **Dashboards → Nitro Rush Backend Metrics**.
4. Observe real-time panels:
   - **API Request Rate**: HTTP throughput per endpoint.
   - **p95 Latency**: 95th percentile request latencies.
   - **JVM Memory Usage**: Heap memory utilization.
   - **WebSocket Active Connections**: Real-time connected multiplayer sessions (`/ws/race`).
   - **Leaderboard Submissions Rate**: Telemetry and race submission velocity.
