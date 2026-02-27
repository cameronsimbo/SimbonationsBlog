# PRD: AI-Evaluated Learning Platform ("LearnFlow")

## Introduction

Convert the existing SimbonationsBlog .NET template into a production-ready backend for a Duolingo-inspired learning application. The core differentiator: instead of fixed correct answers, an Azure OpenAI integration evaluates user responses dynamically — enabling learning across **any subject domain** (languages, history, CFA exam prep, etc.).

The existing CLEAN Architecture, CQRS/MediatR pipeline, FluentValidation, EF Core infrastructure, and testing patterns are preserved. The Next.js frontend is replaced by a React Native / Expo mobile client. ASP.NET Identity + JWT handles authentication.

**Key decisions from discussion:**
- **AI Provider:** Azure OpenAI (abstracted behind interface)
- **Mobile Client:** React Native / Expo (Android + iOS)
- **Auth:** ASP.NET Identity + JWT
- **Scope:** Full-featured — XP, streaks, leaderboards, achievements
- **Content:** Hybrid — AI pre-generates exercises nightly + users maintain personal question banks
- **UI Language:** English only (initially)
- **Subjects:** Any domain — languages, history, finance, science, etc. Users simply pick a topic they like
- **Exercise Types:** Translation (both directions), free-text writing, reading comprehension, general knowledge explanation, audio/speaking input
- **Leaderboards:** Global only
- **Exercise Generation:** Pre-generated (nightly batch job); users collect their daily lesson with fresh exercises
- **Daily Limits:** Per-user daily submission cap to control AI costs

---

## Goals

- Preserve all existing architectural patterns (CLEAN, CQRS, MediatR, FluentValidation pipeline, thin controllers, entity hierarchy)
- Replace blog domain entities with learning-platform domain entities
- Integrate Azure OpenAI for dynamic answer evaluation behind a clean abstraction layer (`IAIEvaluationService`)
- Support multiple subject domains (not just languages) with a single flexible exercise model
- Implement gamification: XP, daily streaks with freeze, global weekly leaderboards, achievement badges
- Deliver a working end-to-end lesson flow: pick a topic → collect daily lesson → answer exercise → AI evaluates → score + feedback → progress updated
- Include audio/speaking input as an exercise type with speech-to-text processing
- Pre-generate exercises nightly so users receive fresh daily lessons
- Enforce per-user daily submission limits to manage AI API costs
- Provide a React Native / Expo mobile client with Duolingo-like interaction patterns
- Maintain comprehensive test coverage: unit tests (domain + application), integration tests (WebApplicationFactory)

---

## User Stories

### US-001: Rename and restructure solution from Blog to Learn
**Description:** As a developer, I want the solution renamed from `Blog.*` to `Learn.*` so the codebase reflects the new domain.

**Acceptance Criteria:**
- [ ] All projects renamed: `Learn.Domain`, `Learn.Application`, `Learn.Infrastructure`, `Learn.WebAPI`, `Learn.Application.Tests`, `Learn.Domain.Tests`, `Learn.IntegrationTests`
- [ ] Solution file updated (`LearnApp.slnx`)
- [ ] All blog-related domain entities, handlers, controllers, and tests removed
- [ ] Project references and namespaces updated
- [ ] `docker-compose.yml` and Dockerfiles updated
- [ ] `.editorconfig` and `.github/copilot-instructions.md` preserved
- [ ] Solution builds and all (empty) test projects pass

---

### US-002: Implement core domain entities
**Description:** As a developer, I need the domain model for the learning platform so all features have a solid foundation.

**Acceptance Criteria:**
- [ ] `BaseEntity<T>` and `CreatedEntity<T>` hierarchy preserved from template
- [ ] New entities created: `User`, `Topic`, `Unit`, `Lesson`, `Exercise`, `ExerciseAttempt`, `QuestionBankItem`, `UserProgress`, `UserStreak`, `Achievement`, `UserAchievement`, `LeaderboardEntry`, `DailyLesson`, `UserDailyLimit`
- [ ] Enums created: `ExerciseType`, `SubjectDomain`, `AttemptStatus`, `AchievementType`, `DifficultyLevel`
- [ ] Domain factory methods on entities (e.g. `Topic.Create(...)`, `Exercise.Create(...)`)
- [ ] No external dependencies in Domain project
- [ ] Domain tests pass for all entity factories and business logic methods
- [ ] Typecheck passes

