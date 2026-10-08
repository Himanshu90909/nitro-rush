# Development Status

Honest, real-time status of every module. This project does not claim anything
that is not implemented. Last updated: 2026-10-08.

## Status overview

| Module | State | Notes |
|---|---|---|
| Unity — Core (state machine, events, pooling) | ✅ Code complete | Syntax-clean; NOT yet compiled in Unity Editor (scene/prefab authoring pending) |
| Unity — Cars (physics, nitro, drift, damage, audio, VFX) | ✅ Code complete | Arcade physics with Vector3.Dot math, comments explain formulas |
| Unity — Racing (checkpoints, laps, race FSM, validation) | ✅ Code complete | Anti-cheat mirrors server rules |
| Unity — AI (A* waypoint graph, heap, decision system) | ✅ Code complete | Pure-C# core is EditMode-testable |
| Unity — Multiplayer (WebSocket client, interpolation, prediction) | ✅ Code complete | 20Hz input send, snapshot interpolation |
| Unity — UI, Inventory, Progression, LiveService, Analytics | ✅ Code complete | Code-driven UGUI controllers |
| Unity — Tests | ✅ Code complete | Requires Unity Test Framework to execute |
| Backend — common (API envelope, errors, rate limit, logging) | ✅ Code complete | Awaited: business packages |
| Backend — auth | ⚠️ Partial | User/Role entities done; JWT service, controllers, SecurityConfig, DTOs pending |
| Backend — player, garage, inventory, rewards | ❌ Not started | |
| Backend — racing, leaderboard, matchmaking, events, analytics | ❌ Not started | |
| Backend — WebSocket netcode (config package) | ✅ Code complete | Server-authoritative broadcast handler, JWT handshake validation |
| Backend — tests | ❌ Not started | CI pipeline ready for them |
| Web player dashboard | ⚠️ ~70% | API layer, components, 5 pages done; App/main entry, Events, Profile pages pending |
| Admin dashboard | ❌ Not started | |
| Infrastructure (Docker, compose, Prometheus, Grafana) | ✅ Code complete | Not yet run on a machine — needs `docker compose up` verification |
| CI/CD (GitHub Actions) | ✅ Code complete | Backend Maven verify, web builds, Unity static check |
| Documentation suite | ✅ Mostly complete | README, ARCHITECTURE, API, DATABASE, AI, MULTIPLAYER, PERFORMANCE done; SECURITY/CONTRIBUTING pending |

## Verification pipeline

- **Backend**: GitHub Actions runs `mvn -B verify` on every push to `backend/**`.
  The Java code must compile and tests must pass there — CI is the verifier.
- **Web dashboards**: CI runs `npm ci && npm run build` (TypeScript strict).
- **Unity**: CI runs a lightweight static check (brace balance, namespace
  presence, no TODOs). A real Unity compile requires the Unity Editor with a
  license; GameCI integration is on the roadmap.

## Known limitations (honest)

1. Unity scenes, prefabs, Addressables groups, and ScriptableObject assets are
   not authored yet — the scripts are build-ready but the project needs the
   Editor step to become a playable build.
2. No performance benchmark numbers exist yet. `PERFORMANCE.md` documents the
   methodology; results will be measured, never fabricated.
3. The backend has never been executed locally (no JDK in the build sandbox);
   CI will produce the first real test run.
