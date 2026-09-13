# AI-Assisted Plausibility Checker

A web app for AI-assisted plausibility checking of answers: users provide a question and an answer, and the system iteratively evaluates via LLM (Perplexity) how plausible/consistent the answer is, returning a score and an explanation.

## Architecture

The repo consists of three independent sub-projects:

| Project | Description | Tech Stack |
|---|---|---|
| `BusinessService` | Core domain "Validation" including authentication (register/login/JWT), ownership management, persistence | ASP.NET Core 10 Web API, EF Core (SQLite) |
| `ValidationScoringService` | Encapsulates the LLM integration (Perplexity Agent API), performs the actual scoring/evaluation | ASP.NET Core 10 Web API |
| `ClientApp` | Single-page frontend with login, account management, and the validation flow | Angular 21 (standalone components, signals), Angular Material |

`BusinessService` calls `ValidationScoringService` synchronously over REST to request a scoring result. `ClientApp` talks exclusively to `BusinessService` (via a generated NSwag client) and authenticates using JWT.

```
ClientApp  --HTTP/JWT-->  BusinessService  --HTTP-->  ValidationScoringService  --HTTP-->  Perplexity API
```

## Prerequisites

- .NET 10 SDK
- Node.js + npm
- (optional) `dotnet-ef` CLI tool for migrations

## Docker Compose

Docker Engine with the Compose v2 plugin is required. Create a `.env` file in the repository root before starting the stack:

```dotenv
PERPLEXITY_API_KEY=<your-perplexity-api-key>
JWT_SIGNING_KEY=<long-local-signing-key>
```

The `.env` file is ignored by Git and must not be committed. Generate a local signing key, for example, with:

```bash
openssl rand -hex 32
```

Validate the Compose configuration and start all services:

```bash
docker compose config --quiet
docker compose up --build
```

The application is then available at `http://localhost:4200`. The BusinessService API is exposed at `http://localhost:5030`; the ValidationScoringService is reachable only inside the Compose backend network and is not published on a host port. The SQLite database is stored in the named `business-data` volume. `docker compose down` keeps the data; `docker compose down -v` removes the volume and its data.

## Setup & Run

### 1. BusinessService (port 5030/5031)

```bash
cd BusinessService
dotnet build BusinessService.sln
dotnet ef database update --project BusinessService.Infrastructure --startup-project BusinessService.Api
dotnet user-secrets set "Jwt:SigningKey" "$(openssl rand -hex 32)" --project BusinessService.Api
dotnet run --project BusinessService.Api
```

The JWT signing key is intentionally not stored in `appsettings.json`. Configure it through .NET User Secrets for local development or through the `Jwt__SigningKey` environment variable in a deployment.

Runs by default at `https://localhost:5030` (HTTPS) / `http://localhost:5031` (HTTP). Uses a local SQLite database (`business.db`).

### 2. ValidationScoringService (port 5040/5041)

Requires a Perplexity API key. This is **not** stored in `appsettings.json`, but configured via .NET User Secrets or an environment variable:

```bash
cd ValidationScoringService/ValidationScoringService.Api
dotnet user-secrets set "Perplexity:ApiKey" "<your-api-key>"
dotnet run
```

Runs by default at `https://localhost:5040` (HTTPS) / `http://localhost:5041` (HTTP).

### 3. ClientApp (port 4200)

```bash
cd ClientApp
npm install
npm start
```

Opens at `http://localhost:4200`. Requests to `/api` are forwarded via the dev proxy (`proxy.conf.json`) to `BusinessService` (`https://localhost:5030`).

## Tests

```bash
# BusinessService
dotnet test BusinessService/BusinessService.sln

# ValidationScoringService
dotnet test ValidationScoringService/ValidationScoringService.sln

# ClientApp
cd ClientApp && npm test
```

All three test suites are also automated in the CI pipeline (`.github/workflows/ci.yml`) and run on every push/PR against `main`.

## Authentication

`BusinessService` provides JWT-based authentication:

- `POST /api/Auth/register` – registration
- `POST /api/Auth/login` – login, returns a JWT
- `GET /api/Auth/me` – current user (authenticated)
- `DELETE /api/Auth/me` – delete account (authenticated, cascades to all related validations)

Ownership of validations is determined exclusively from the JWT claim, never from client-supplied IDs.

## API Client Generation

The TypeScript client for `ClientApp` is generated from `BusinessService`'s OpenAPI specification via [NSwag](https://github.com/RicoSuter/NSwag) and **must not be edited manually**:

```bash
cd ClientApp
npm run generate:businessServiceClient-client
```

Prerequisite: `BusinessService` must be running so the OpenAPI specification can be fetched.
