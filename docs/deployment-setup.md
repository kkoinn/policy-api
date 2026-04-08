# Deployment Setup

This document covers the one-time setup required before the CD pipeline (`.github/workflows/cd.yml`) can deploy to Azure. The pipeline triggers automatically when a Git tag matching `v*.*.*` is pushed.

## Prerequisites

- [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) installed
- Owner or Contributor role on the target Azure subscription
- Admin access to the GitHub repository

---

## 1. Azure App Registration (OIDC)

The CD pipeline authenticates to Azure using OpenID Connect (OIDC) — no client secrets required.

### Create the app registration

```bash
az ad app create --display-name "policy-api-deploy"
```

Note the `appId` from the output — this is your `AZURE_CLIENT_ID`.

### Create a service principal for it

```bash
az ad sp create --id <appId>
```

### Assign the Contributor role on your subscription

```bash
az role assignment create \
  --assignee <appId> \
  --role Contributor \
  --scope /subscriptions/<AZURE_SUBSCRIPTION_ID>
```

### Create the federated credential

This allows GitHub Actions to authenticate as the app registration when deploying from the `test` environment.

**PowerShell:**
```powershell
az ad app federated-credential create --id <appId> --parameters '{\"name\":\"github-policy-api-test\",\"issuer\":\"https://token.actions.githubusercontent.com\",\"subject\":\"repo:kkoinn/policy-api:environment:test\",\"audiences\":[\"api://AzureADTokenExchange\"]}'
```

**Bash/macOS/Linux:**
```bash
az ad app federated-credential create --id <appId> --parameters '{
  "name": "github-policy-api-test",
  "issuer": "https://token.actions.githubusercontent.com",
  "subject": "repo:kkoinn/policy-api:environment:test",
  "audiences": ["api://AzureADTokenExchange"]
}'
```

---

## 2. GitHub Environment

Create the `test` environment in GitHub:

1. Go to **GitHub repo → Settings → Environments → New environment**
2. Name it `test`
3. Optionally add protection rules (e.g. require manual approval, restrict to tag-based deployments)

---

## 3. GitHub Secrets

### Environment secrets (`test`)

Go to **Settings → Environments → test → Add secret** and create the following:

| Secret | Description |
|--------|-------------|
| `AZURE_CLIENT_ID` | `appId` from the app registration created in step 1 |
| `AZURE_TENANT_ID` | Your Azure AD tenant ID (`az account show --query tenantId -o tsv`) |
| `AZURE_SUBSCRIPTION_ID` | Your Azure subscription ID (`az account show --query id -o tsv`) |
| `SERVICEBUS_CONNECTION_STRING` | Connection string for the Azure Service Bus in the test environment |
| `GUIDEWIRE_BASE_URL` | Base URL of the Guidewire API, e.g. `https://test-guidewire.example.com` |
| `GHCR_PAT` | GitHub PAT for pushing/pulling images — see section 4 below |

### Useful Azure CLI commands to find IDs

```bash
az account show --query id -o tsv        # Subscription ID
az account show --query tenantId -o tsv  # Tenant ID
```

---

## 4. GHCR PAT (Container Registry)

The pipeline uses a PAT to push and pull Docker images to/from GitHub Container Registry (GHCR).

Create a GitHub PAT at [github.com/settings/tokens](https://github.com/settings/tokens) (**Tokens (classic)**) with these scopes:

- **repo** — package visibility tied to a repository
- **write:packages** — push container images
- **delete:packages** — remove old image versions (optional)
- **admin:org** — required if the repository belongs to a GitHub organisation

Store it as the `GHCR_PAT` secret in the `test` environment (see section 3).

---

## 5. Trigger a Deployment

Once all secrets are configured, push a version tag to trigger the CD pipeline:

```bash
git tag v1.0.0
git push origin v1.0.0
```

The pipeline will:
1. Build and push the Docker image tagged `v1.0.0` to GHCR
2. Deploy the Bicep infrastructure to Azure
3. Run a health check against the deployed app