---

### US-003: Implement User entity with ASP.NET Identity
**Description:** As a user, I want to register and log in so my progress is saved across devices.

**Acceptance Criteria:**
- [ ] `User` entity extends `IdentityUser` and adds `ICreatedEntity` interface
- [ ] Properties: `DisplayName`, `AvatarUrl`, `TotalXP`, `CurrentStreak`, `LongestStreak`, `DailySubmissionsUsed`, `DailySubmissionResetDate`
- [ ] `LearnDbContext` extends `IdentityDbContext<User>`
- [ ] JWT bearer auth configured in `Program.cs`
- [ ] Registration endpoint: `POST /api/auth/register` (email, password, display name)
- [ ] Login endpoint: `POST /api/auth/login` → returns JWT token + refresh token
- [ ] Refresh token endpoint: `POST /api/auth/refresh`
- [ ] `ICurrentUser` interface implemented via `HttpContext.User` claims
- [ ] Password requirements: minimum 8 chars, 1 uppercase, 1 digit
- [ ] Typecheck passes
- [ ] Unit tests for auth command handlers

---

### US-004: Implement Topic selection and browsing
**Description:** As a learner, I want to pick a topic I'm interested in so I can start learning immediately.

**Acceptance Criteria:**
- [ ] `Topic` entity: `Name`, `Description`, `SubjectDomain` (enum), `IconUrl`, `DifficultyLevel`, `IsPublished`, `TotalUnits`
- [ ] `SubjectDomain` enum: `Language`, `History`, `Finance`, `Science`, `Technology`, `General`
- [ ] Users browse and select topics — no enrollment required, just pick and go
- [ ] `Unit` entity: `TopicId` (FK), `Name`, `Description`, `OrderIndex`, `TotalLessons`
- [ ] CQRS operations following existing folder pattern:
  - `Topics/GetAll/` (filterable by `SubjectDomain`), `Topics/Get/`, `Topics/Create/`, `Topics/Update/`, `Topics/Delete/`
  - `Units/GetByTopic/`, `Units/Get/`, `Units/Upsert/`, `Units/Delete/`
- [ ] `TopicsController` and `UnitsController` (thin, `Mediator.Send()` only)
- [ ] FluentValidation validators for Create/Update commands
- [ ] EF Core configurations in separate `IEntityTypeConfiguration<T>` files
- [ ] Typecheck passes
- [ ] Unit tests for handlers and validators

---

### US-005: Implement Lesson and Exercise entities
**Description:** As a learner, I want structured lessons with various exercise types so I can practice effectively.

**Acceptance Criteria:**
- [ ] `Lesson` entity: `UnitId` (FK), `Name`, `Description`, `OrderIndex`, `ExerciseCount`, `EstimatedMinutes`
- [ ] `Exercise` entity: `LessonId` (FK), `OrderIndex`, `ExerciseType` (enum), `DifficultyLevel`, `Prompt`, `Context` (nullable — reading passage, additional info), `ReferenceAnswer` (what AI uses as grading rubric), `AudioUrl` (nullable — for listening/speaking exercises), `Hints` (JSON array), `MaxScore`, `QuestionBankItemId` (nullable FK — links to personal question bank item)
- [ ] `ExerciseType` enum: `TranslateToTarget`, `TranslateToSource`, `FreeTextResponse`, `ReadingComprehension`, `Explanation`, `ListeningComprehension`, `SpeakingResponse`
- [ ] `DifficultyLevel` enum: `Beginner`, `Intermediate`, `Advanced`
- [ ] CQRS: `Lessons/GetByUnit/`, `Lessons/Get/`, `Lessons/Upsert/`, `Lessons/Delete/`
- [ ] CQRS: `Exercises/GetByLesson/`, `Exercises/Get/`, `Exercises/Upsert/`, `Exercises/Delete/`
- [ ] `LessonsController` and `ExercisesController`
- [ ] Validators ensure exercises have valid type-specific data (e.g. `AudioUrl` required for listening/speaking types)
- [ ] Typecheck passes
- [ ] Unit tests for handlers and validators

---

### US-006: Implement audio/speaking exercise support
**Description:** As a learner, I want to listen to audio prompts and record spoken answers so I can practice pronunciation and listening skills.

