# SimbonationsBlog

A blog application built with CLEAN Architecture, CQRS with MediatR, and Next.js 15.

## Architecture

- **Blog.Domain** — Pure domain entities, enums, value objects
- **Blog.Application** — MediatR handlers, FluentValidation, interfaces
- **Blog.Infrastructure** — EF Core DbContext, persistence, file storage
- **Blog.WebAPI** — ASP.NET 8 API controllers
- **Blog.Website** — Next.js 15 frontend with React 19 and Tailwind CSS

## Getting Started

### Prerequisites
- .NET 8 SDK
- Node.js 20+ with pnpm
- SQL Server (LocalDB or Docker)

### Backend
```bash
cd SimbonationsBlog
dotnet restore BlogApp.sln
dotnet build BlogApp.sln
dotnet run --project src/Blog.WebAPI
```

### Frontend
```bash
cd src/Blog.Website
pnpm install
pnpm dev
```

### Docker
```bash
cd docker
docker compose up
```

## Testing
```bash
dotnet test BlogApp.sln
cd src/Blog.Website && pnpm test
```
