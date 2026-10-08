# NITRO RUSH - Player Web Dashboard

Live-service player dashboard for NITRO RUSH multiplayer arcade racing game. Built with Vite, React 18, TypeScript, and Tailwind CSS.

## Features

- **Auth System**: Registration, JWT login, auto token refresh, protected routes.
- **Player Profile**: Level & XP progress tracking, Credits, Tokens, stats, and profile editing.
- **Garage**: View all cars, purchase new vehicles, upgrade owned cars (Engine, Turbo, Tires, Brakes, Nitro levels 1-5).
- **Leaderboards**: Global top 50 rankings, nearby competitors, pinned self-rank.
- **Live Events**: Real-time event countdown timers, objective tracking, join events, and claim rewards.
- **Analytics & Stats**: Personal performance summary from live REST endpoints.

## Quickstart

```bash
# Install dependencies
npm install

# Start development server
npm run dev

# Build for production
npm run build
```

## Environment Variables

Copy `.env.example` to `.env`:
```env
VITE_API_URL=http://localhost:8080
```
