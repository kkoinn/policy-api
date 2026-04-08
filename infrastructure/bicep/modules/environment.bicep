@description('Name of the Container Apps environment')
param environmentName string

@description('Location for all resources')
param location string = resourceGroup().location

@description('User-assigned managed identity resource ID')
param userAssignedIdentityId string

@description('Tags to apply to all resources')
param tags object = {}

resource environment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name:     environmentName
  location: location
  tags:     tags
  properties: {
    workloadProfiles: [
      {
        name:                'Consumption'
        workloadProfileType: 'Consumption'
      }
    ]
  }
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name:     'stpolicy${uniqueString(resourceGroup().id)}'
  location: location
  tags:     tags
  kind:     'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userAssignedIdentityId}': {}
    }
  }
  properties: {
    accessTier:               'Hot'
    minimumTlsVersion:        'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
}

output environmentId      string = environment.id
output environmentName    string = environment.name
output storageAccountName string = storageAccount.name
