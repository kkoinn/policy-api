targetScope = 'resourceGroup'

@description('Name of the managed identity')
param identityName string

@description('Location for the identity')
param location string

@description('Tags to apply to the identity')
param tags object

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name:     identityName
  location: location
  tags:     tags
}

output identityId   string = managedIdentity.id
output identityName string = managedIdentity.name
output principalId  string = managedIdentity.properties.principalId
output clientId     string = managedIdentity.properties.clientId
