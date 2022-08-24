targetScope = 'subscription'

param resourceGroupName string = 'ServerlessFacesAnalyzer-rg'
param location string = deployment().location

resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-01-01' = {
  name: resourceGroupName
  location: location
}

module keyVaultsModule 'resources.bicep' = {
  scope: resourceGroup
  name: 'resources'
  params: {
    location: location
  }
}
