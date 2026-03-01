# GitHub Copilot Instructions for LearnFlow

## Critically Important Rules

### All Code
1. Always generate full code. If not possible, explain in comments.
2. Implement error checking and type validation in all code.
3. Always use explicit types. Never use the non-null assertion operator (`!`).
4. Use double quotes for all strings. Use string templates or `.join()` instead of concatenation.
5. Keep changes simple. Do not add anything not explicitly requested.

### Backend Rules (C# / .NET)
1. Always define types explicitly. **Never use `var`**.
2. Always use explicit boolean comparisons with literal on right (e.g., `myBool == true`, `hasValue == false`).
3. Use `is null` / `is not null` for null checks (outside LINQ/EF queries).
4. Allman-style braces (opening brace on new line).
5. File-scoped namespaces.
6. No primary constructors.

### Frontend Rules (TypeScript / React Native)
1. Strict TypeScript. Never use `any`. Never cast to `unknown`.
2. Use double quotes for all strings.
3. Use string templates over concatenation.
4. Use `StyleSheet.create({})` for all styles — never inline style objects.
5. Use `AsyncStorage` for client-side persistence — no `localStorage` or `sessionStorage` (web APIs not available in React Native).
6. Auth tokens are set globally via `setAuthToken()` — never pass them as explicit function parameters.

## Technology Stack
- Backend: .NET 8, C#, MediatR (CQRS), FluentValidation, CLEAN Architecture
- Mobile: React Native 0.83, Expo Router, TypeScript, Axios
- Database: SQL Server with EF Core
- AI: Claude (claude-haiku-4-5-20251001) primary, Ollama (llama3.2:3b) fallback
- Testing: xUnit v3, FluentAssertions, Moq, MockQueryable, Testcontainers

## Architecture
- CLEAN Architecture: Domain → Application → Infrastructure → WebAPI
- CQRS with MediatR: Commands (Create/Update/Delete) and Queries (Get/GetAll) per feature
- FluentValidation pipeline behavior for automatic validation
- Thin controllers that only call `Mediator.Send()`
- One folder per operation under the feature directory (e.g., `Exercises/Submit/`)