**Acceptance Criteria:**
- [ ] `ISpeechService` interface in Application layer:
  - `Task<string> TranscribeAudioAsync(Stream audioStream, string language, CancellationToken ct)` — speech-to-text
  - `Task<Stream> SynthesizeSpeechAsync(string text, string language, CancellationToken ct)` — text-to-speech for prompts
- [ ] `AzureSpeechService` implementation in Infrastructure layer (Azure Cognitive Services Speech SDK)
- [ ] Configurable via `appsettings.json`: `AzureSpeech:SubscriptionKey`, `AzureSpeech:Region`
- [ ] `ListeningComprehension` exercise: audio prompt played → user types answer → AI evaluates text
- [ ] `SpeakingResponse` exercise: text prompt shown → user records audio → speech-to-text → AI evaluates transcription
- [ ] Audio upload endpoint: `POST /api/exercises/{id}/submit-audio` (multipart form data)
- [ ] Handler flow: receive audio → transcribe via `ISpeechService` → evaluate transcription via `IAIEvaluationService` → return score + feedback
- [ ] Typecheck passes
- [ ] Unit tests with mocked `ISpeechService`

---

### US-007: Implement Personal Question Bank
**Description:** As a learner, I want to add my own questions to a personal question bank so I can customize my learning and create exercises on topics I care about.

**Acceptance Criteria:**
- [ ] `QuestionBankItem` entity: `UserId` (FK — owner), `SubjectDomain`, `ExerciseType`, `DifficultyLevel`, `Prompt`, `Context`, `ReferenceAnswer`, `Hints`, `UsageCount`
- [ ] Question bank items are **private to the user** who created them — not shared or community-moderated
- [ ] No approval workflow needed — items are immediately usable by their owner
- [ ] CQRS: `QuestionBank/GetMine/` (with filters by domain, type, difficulty), `QuestionBank/Get/`, `QuestionBank/Create/`, `QuestionBank/Update/`, `QuestionBank/Delete/`
- [ ] `QuestionBankController` — all endpoints scoped to the authenticated user
- [ ] Only the owner can view/update/delete their items
- [ ] Validators for content quality (non-empty prompt, reference answer required)
- [ ] Personal question bank items can be used as seeds for AI exercise generation in daily lessons
- [ ] Typecheck passes
- [ ] Unit tests

---

### US-008: Implement Azure OpenAI integration for answer evaluation
**Description:** As a learner, I want my free-text answers evaluated by AI so I get meaningful, context-aware feedback.

**Acceptance Criteria:**
- [ ] `IAIEvaluationService` interface in Application layer:
  - `Task<AIEvaluationResult> EvaluateAnswerAsync(EvaluationRequest request, CancellationToken ct)`
  - `Task<List<GeneratedExercise>> GenerateExercisesAsync(ExerciseGenerationRequest request, CancellationToken ct)`
- [ ] `AIEvaluationResult` model: `Score` (0–100), `IsPassing` (bool), `Feedback` (string), `SuggestedCorrection` (nullable), `DetailedBreakdown` (nullable — accuracy, grammar, relevance sub-scores)
- [ ] `EvaluationRequest` model: `UserAnswer`, `ExerciseType`, `Prompt`, `ReferenceAnswer`, `SubjectDomain`, `DifficultyLevel`
- [ ] `AzureOpenAIEvaluationService` implementation in Infrastructure layer
- [ ] Configurable via `appsettings.json` / environment variables: `AzureOpenAI:Endpoint`, `AzureOpenAI:ApiKey`, `AzureOpenAI:DeploymentName`, `AzureOpenAI:ModelVersion`
- [ ] Prompt engineering: different system prompts per `ExerciseType` and `SubjectDomain`
- [ ] Retry policy with Polly for transient failures
- [ ] Response parsed into strongly-typed `AIEvaluationResult`
- [ ] Typecheck passes
- [ ] Unit tests with mocked `IAIEvaluationService`
- [ ] Integration test verifying DI registration and configuration binding

---

### US-009: Implement exercise attempt flow with daily submission limit
**Description:** As a learner, I want to submit my answer and immediately receive AI-generated feedback, within my daily submission allowance.

