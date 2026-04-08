@description('Name of the application container app')
param appName string

@description('Container Apps environment ID')
param environmentId string

@description('Container image to deploy')
param containerImage string

@description('Service Bus connection string')
@secure()
param serviceBusConnectionString string

@description('Guidewire base URL')
param guidewireBaseUrl string

@description('Azure App Configuration endpoint')
param appConfigEndpoint string

@description('User-assigned managed identity resource ID')
param identityId string

@description('User-assigned managed identity client ID')
param identityClientId string

@description('GitHub Container Registry username')
param registryUsername string

@description('GitHub Container Registry password (PAT)')
@secure()
param registryPassword string

@description('ASP.NET Core environment')
param aspNetCoreEnvironment string = 'Production'

@description('Revision suffix for unique revision naming')
param revisionSuffix string = uniqueString(utcNow())

@description('Location for all resources')
param location string = resourceGroup().location

@description('Tags to apply to all resources')
param tags object = {}

resource app 'Microsoft.App/containerApps@2024-03-01' = {
  name: appName
  location: location
  tags: tags
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${identityId}': {}
    }
  }
  properties: {
    environmentId: environmentId
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 8080
        transport: 'http'
        allowInsecure: false
      }
      registries: [
        {
          server: 'ghcr.io'
          username: registryUsername
          passwordSecretRef: 'registry-password'
        }
      ]
      secrets: [
        {
          name: 'registry-password'
          value: registryPassword
        }
        {
          name: 'servicebus-connection-string'
          value: serviceBusConnectionString
        }
      ]
    }
    template: {
      revisionSuffix: revisionSuffix
      containers: [
        {
          name: 'policy-api'
          image: containerImage
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          probes: [
            {
              type: 'Liveness'
              httpGet: {
                path: '/health'
                port: 8080
                scheme: 'HTTP'
              }
              initialDelaySeconds: 5
              periodSeconds: 10
              failureThreshold: 3
            }
            {
              type: 'Readiness'
              httpGet: {
                path: '/health'
                port: 8080
                scheme: 'HTTP'
              }
              initialDelaySeconds: 5
              periodSeconds: 10
              failureThreshold: 3
            }
          ]
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: aspNetCoreEnvironment
            }
            {
              name: 'Guidewire__BaseUrl'
              value: guidewireBaseUrl
            }
            {
              name: 'ServiceBus__ConnectionString'
              secretRef: 'servicebus-connection-string'
            }
            {
              name: 'ServiceBus__TopicName'
              value: 'policy-events'
            }
            {
              name: 'AzureAppConfiguration__Endpoint'
              value: appConfigEndpoint
            }
            {
              name: 'AZURE_CLIENT_ID'
              value: identityClientId
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 3
      }
    }
  }
}

output appName string = app.name
output appFqdn string = app.properties.configuration.ingress.fqdn
output appUrl string = 'https://${app.properties.configuration.ingress.fqdn}'
