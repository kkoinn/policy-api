@description('Name of the App Configuration store')
param appConfigName string

@description('Location for all resources')
param location string = resourceGroup().location

@description('Principal ID of the managed identity to assign App Configuration Reader role')
param principalId string

@description('Tags to apply to all resources')
param tags object = {}

resource appConfig 'Microsoft.AppConfiguration/configurationStores@2024-05-01' = {
  name:     appConfigName
  location: location
  tags:     tags
  sku: {
    name: 'standard'
  }
  properties: {
    disableLocalAuth: false
  }
}

resource featureFlag 'Microsoft.AppConfiguration/configurationStores/keyValues@2024-05-01' = {
  name:   '.appconfig.featureflag~2FEnableEventPublish'
  parent: appConfig
  properties: {
    value:       '{"id":"EnableEventPublish","description":"Controls whether policy events are published to Service Bus","enabled":true}'
    contentType: 'application/vnd.microsoft.appconfig.ff+json;charset=utf-8'
  }
}

resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name:  guid(appConfig.id, principalId, 'App Configuration Data Reader')
  scope: appConfig
  properties: {
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      '516239f1-63e1-4d78-a4de-a74fb236a071'
    )
    principalId:   principalId
    principalType: 'ServicePrincipal'
  }
}

output appConfigName     string = appConfig.name
output appConfigEndpoint string = appConfig.properties.endpoint