**Acceptance Criteria:**
- [ ] `ExerciseAttempt` entity: `ExerciseId` (FK), `UserId` (FK), `UserAnswer`, `Score`, `IsPassing`, `Feedback`, `SuggestedCorrection`, `DetailedBreakdown` (JSON), `AttemptNumber`, `TimeTakenSeconds`, `IsAudioSubmission`
- [ ] `SubmitAnswerCommand`: accepts `ExerciseId`, `UserAnswer`, `TimeTakenSeconds`
- [ ] Handler flow: check daily limit → load exercise → call `IAIEvaluationService.EvaluateAnswerAsync()` → persist `ExerciseAttempt` → award XP → decrement daily submissions remaining → return result
- [ ] **Daily submission limit**: configurable per environment (default: 50/day), tracked on `User` entity (`DailySubmissionsUsed`, `DailySubmissionResetDate`)
- [ ] Limit resets at 00:00 UTC daily
- [ ] When limit reached, return 429 Too Many Requests with remaining reset time
- [ ] `GET /api/users/me/daily-limit` — returns submissions used, remaining, reset time
- [ ] `SubmitAnswerCommandHandler` returns `ExerciseAttemptResultVm` with score, feedback, XP earned, daily submissions remaining, and whether the lesson is complete
- [ ] `POST /api/exercises/{id}/submit` endpoint
- [ ] Validator ensures non-empty answer and valid exercise ID
- [ ] Typecheck passes
- [ ] Unit tests: handler with mocked AI service, daily limit enforcement, validator tests
- [ ] Integration test for full submit flow

---

### US-010: Implement nightly exercise pre-generation (Daily Lessons)
**Description:** As the system, I want to pre-generate exercises overnight so users receive fresh daily lessons without waiting for AI.

**Acceptance Criteria:**
- [ ] `DailyLesson` entity: `UserId` (FK), `TopicId` (FK), `LessonDate` (date), `IsCollected` (bool), `Exercises` (navigation — generated exercises linked to this daily lesson)
- [ ] `IExerciseGenerationService` interface in Application layer with `GenerateDailyLessonsAsync(CancellationToken ct)` — generates lessons for all active users
- [ ] Generation logic:
  - For each user with active topics, generate a daily lesson with 5-10 exercises
  - Pull from user's personal question bank items (if any) as seeds
  - Mix exercise types appropriate to the topic's `SubjectDomain`
  - Vary difficulty based on user's recent performance
- [ ] `GenerateDailyLessonsCommand` — can be triggered by a scheduled job or manual API call
- [ ] `POST /api/admin/generate-daily-lessons` — admin-only endpoint to trigger generation
- [ ] Background job approach: `IHostedService` with configurable schedule (default: 02:00 UTC nightly)
- [ ] `GET /api/daily-lessons/today` — user collects their daily lesson
- [ ] `GET /api/daily-lessons/history` — user's past daily lessons
- [ ] Generated exercises are persisted with `QuestionBankItemId` if derived from a personal bank item
- [ ] Rate limiting on manual generation trigger
- [ ] Typecheck passes
- [ ] Unit tests with mocked AI service

---

### US-011: Implement XP and progress tracking
**Description:** As a learner, I want to earn XP and see my progress through topics so I stay motivated.

**Acceptance Criteria:**
- [ ] `UserProgress` entity: `UserId` (FK), `TopicId` (FK), `LessonId` (FK), `ExercisesCompleted`, `ExercisesTotal`, `BestScore`, `AverageScore`, `IsCompleted`, `CompletedDate`
- [ ] XP calculation: base XP per exercise (10) × score multiplier (score/100) + bonus for perfect score (+5) + bonus for streak (+2 per day in streak, max +10)
- [ ] `User.TotalXP` updated atomically when XP earned
- [ ] CQRS: `Progress/GetByUser/` (all topic progress), `Progress/GetByTopic/` (detailed unit/lesson progress)
- [ ] `ProgressController` with endpoints
- [ ] Lesson marked complete when all exercises have a passing attempt
- [ ] Unit marked complete when all lessons complete
- [ ] Topic marked complete when all units complete
- [ ] Typecheck passes
- [ ] Unit tests for XP calculation logic
- [ ] Domain tests for progress state transitions

---

### US-012: Implement daily streak system
**Description:** As a learner, I want my daily practice streak tracked with the option to use a freeze so I don't lose progress.

**Acceptance Criteria:**
- [ ] `UserStreak` entity: `UserId` (FK), `CurrentStreak`, `LongestStreak`, `LastActivityDate`, `StreakFreezeCount` (default 2), `StreakFreezeUsedDate` (nullable)
- [ ] Streak incremented when user completes at least 1 exercise in a new day (UTC)
- [ ] Streak reset to 0 if no activity yesterday AND no freeze available
- [ ] Streak freeze auto-consumed if user missed yesterday but has freezes remaining
- [ ] `GET /api/streaks/me` — current streak info
- [ ] `POST /api/streaks/freeze` — manually activate a freeze for today
- [ ] Domain logic in `UserStreak` entity (pure methods, testable)
- [ ] Typecheck passes
- [ ] Domain tests for all streak scenarios (increment, reset, freeze consume, freeze expired)

