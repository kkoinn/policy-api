using './main.bicep'

param environmentName            = 'test'
param location                   = 'westeurope'
param registryUsername           = 'kkoinn'
param containerImage             = 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'
param serviceBusConnectionString = ''
param guidewireBaseUrl           = ''
param registryPassword           = ''
