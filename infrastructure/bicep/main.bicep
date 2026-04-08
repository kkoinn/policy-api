targetScope = 'subscription'

@description('Environment name (e.g., test, production)')
@maxLength(15)
param environmentName string = 'test'

@description('Location for all resources')
param location string = 'westeurope'

@description('Container image for the application')
param containerImage string

@description('Service Bus connection string')
@secure()
param serviceBusConnectionString string

@description('Guidewire base URL')
param guidewireBaseUrl string

@description('GitHub Container Registry username')
param registryUsername string

@description('GitHub Container Registry password (PAT)')
@secure()
param registryPassword string

@description('Revision suffix for unique revision naming (e.g., GitHub run ID)')
param revisionSuffix string = uniqueString(utcNow())

@description('Tags to apply to all resources')
param tags object = {
  application: 'PolicyApi'
  environment: environmentName
}

var envShort = environmentName == 'production' ? 'prod' : 'test'

var naming = {
  resourceGroup: 'rg-policy-api-${envShort}'
  identity: 'id-policy-${envShort}'
  environment: 'policy-env-${envShort}'
  appConfig: 'appconfig-policy-${envShort}-${uniqueString(environmentName)}'
  app: 'policy-api-${envShort}'
}

resource resourceGroup 'Microsoft.Resources/resourceGroups@2023-07-01' = {
  name: naming.resourceGroup
  location: location
  tags: tags
}

module identity './modules/identity.bicep' = {
  name: 'deploy-identity'
  scope: resourceGroup
  params: {
    identityName: naming.identity
    location: location
    tags: tags
  }
}

module environment './modules/environment.bicep' = {
  name: 'deploy-environment'
  scope: resourceGroup
  params: {
    environmentName: naming.environment
    location: location
    userAssignedIdentityId: identity.outputs.identityId
    tags: tags
  }
}

module appConfiguration './modules/app-configuration.bicep' = {
  name: 'deploy-app-configuration'
  scope: resourceGroup
  params: {
    appConfigName: naming.appConfig
    location: location
    principalId: identity.outputs.principalId
    tags: tags
  }
}

module app './modules/app.bicep' = {
  name: 'deploy-app'
  scope: resourceGroup
  params: {
    appName: naming.app
    environmentId: environment.outputs.environmentId
    containerImage: containerImage
    serviceBusConnectionString: serviceBusConnectionString
    guidewireBaseUrl: guidewireBaseUrl
    appConfigEndpoint: appConfiguration.outputs.appConfigEndpoint
    registryUsername: registryUsername
    registryPassword: registryPassword
    aspNetCoreEnvironment: environmentName == 'production' ? 'Production' : 'Test'
    revisionSuffix: revisionSuffix
    identityId: identity.outputs.identityId
    identityClientId: identity.outputs.clientId
    location: location
    tags: tags
  }
}

output resourceGroupName string = resourceGroup.name
output appName string = app.outputs.appName
output appFqdn string = app.outputs.appFqdn
output appUrl string = app.outputs.appUrl
output appConfigEndpoint string = appConfiguration.outputs.appConfigEndpoint