---

### US-013: Implement global leaderboard system
**Description:** As a learner, I want to see how I rank globally against all other users.

**Acceptance Criteria:**
- [ ] `LeaderboardEntry` entity: `UserId` (FK), `WeekStartDate`, `WeeklyXP`, `Rank` (computed)
- [ ] Weekly leaderboard resets every Monday 00:00 UTC
- [ ] `GET /api/leaderboards/weekly` — top 50 users this week + requester's position
- [ ] `GET /api/leaderboards/all-time` — top 50 by `User.TotalXP` + requester's position
- [ ] Leaderboard entries created/updated when XP is earned
- [ ] CQRS query handlers with efficient SQL (window functions for ranking)
- [ ] Global scope only — no per-topic or friends leaderboards
- [ ] Typecheck passes
- [ ] Unit tests for leaderboard queries

---

### US-014: Implement achievement / badge system
**Description:** As a learner, I want to earn badges for milestones so I have long-term goals beyond daily XP.

**Acceptance Criteria:**
- [ ] `Achievement` entity: `Name`, `Description`, `IconUrl`, `AchievementType` (enum), `Threshold` (int — e.g. "Complete 10 lessons" → threshold = 10)
- [ ] `AchievementType` enum: `LessonsCompleted`, `TopicsCompleted`, `StreakDays`, `TotalXP`, `PerfectScores`, `ExercisesCompleted`, `DailyLessonsCollected`
- [ ] `UserAchievement` entity: `UserId` (FK), `AchievementId` (FK), `EarnedDate`
- [ ] Achievement evaluation runs after XP/progress updates (checked in handler or via MediatR notification)
- [ ] `GET /api/achievements` — all available achievements
- [ ] `GET /api/achievements/me` — user's earned achievements
- [ ] Seed data: ~15-20 achievements across all types
- [ ] Typecheck passes
- [ ] Unit tests for achievement evaluation logic

---

### US-015: Implement API exception handling and middleware
**Description:** As a developer, I want consistent error responses across all endpoints.

**Acceptance Criteria:**
- [ ] `ApiExceptionFilterAttribute` preserved from template — maps `ValidationException` → 400, `NotFoundException` → 404, `ForbiddenAccessException` → 403, `UnauthorizedAccessException` → 401
- [ ] Added: `DailyLimitExceededException` → 429 with `Retry-After` header
- [ ] `RequestLoggingMiddleware` preserved with Serilog structured logging
- [ ] AI service errors (timeout, rate limit) mapped to 503 with retry-after header
- [ ] All error responses follow `ProblemDetails` RFC 7807 format
- [ ] Typecheck passes

---

### US-016: Implement EF Core DbContext and configurations
**Description:** As a developer, I need the database schema properly configured with EF Core.

**Acceptance Criteria:**
- [ ] `LearnDbContext` extends `IdentityDbContext<User>` and implements `ILearnDbContext`
- [ ] `DbSet<T>` for all entities
- [ ] `SaveChangesAsync` override sets `CreatedDate`/`ModifiedDate` (preserved from template)
- [ ] `OnModelCreating` uses `ApplyConfigurationsFromAssembly` (preserved from template)
- [ ] Separate `IEntityTypeConfiguration<T>` per entity with proper indexes:
  - Unique index on `Topic.Name`
  - Composite index on `UserProgress (UserId, TopicId, LessonId)`
  - Index on `LeaderboardEntry (WeekStartDate, WeeklyXP DESC)`
  - Index on `ExerciseAttempt (UserId, ExerciseId)`
  - Index on `DailyLesson (UserId, LessonDate)`
  - Index on `QuestionBankItem (UserId, SubjectDomain)`
- [ ] Initial migration generated and tested
- [ ] Typecheck passes

---

### US-017: Seed data — topics, units, lessons, exercises, achievements
**Description:** As a developer, I need seed data so the app is usable immediately after deployment.

