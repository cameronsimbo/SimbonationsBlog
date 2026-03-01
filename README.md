# LearnFlow — AI-evaluated spaced repetition learning platform

An adaptive learning platform that combines FSRS spaced repetition scheduling, AI-powered answer grading (Claude / Ollama), and gamification (XP, streaks, leaderboards) to help users master structured topics through short, focused daily sessions.

---

## Architecture

| Project | Description |
|---------|-------------|
| `src/Learn.Domain` | Pure domain entities, enums, value objects, and domain services (no external dependencies) |
| `src/Learn.Application` | MediatR command/query handlers, FluentValidation pipeline, interfaces, and service models |
| `src/Learn.Infrastructure` | EF Core DbContext, repository implementations, AI evaluation services (Claude + Ollama) |
| `src/Learn.WebAPI` | ASP.NET Core 8 thin controllers — all logic delegated to MediatR |
| `mobile/` | React Native 0.83 / Expo Router — iOS & Android client |

---

## Learning Algorithms

### FSRS-Lite Spaced Repetition (`ReviewItem.RecordReview()`)

When a user fails an exercise (`score < 70`), a `ReviewItem` is created for that exercise. On subsequent sessions, due review items are surfaced and, when answered, `RecordReview()` is called to update three FSRS parameters:

- **Stability** — how long the memory will hold before decay. Increases with each successful review.
- **Difficulty** — how hard the item is for this user (0–1). Increases on failure, decreases on success.
- **Retrievability** — the probability of recalling the item right now, calculated from stability and elapsed days.

A `ReviewItem` is never deleted; it simply becomes due again in the future once retrievability falls below threshold.

### Session Mix (`SessionEngine.CalculateSessionMix()`)

Every session is exactly **5 exercises**, mixed as follows:

| Slot type | Count | Condition |
|-----------|-------|-----------|
| New exercises | 3–4 | Always |
| Spaced-repetition reviews | up to 30% (≤1) | Only if due review items exist |
| Interleaved past-lesson exercise | 1 | Only if past lesson exercises are available |

### XP and Streak Bonuses (`XPCalculator.Calculate()`)

| Component | Value |
|-----------|-------|
| Base XP | `score / 100 × 10` |
| Perfect score bonus | `+5` (score = 100 only) |
| Streak bonus | `+2 per day streak`, capped at `+10` |
| Session completion bonus | `+10` (avg ≥ 90%), `+5` (avg ≥ 70%) |

---

## Quick Start (Docker — recommended)

```bash
docker compose up
```

- API: `http://localhost:5000`
- Swagger UI: `http://localhost:5000/swagger`

> **Note:** `ANTHROPIC_API_KEY` is optional. Without it, the system falls back to the bundled Ollama (`llama3.2:3b`) container for AI grading. Set it for faster, higher-quality grading:
> ```bash
> ANTHROPIC_API_KEY=sk-ant-... docker compose up
> ```

---

## Mobile Setup

Requires the backend running at `localhost:5000`.

```bash
cd mobile
npx expo start
```

Scan the QR code with Expo Go (iOS / Android) or press `i`/`a` for simulators.

---

## Manual Backend Setup

**Prerequisites**
- .NET 8 SDK
- SQL Server (LocalDB, Docker, or Azure SQL)

```bash
dotnet restore LearnApp.sln
dotnet run --project src/Learn.WebAPI
```

Apply migrations on first run (the API applies them automatically on startup in Development mode).

---

## Configuration Reference

| Variable | Description | Default |
|----------|-------------|---------|
| `ANTHROPIC_API_KEY` | Claude API key for AI grading (optional) | *(none — Ollama fallback)* |
| `ConnectionStrings__DefaultConnection` | SQL Server connection string | LocalDB |
| `Jwt__Key` | JWT signing secret (min 32 chars) | *(required)* |
| `DailySubmissionLimit` | Max exercise submissions per user per day | `50` |

---

## API Overview

| Controller | Base route | Key endpoints |
|------------|-----------|---------------|
| `AuthController` | `/api/Auth` | `POST /register`, `POST /login`, `POST /google` |
| `TopicsController` | `/api/Topics` | `GET /`, `GET /{id}`, `POST /{id}/enroll`, `GET /{id}/path` |
| `LessonsController` | `/api/Lessons` | `GET ?unitId=` |
| `ExercisesController` | `/api/Exercises` | `GET ?lessonId=`, `POST /submit`, `POST /generate`, `POST /{id}/vote` |
| `SessionsController` | `/api/Sessions` | `POST /start`, `POST /complete` |
| `StreaksController` | `/api/Streaks` | `GET /me` |
| `LeaderboardsController` | `/api/Leaderboards` | `GET /weekly` |
| `QuestionBankController` | `/api/QuestionBank` | `GET /mine`, `POST /` |
| `DashboardController` | `/api/Dashboard` | `GET /` |
| `ProfileController` | `/api/Profile` | `GET /me`, `PUT /me`, `POST /test-ai` |

---

## Testing

```bash
dotnet test LearnApp.sln
```
