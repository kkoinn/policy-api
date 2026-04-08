# Policy API

A test case built with .NET 10 Minimal API around the idea of a Policy API that creates policies through a Guidewire backend and distributes policy-created events via Azure Service Bus. 

**Key endpoint:** `POST /api/v1/policies` — validates input, creates a policy in Guidewire, and optionally publishes a `PolicyCreated` event (controlled by a feature toggle).

## Documentation

- [Local development](docs/local-development.md) — Docker emulator setup, user secrets, and running the app locally.
- [Deployment setup](docs/deployment-setup.md) — Azure infrastructure and CI/CD deployment configuration.

## Assessment Criteria

### 1. Test-Driven Development (TDD)

The project contains both unit and integration tests.

**Unit tests** ([tests/PolicyApi.UnitTests](tests/PolicyApi.UnitTests/Endpoints/CreatePolicyHandlerTests.cs)) use NSubstitute for mocking and FluentAssertions for assertions. They verify handler logic in isolation — valid creation returns `201 Created`, input validation rejects non-numeric customer numbers with `400`, and exceptions from downstream services propagate correctly.

**Integration tests** ([tests/PolicyApi.IntegrationTests](tests/PolicyApi.IntegrationTests/Endpoints/CreatePolicyEndpointTests.cs)) run the full HTTP stack via `WebApplicationFactory` with fake implementations of Guidewire and the event publisher. They verify end-to-end request handling, including that `GET /health` returns `200 OK`. In a production-grade setup, these tests would ideally run against live instances of Service Bus and the Guidewire API (e.g. using Docker-based emulators or dedicated test environments) to catch integration issues that fakes cannot surface.

Run all tests:
```bash
dotnet test PolicyApi.slnx
```

### 2. Feature Toggles

The `EnableEventPublish` feature flag (defined in [FeatureFlags.cs](src/PolicyApi/FeatureToggles/FeatureFlags.cs)) controls whether a `PolicyCreated` event is published to Service Bus after a policy is created. The flag is evaluated at runtime via `IFeatureManager` from `Microsoft.FeatureManagement`.

In runtime environment (test/prod) the flag is managed through Azure App Configuration. Locally it can be set in `appsettings.Development.json` under `FeatureManagement`. Both unit and integration tests verify behavior with the toggle enabled and disabled.

### 3. CI/CD Pipeline (YAML)

The GitHub Actions pipeline ([.github/workflows/ci.yml](.github/workflows/ci.yml)) runs on every push and pull request to `main`. It:

- **Builds** the solution with a semantic version determined by GitVersion (Conventional Commits).
- **Tests** — runs unit and integration tests, publishes results via `dorny/tests-reporter`, and uploads test artifacts.
- **Scans** — runs CodeQL static analysis.
- **Packages** — builds a Docker image and pushes it to GitHub Container Registry (`ghcr.io`), tagged with the semantic version, commit SHA, and `latest`.

### 4. Automated Security and Quality

Instead of Snyk or SonarQube, the project uses GitHub's built-in free tools:

- **CodeQL** — static application security testing (SAST) with the `security-and-quality` query suite, running on every push and PR.
- **Dependabot** ([.github/dependabot.yml](.github/dependabot.yml)) — weekly automated pull requests for NuGet, Docker, and GitHub Actions version updates.
- **Secret scanning** — enabled by default on the repository to detect accidentally committed credentials.

### 5. Observability

- **Structured logging** via OpenTelemetry configured in [OpenTelemetryExtensions.cs](src/PolicyApi/Configuration/OpenTelemetryExtensions.cs). The service is identified as `PolicyApi` and logs include formatted messages and scopes. In a production setup, metrics and distributed traces would also be included in the OpenTelemetry configuration but are excluded from this example.
- **Health check** at `GET /health` returns `200 OK` with body `Healthy`. Verified by an integration test.
- **Problem Details** middleware provides standardized error responses (RFC 9457).

### 6. AI-Assisted Development

This project was developed with help of **GitHub Copilot**. Starting from a plan describing the idea — a Policy API that calls a backend Guidewire API and publishes a policy-created event, with a feature toggle to disable event publishing if Guidewire is down — Copilot assisted with scaffolding code, writing tests, configuring the CI/CD pipeline, setting up Azure Bicep infrastructure, and authoring documentation.

The Copilot workspace instructions are maintained in [.github/copilot-instructions.md](.github/copilot-instructions.md) to keep AI suggestions aligned with project conventions.