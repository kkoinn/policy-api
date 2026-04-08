# Policy API – Copilot Instructions

## Stack

- **Runtime**: .NET 10, C# 14
- **API**: ASP.NET Core Minimal APIs
- **Testing**: xUnit.v3, NSubstitute, FluentAssertions
- **Observability**: OpenTelemetry (logs only, no console exporter)
- **Messaging**: Azure Service Bus (emulator for local dev)
- **Config**: Azure App Configuration (optional in Development)
- **API docs**: Scalar + Microsoft.AspNetCore.OpenApi

## Code Style

- Use C# 14 **extension members** syntax for all extension methods:
  ```csharp
  public static class MyExtensions
  {
      extension(WebApplication app)
      {
          public void UseMyMiddleware() { app.UseMiddleware<MyMiddleware>(); }
      }
  }
  ```
  Named receiver (e.g. `app`, `builder`) required for instance methods. No `public` on the outer class.
- Use **record types** for models: `public record Policy(string PolicyNumber, ...);`
- Use **primary constructors** for services
- Use **file-scoped namespaces** — but this project uses implicit global usings, so namespaces are omitted
- Return `Results.*` from Minimal API handlers, never `IActionResult`
- Validate at the handler boundary; use `Results.ValidationProblem(...)` for input errors

## Architecture

```
src/PolicyApi/
  Endpoints/       # Minimal API handlers (static classes)
  Models/          # Records (CreatePolicyRequest, Policy, PolicyEvent)
  Services/        # Interfaces + implementations (Guidewire, ServiceBus)
  Configuration/   # Extension methods for DI setup
  FeatureToggles/  # Feature flag constants
  Middleware/       # Exception handling
tests/
  PolicyApi.UnitTests/          # Handler logic, uses NSubstitute fakes
  PolicyApi.IntegrationTests/   # Full HTTP stack via WebApplicationFactory
```

## Build and Test

```bash
dotnet build PolicyApi.slnx -c Debug
dotnet test PolicyApi.slnx -c Debug
```

## Testing Conventions

- **Unit tests**: Use `NSubstitute` for mocking, `FluentAssertions` for assertions
- **Integration tests**: Extend `PolicyApiFactory` (WebApplicationFactory); inject `FakeGuidewireService` and `FakeEventPublisher`
- Test class names: `{Subject}Tests` — e.g. `CreatePolicyHandlerTests`
- Always add `xUnit.v3` — requires explicit `using Xunit;` (no auto global using)
- Avoid `CancellationToken.None` in tests; prefer `TestContext.Current.CancellationToken`

## Commit Conventions

Follow Conventional Commits — GitVersion uses these to determine the next SemVer:

| Prefix | Version bump |
|--------|-------------|
| `fix:` | patch (1.2.3 → 1.2.4) |
| `feat:` | minor (1.2.3 → 1.3.0) |
| `BREAKING CHANGE` in body | major (1.2.3 → 2.0.0) |

## Local Development

See [docs/local-development.md](../docs/local-development.md) for Docker emulator setup (Service Bus + Guidewire stub), user secrets, and running the app.

Health check: `GET /health`  
API docs: `GET /scalar` (Development only)
