# LearnFlow — Best Practices

This document is the authoritative coding reference for the LearnFlow codebase. Follow these rules consistently across all new code and changes.

---

## 1. Clean Architecture

### Layer dependency rules

```
Domain ← Application ← Infrastructure
                     ← WebAPI
```

- **Domain** has zero external dependencies. No EF Core, no MediatR, no NuGet packages except primitives.
- **Application** depends on Domain and declares interfaces (e.g., `ILearnDbContext`, `IAIEvaluationService`). It never references Infrastructure or WebAPI.
- **Infrastructure** implements Application interfaces. It owns EF Core, AI SDK calls, and third-party integrations.
- **WebAPI** wires everything together (DI registration) and exposes HTTP endpoints. Controllers contain no business logic.

### What belongs where

| Layer | Allowed | Forbidden |
|-------|---------|-----------|
| Domain | Entities, value objects, enums, domain services (pure C#) | EF, MediatR, HTTP, logging |
| Application | Commands, queries, handlers, validators, interfaces, DTOs/VMs | EF DbContext directly, HTTP types |
| Infrastructure | DbContext, migrations, AI clients, email, storage | Business logic, domain rules |
| WebAPI | Controllers, DI setup, middleware, filters | Business logic, DB queries |

---

## 2. CQRS with MediatR

### Folder structure

Each operation gets its own folder under the feature directory:

```
Learn.Application/
  Exercises/
    Submit/
      SubmitAnswerCommand.cs
      SubmitAnswerCommandHandler.cs
      SubmitAnswerCommandValidator.cs
      Models/
        ExerciseAttemptResultVm.cs
```

### Commands vs. Queries

- **Command**: mutates state. Returns a result VM or `Unit`. Named `*Command`.
- **Query**: read-only. Returns a VM or list. Named `*Query`.

### Validation pipeline

All commands and queries that require validation get a `*Validator.cs` using FluentValidation. The pipeline behavior automatically runs validators before the handler. Handlers must never duplicate validation logic.

### Handler responsibilities

A handler should do exactly one thing: orchestrate domain operations and persistence. It must not:
- Contain complex business logic (push that to domain entities or domain services)
- Call other handlers via `ISender.Send()`
- Throw generic `Exception` — use the typed exceptions in `Learn.Application.Common.Exceptions`

---

## 3. C# Conventions

```csharp
// Explicit types — never var
List<ExerciseAttempt> attempts = await _db.ExerciseAttempts.ToListAsync();

// Allman braces
if (attempt is null)
{
    throw new NotFoundException(nameof(ExerciseAttempt), id);
}

// File-scoped namespaces
namespace Learn.Application.Exercises.Submit;

// Null checks with is null / is not null (outside LINQ expressions)
if (streak is null) { ... }
if (streak is not null) { ... }

// Boolean literals on the right
if (isPassing == true) { ... }
if (hasErrors == false) { ... }

// No primary constructors — use full constructor syntax
public class MyService
{
    private readonly ILearnDbContext _db;

    public MyService(ILearnDbContext db)
    {
        _db = db;
    }
}
```

- Use double quotes for all strings.
- No non-null assertion operator (`!`). Null-check and throw explicitly.
- String interpolation over concatenation: `$"User {userId} not found"`.

---

## 4. Entity Design

### Static `Create()` factories

Domain entities are created exclusively through static `Create()` factory methods. Never instantiate via `new` outside the entity itself.

```csharp
public class ExerciseAttempt : CreatedEntity<ExerciseAttempt>
{
    public Guid ExerciseId { get; set; }
    // ...

    public static ExerciseAttempt Create(
        Guid exerciseId,
        string userId,
        /* ... */
        bool isReview = false)
    {
        return new ExerciseAttempt
        {
            ExerciseId = exerciseId,
            // ...
            IsReview = isReview
        };
    }
}
```

### Rules

- No public setters on properties that represent invariants. Use `internal set` or `private set` where mutation is needed.
- Audit fields (`Id`, `CreatedDate`) are set by `CreatedEntity<T>` — never set them manually.
- Domain entities must never reference Application or Infrastructure types.

---

## 5. EF Core

### Configuration classes

Use fluent `IEntityTypeConfiguration<T>` classes — never data annotations on entities.

```csharp
// Learn.Infrastructure/Persistence/Configurations/ExerciseAttemptConfiguration.cs
public class ExerciseAttemptConfiguration : IEntityTypeConfiguration<ExerciseAttempt>
{
    public void Configure(EntityTypeBuilder<ExerciseAttempt> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.UserAnswer).IsRequired().HasMaxLength(4000);
        builder.HasIndex(a => new { a.UserId, a.CreatedDate });
    }
}
```

### When to add indexes

Add an index when:
- You filter or sort by the column in application queries.
- The table is expected to have > 10k rows in production.

### Migration discipline

- One migration per logical change. Never batch unrelated schema changes.
- Descriptive migration names: `AddGamificationPath`, `AddIsReviewToAttempts`.
- Never edit an already-applied migration. Add a new one to fix mistakes.
- No migration should drop a column without explicit product sign-off (data loss).

---

## 6. AI Service Patterns

### Dual-provider strategy

The AI evaluation service always has a primary (Claude) and a fallback (Ollama). The pattern is:

```csharp
try
{
    return await _claudeService.EvaluateAsync(request, cancellationToken);
}
catch (Exception)
{
    return await _ollamaService.EvaluateAsync(request, cancellationToken);
}
```

Never surface AI provider errors directly to the client — always attempt the fallback first.

### Per-request key override

Users can supply their own Claude API key via the `X-Claude-Api-Key` request header. The infrastructure layer reads this header before falling back to the configured key. Handlers must never reference HTTP headers directly — the `IAIEvaluationService` abstraction handles this.

### Structured prompts and response parsing

- Prompts must instruct the model to respond in a specific JSON schema.
- Always include a fallback parse path for when the model returns invalid JSON (e.g., return score=0, feedback="Grading failed").
- Set `GradedBy` in the result VM so the client can display which provider graded the answer.

---

## 7. FSRS / Spaced Repetition

### `ReviewItem` lifecycle

| Event | Action |
|-------|--------|
| User fails exercise (score < 70) | `ReviewItem.Create()` — new item added with initial stability/difficulty |
| Due review item answered correctly | `ReviewItem.RecordReview(grade: Good/Easy)` — stability increases, difficulty decreases |
| Due review item answered incorrectly | `ReviewItem.RecordReview(grade: Again)` — stability resets, difficulty increases |
| User passes exercise on first attempt | No `ReviewItem` created |

Review items are **never deleted**. They become due again based on retrievability decay.

### Score-to-grade mapping

| Score | FSRS Grade |
|-------|-----------|
| ≥ 90 | Easy |
| ≥ 70 | Good |
| < 70 | Again |

### Parameters

- **Stability**: days until retrievability drops to threshold. Grows with each successful review.
- **Difficulty**: 0–1. Resistance to stability growth. High-difficulty items need more frequent review.
- **Retrievability**: probability of recall right now. Computed from stability and elapsed time.

---

## 8. React Native / Expo Conventions

- Use `StyleSheet.create({})` for all styles — never inline style objects.
- AsyncStorage for all client-side persistence (tokens, preferences).
- Auth token is set globally on the axios instance via `setAuthToken()` in `mobile/lib/api.ts`. Never pass tokens as explicit parameters.
- No `any` types. Always define interfaces for API response shapes.
- Handle loading and error states explicitly in every screen. Never render partial data silently.
- Use `expo-router` file-based routing. Avoid imperative `router.navigate` for simple back-navigation — prefer `router.back()`.

```tsx
// Good — StyleSheet
const s = StyleSheet.create({
  container: { flex: 1, backgroundColor: Colors.background },
});

// Bad — inline
<View style={{ flex: 1, backgroundColor: "#1a1a2e" }} />
```

---

## 9. API Design

### Thin controllers

Controllers must contain no business logic. Every action dispatches a single MediatR request:

```csharp
[HttpPost("submit")]
public async Task<ActionResult<ExerciseAttemptResultVm>> Submit(
    SubmitAnswerCommand command,
    CancellationToken cancellationToken)
{
    return Ok(await Mediator.Send(command, cancellationToken));
}
```

### Authorization

All endpoints require `[Authorize]` by default (applied at the base controller). Remove it only when explicitly building a public endpoint (e.g., `POST /Auth/login`).

### Exception-to-HTTP mapping

| Exception type | HTTP status |
|---------------|-------------|
| `NotFoundException` | 404 |
| `ForbiddenAccessException` | 403 |
| `ValidationException` | 400 |
| `DailyLimitExceededException` | 429 |
| Unhandled | 500 |

These mappings are handled centrally in the exception middleware — never call `StatusCode()` or `BadRequest()` directly for these cases.

---

## 10. Git Workflow

### Branch naming

```
feature/short-description
fix/short-description
refactor/short-description
```

### Commit message style

Follow the existing commit history — imperative mood, sentence case, no period:

```
Add per-user AI model selector with API key override
Fix AI grading timeout on first launch
Upgrade to FSRS, add interleaving, weak-area summary, and content expansion
```

### When to migrate vs. avoid schema changes

**Migrate when:**
- A new domain concept requires a new table or column.
- An existing column's constraint needs to change (length, nullability).

**Avoid schema changes when:**
- You can store state in an existing column with a different interpretation.
- The change requires a data backfill that cannot be expressed safely in a migration.
- The change is speculative ("we might need this later").