**Acceptance Criteria:**
- [ ] Seed at least 3 topics:
  - "Spanish for Beginners" (SubjectDomain: Language) — 3 units, 3 lessons each, 5 exercises per lesson
  - "CFA Level 1 — Ethics" (SubjectDomain: Finance) — 2 units, 3 lessons each, 5 exercises per lesson
  - "World History: Ancient Civilizations" (SubjectDomain: History) — 2 units, 3 lessons each, 5 exercises per lesson
- [ ] Seed ~15-20 achievements across all types
- [ ] Seed data applied via `HasData()` in EF configurations or a `DatabaseSeeder` service
- [ ] Seed data includes varied exercise types per topic (including audio exercise stubs)
- [ ] Typecheck passes

---

### US-018: Docker Compose for local development
**Description:** As a developer, I want a single `docker-compose up` to run the entire stack locally.

**Acceptance Criteria:**
- [ ] Services: SQL Server 2022, Learn.WebAPI
- [ ] API Dockerfile updated from template
- [ ] SQL Server volume persisted
- [ ] API waits for SQL Server health check
- [ ] Environment variables for Azure OpenAI and Azure Speech configurable via `.env` file
- [ ] Daily limit configurable via environment variable
- [ ] `README.md` updated with setup instructions
- [ ] `docker-compose up` successfully starts all services

---

### US-019: Integration tests with WebApplicationFactory
**Description:** As a developer, I want integration tests that verify the full HTTP pipeline.

**Acceptance Criteria:**
- [ ] `TestWebApplicationFactory` preserved from template — swaps DB for InMemory (or Testcontainers if upgrading)
- [ ] `IAIEvaluationService` replaced with a deterministic mock in test configuration
- [ ] `ISpeechService` replaced with a deterministic mock in test configuration
- [ ] `BaseIntegrationTest` with HTTP helpers (`GetAsync<T>`, `PostAsync<T>`, `AuthenticateAsync`)
- [ ] Test database seeded with minimal data
- [ ] Integration tests for:
  - Auth flow (register → login → access protected endpoint)
  - Topic browsing
  - Lesson retrieval
  - Exercise submission → AI evaluation → score returned
  - Daily submission limit enforcement (submit up to limit → 429 on next)
  - Streak update after exercise completion
  - Leaderboard query
  - Personal question bank CRUD
  - Daily lesson collection
- [ ] All tests pass in CI
- [ ] Typecheck passes

---

### US-020: React Native / Expo mobile client setup
**Description:** As a developer, I need the mobile project scaffolded with shared types and API client.

