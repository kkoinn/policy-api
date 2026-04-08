# Local Development Setup

## Prerequisites
- .NET 10 SDK
- Docker Desktop (for the Service Bus emulator)
- Azure CLI (optional, for cloud deployments)

## Service Bus Emulator

The project uses the [Azure Service Bus Emulator](https://learn.microsoft.com/en-us/azure/service-bus-messaging/overview-emulator) running in Docker for local development.

### Start the emulator

```bash
docker compose up -d
```

This starts the Service Bus emulator, a SQL Server container (required dependency), and a **Guidewire stub** (WireMock) on port 9090. The emulator pre-creates a `policy-events` topic as configured in `infrastructure/emulator/servicebus-config.json`. The Guidewire stub serves a fixed response from `infrastructure/stubs/guidewire/mappings/`.

### Stop the emulator

```bash
docker compose down
```

> **Note:** Data does not persist across container restarts.

## User Secrets

The connection string for the local emulator is stored as a user secret (not in appsettings). The `SharedAccessKey` value is a fixed dummy key used by the emulator

```bash
dotnet user-secrets set "ServiceBus:ConnectionString" "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;" --project src/PolicyApi
dotnet user-secrets list --project src/PolicyApi
```

Azure App Configuration is optional for local development. When the endpoint is empty the app skips it and uses values from `appsettings.Development.json` directly (including feature flags). To connect to a real App Configuration instance:

```bash
dotnet user-secrets set "AzureAppConfiguration:Endpoint" "https://your-appconfig.azconfig.io" --project src/PolicyApi
```

## GitHub MCP Server

The workspace includes a GitHub MCP server (`.vscode/mcp.json`) that connects Copilot's agent mode to GitHub APIs (issues, PRs, repos, etc.) directly from VS Code. It uses the `@modelcontextprotocol/server-github` package via `npx`.

Set the `POLICY_API_GITHUB_TOKEN` environment variable with a GitHub PAT (classic) that has the following scopes.

To create the token, go to [GitHub.com → Settings → Developer settings → Personal access tokens → Tokens (classic)](https://github.com/settings/tokens) and click **Generate new token (classic)**. Select the scopes below:

- **repo** — full access to repositories (read code, issues, PRs, commits)
- **read:org** — read organisation and team membership (needed if the repo is under an org)

```bash
# Windows (PowerShell)
[System.Environment]::SetEnvironmentVariable("POLICY_API_GITHUB_TOKEN", "ghp_your-token-here", "User")

# macOS / Linux
export POLICY_API_GITHUB_TOKEN="ghp_your-token-here"
```

Restart VS Code after setting the variable so it picks up the new value.