**Acceptance Criteria:**
- [ ] Expo project initialized in `src/Learn.Mobile/` with TypeScript, Expo Router
- [ ] `src/lib/api.ts` — typed fetch wrapper matching backend endpoints (mirrors template's `api.ts` pattern)
- [ ] `src/types/` — TypeScript interfaces matching backend VMs
- [ ] `src/stores/` — state management (Zustand or React Context) for auth token, user profile, current lesson state
- [ ] Authentication flow: register / login screens → JWT stored in secure storage → attached to API requests
- [ ] `expo-av` configured for audio playback (listening exercises) and recording (speaking exercises)
- [ ] Expo config for Android + iOS
- [ ] Typecheck passes

---

### US-021: Mobile — Topic selection and lesson flow screens
**Description:** As a learner, I want to browse topics, collect my daily lesson, and complete exercises on my phone.

**Acceptance Criteria:**
- [ ] Home screen: topic grid/list organized by `SubjectDomain`, user can just tap to start
- [ ] Daily lesson card: prominent "Collect Today's Lesson" button on home screen
- [ ] Topic detail screen: units and lessons displayed as a skill tree / progress list
- [ ] Lesson screen: sequential exercise presentation (one at a time)
- [ ] Exercise screen: prompt display, text input area (or audio record button for speaking exercises), submit button, daily submissions counter
- [ ] Feedback screen: score, AI feedback, suggested correction, "Continue" button
- [ ] Lesson complete screen: total score, XP earned, streak status, daily submissions remaining
- [ ] Navigation follows Duolingo-like flow (linear lesson progression, can't skip ahead)
- [ ] Typecheck passes

---

### US-022: Mobile — Audio exercise screens
**Description:** As a learner, I want to listen to audio prompts and record spoken answers on my phone.

**Acceptance Criteria:**
- [ ] Listening exercise screen: play button for audio prompt, text input for answer
- [ ] Speaking exercise screen: text prompt displayed, record button, playback of recording, submit button
- [ ] Audio recording uses `expo-av` with proper permissions handling (microphone access)
- [ ] Visual feedback during recording (waveform or timer)
- [ ] Recorded audio uploaded as multipart form data to `/api/exercises/{id}/submit-audio`
- [ ] Graceful fallback if microphone permission denied (show text input option)
- [ ] Typecheck passes

---

### US-023: Mobile — Progress, streaks, and gamification screens
**Description:** As a learner, I want to see my XP, streak, leaderboard position, and achievements in the app.

**Acceptance Criteria:**
- [ ] Profile screen: display name, avatar, total XP, current streak, longest streak, daily submissions remaining
- [ ] Streak widget on home screen: flame icon + day count, freeze indicator
- [ ] Leaderboard screen: weekly + all-time tabs, user's global rank highlighted
- [ ] Achievements screen: grid of badges — earned (color) / locked (grayscale)
- [ ] Progress screen per topic: unit/lesson completion visualization
- [ ] Typecheck passes

---

### US-024: Mobile — Personal Question Bank management
**Description:** As a learner, I want to add and manage my own personal question bank from the mobile app.

**Acceptance Criteria:**
- [ ] "My Questions" screen accessible from profile/settings
- [ ] Create question form: subject domain picker, exercise type, difficulty, prompt, reference answer, hints
- [ ] List of user's own items with edit/delete actions
- [ ] Items immediately usable — no approval needed (personal bank)
- [ ] Indicator showing how many times each item has been used in daily lessons
- [ ] Typecheck passes

---

## Functional Requirements

- **FR-1:** The system must authenticate users via JWT tokens issued on login/registration
- **FR-2:** The system must support multiple subject domains (Language, History, Finance, Science, Technology, General) via a `SubjectDomain` enum
- **FR-3:** Users browse topics by subject domain and simply pick what interests them — no enrollment step
- **FR-4:** Topics, units, and lessons must be hierarchically organized with ordering
- **FR-5:** Exercises must support 7 types: `TranslateToTarget`, `TranslateToSource`, `FreeTextResponse`, `ReadingComprehension`, `Explanation`, `ListeningComprehension`, `SpeakingResponse`
- **FR-6:** When a user submits an answer, the backend must send the answer + exercise context to Azure OpenAI for evaluation
- **FR-7:** The AI must return a score (0–100), pass/fail determination, textual feedback, and optional suggested correction
- **FR-8:** AI prompts must vary by exercise type and subject domain for accurate evaluation
- **FR-9:** Audio exercises use Azure Speech Services: text-to-speech for prompts, speech-to-text for user recordings
- **FR-10:** Each user has a personal question bank — items are private, immediately usable, no moderation needed
- **FR-11:** Exercises are pre-generated nightly by a background job using AI + personal question bank items as seeds
- **FR-12:** Users collect a daily lesson containing fresh exercises for each active topic
- **FR-13:** A per-user daily submission limit (configurable, default 50) controls AI API costs; returns 429 when exceeded
- **FR-14:** XP is awarded per exercise based on score, with bonuses for perfect scores and active streaks
- **FR-15:** Daily streaks increment on any exercise completion; reset after 1 missed day unless a freeze is available
- **FR-16:** Streak freezes are limited (default 2); auto-consumed when a day is missed
- **FR-17:** Global weekly leaderboards rank users by XP earned that week; reset every Monday UTC
- **FR-18:** Achievements are evaluated after progress/XP updates and awarded automatically
- **FR-19:** All CQRS commands pass through the `ValidationBehaviour` MediatR pipeline before handler execution
- **FR-20:** All controllers are thin wrappers over `Mediator.Send()` — zero business logic in controllers
- **FR-21:** AI service access must be configurable via environment variables / appsettings and abstracted behind `IAIEvaluationService`
- **FR-22:** Speech service access must be configurable and abstracted behind `ISpeechService`
- **FR-23:** The mobile client must work on both Android and iOS via React Native / Expo

---

## Non-Goals (Out of Scope)

- **No real-time multiplayer** — leaderboards are async, no live competition
- **No payment / subscription system** — all content is free
- **No social features** — no friends list, no sharing, no messaging
- **No per-topic or friends leaderboards** — global only for now
- **No offline mode** — mobile app requires internet (AI evaluation is server-side)
- **No admin web panel** — content management via API endpoints + mobile app only
- **No push notifications** — streak reminders are out of scope
- **No localization** — English UI only
- **No Duolingo branding replication** — UI patterns inspired by Duolingo but visuals are original
- **No CI/CD pipeline** — infrastructure/DevOps deferred to a later phase
- **No community/shared question bank** — question banks are personal only
- **No question bank moderation** — items are private, no approval workflow

---

## Design Considerations

- **Mobile UI** should follow Duolingo's interaction patterns: linear lesson flow, immediate feedback after each exercise, progress visualization as skill tree or progress bars
- **Daily lesson collection** should feel rewarding — prominent card on home screen, animation on collect
- **Exercise screen** should be clean and focused: one exercise at a time, large text input (or audio controls), prominent submit button, daily submissions counter visible
- **Audio exercises** need clear visual states: idle → recording → processing → result
- **Feedback screen** should use color coding: green for pass, orange for partial, red for fail
- **Daily limit indicator** visible but not intrusive — shows remaining submissions
- **Reuse from template**: `ApiExceptionFilterAttribute`, `ValidationBehaviour`, `ApiControllerBase`, `RequestLoggingMiddleware`, `PaginatedList<T>`, entity hierarchy

---

## Technical Considerations

- **Existing patterns to preserve** (from `.github/copilot-instructions.md` and `.editorconfig`):
  - No `var` — explicit types always
  - Allman braces, file-scoped namespaces
  - No `!` null-forgiving operator — use `is null` / `is not null`
  - No primary constructors
  - CQRS: one folder per operation with `*Command.cs`/`*Query.cs` + `*Handler.cs` + `*Validator.cs`
  - Records with `init` for commands, queries, and VMs
- **Azure OpenAI SDK**: Use `Azure.AI.OpenAI` NuGet package with structured JSON output mode for consistent grading results
- **Azure Speech SDK**: Use `Microsoft.CognitiveServices.Speech` NuGet package for STT/TTS
- **Polly resilience**: Retry + circuit breaker on AI and Speech calls (transient HTTP errors, rate limiting)
- **AI cost management**: Pre-generate exercises nightly to batch API calls; enforce per-user daily submission limits; cache AI evaluations for identical exercise+answer combinations
- **Identity**: `Microsoft.AspNetCore.Identity.EntityFrameworkCore` — `User` extends `IdentityUser` and adds `ICreatedEntity` interface manually (since `IdentityUser` has its own `Id`)
- **JSON columns**: Use EF Core JSON column mapping for `Exercise.Hints` and `ExerciseAttempt.DetailedBreakdown`
- **Background jobs**: `IHostedService` for nightly exercise generation; consider Hangfire if scheduling complexity grows
- **Performance**: Leaderboard queries should use SQL window functions (`ROW_NUMBER`) for efficient ranking
- **Testing**: Mock `IAIEvaluationService` and `ISpeechService` in unit tests; optionally test with real Azure services in a separate test category marked `[Trait("Category", "AI")]`
- **Audio storage**: Use `IFileStorageService` (preserved from template) for audio recordings; upgrade to Azure Blob Storage for production

---

## Success Metrics

- End-to-end lesson flow works: user registers → picks a topic → collects daily lesson → completes exercises → AI evaluates each answer → XP + streak updated → appears on leaderboard
- Audio exercises work end-to-end: listen to prompt → record answer → transcription → AI evaluation → feedback
- AI evaluation returns meaningful, subject-appropriate feedback within 3 seconds
- Daily submission limit correctly enforced and resets at midnight UTC
- Nightly exercise generation runs reliably and produces varied, appropriate exercises
- All unit and integration tests pass
- Mobile app builds and runs on both Android and iOS simulators
- At least 3 fully seeded topics with different subject domains are playable
- Personal question bank items successfully seed into daily lesson generation

---

## Open Questions

1. **Identity + entity hierarchy**: `IdentityUser` has its own `Id` (string). Recommend extending `IdentityUser` directly and implementing `ICreatedEntity` as an interface — confirm this approach.
2. **Daily submission limit default**: Is 50/day the right default, or should it be higher/lower?
3. **Nightly generation timing**: 02:00 UTC works globally but may need per-timezone consideration — is UTC-based acceptable?
4. **Audio file size limits**: What max recording duration/file size for speaking exercises? Recommend 60 seconds / 10MB.
5. **Exercise generation fallback**: If AI generation fails overnight, should users get a "no lesson available" message or fall back to previously generated exercises?
