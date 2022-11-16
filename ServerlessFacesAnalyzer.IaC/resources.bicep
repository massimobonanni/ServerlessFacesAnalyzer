@description('The location wher you want to create the resources.')
param location string = resourceGroup().location

@description('The name of the environment. It will be used to create the name of the resources in the resource group.')
@maxLength(16)
@minLength(3)
param environmentName string = 'sfa${uniqueString(subscription().id, resourceGroup().name)}'

var storageAccountName = toLower('${environmentName}dstore')
var functionAppStorageAccountName = toLower('${environmentName}appstore')
var funcHostingPlanName = toLower('${environmentName}-plan')
var functionAppName = toLower('${environmentName}-func')
var applicationInsightsName = toLower('${environmentName}-ai')
var cognitiveServiceName = toLower('${environmentName}-cs')
var keyVaultName = toLower('${environmentName}-kv')
var eventGridTopicName = toLower('${environmentName}-topic')

resource keyVault 'Microsoft.KeyVault/vaults@2021-11-01-preview' = {
  name: keyVaultName
  location: location
  properties: {
    accessPolicies: []
    enableRbacAuthorization: true
    enableSoftDelete: false
    enabledForDeployment: false
    enabledForDiskEncryption: false
    enabledForTemplateDeployment: false
    tenantId: subscription().tenantId
    sku: {
      name: 'standard'
      family: 'A'
    }
    networkAcls: {
      defaultAction: 'Allow'
      bypass: 'AzureServices'
    }
  }
}

resource appServiceKeyVaultAssignment 'Microsoft.Authorization/roleAssignments@2020-04-01-preview' = {
  name: guid('Key Vault Secret User', functionAppName, subscription().subscriptionId)
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6') // this is the role "Key Vault Secrets User"
    principalId: functionApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

resource azureWebJobsStorageSecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  name: 'AzureWebJobsStorage'
  parent: keyVault
  properties: {
    attributes: {
      enabled: true
    }
    value: 'DefaultEndpointsProtocol=https;AccountName=${functionAppStorageAccountName};EndpointSuffix=${environment().suffixes.storage};AccountKey=${functionAppStorageAccount.listKeys().keys[0].value}'
  }
}

resource appInsightInstrumentationKeySecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  name: 'AppInsightInstrumentationKey'
  parent: keyVault
  properties: {
    attributes: {
      enabled: true
    }
    value: applicationInsights.properties.InstrumentationKey
  }
}

resource storageConnectionStringSecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  name: 'StorageConnectionString'
  parent: keyVault
  properties: {
    attributes: {
      enabled: true
    }
    value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccountName};EndpointSuffix=${environment().suffixes.storage};AccountKey=${storageAccount.listKeys().keys[0].value}'
  }
}

resource cognitiveServiceApiKeySecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  name: 'CognitiveServiceApiKey'
  parent: keyVault
  properties: {
    attributes: {
      enabled: true
    }
    value: cognitiveService.listKeys().key1
  }
}

resource cognitiveServiceEndpointSecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  name: 'CognitiveServiceEndpoint'
  parent: keyVault
  properties: {
    attributes: {
      enabled: true
    }
    value: cognitiveService.properties.endpoint
  }
}

resource eventGridTopicEndpointSecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  name: 'EventGridTopicServiceEndpoint'
  parent: keyVault
  properties: {
    attributes: {
      enabled: true
    }
    value: eventGridTopic.properties.endpoint
  }
}

resource eventGridTopicKeySecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  name: 'EventGridTopicKey'
  parent: keyVault
  properties: {
    attributes: {
      enabled: true
    }
    value: eventGridTopic.listKeys().key1
  }
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-04-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
  }
}

resource containers 'Microsoft.Storage/storageAccounts/blobServices/containers@2019-06-01' = {
  name: '${storageAccount.name}/default/faces'
  properties: {
    publicAccess: 'None'
    metadata: {}
  }
}

resource analysisResultRule 'Microsoft.Storage/storageAccounts/managementPolicies@2021-09-01' = {
  name: 'default'
  parent: storageAccount
  properties: {
    policy: {
      rules: [
        {
          enabled: true
          name: 'AnalysisResultsRule'
          type: 'Lifecycle'
          definition: {
            actions: {
              baseBlob: {
                tierToCool: {
                  daysAfterModificationGreaterThan: 1
                }
                delete: {
                  daysAfterModificationGreaterThan: 10
                }
              }
            }
            filters: {
              blobTypes: [
                'blockBlob'
              ]
              prefixMatch: [
                'faces/'
              ]
            }
          }
        }
      ]
    }
  }
}

resource cognitiveService 'Microsoft.CognitiveServices/accounts@2021-10-01' = {
  name: cognitiveServiceName
  location: location
  sku: {
    name: 'S0'
  }
  kind: 'CognitiveServices'
  properties: {
    apiProperties: {
      statisticsEnabled: false
    }
  }
}

resource functionAppStorageAccount 'Microsoft.Storage/storageAccounts@2021-08-01' = {
  name: functionAppStorageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Request_Source: 'rest'
  }
}

resource funcHostingPlan 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: funcHostingPlanName
  location: location
  sku: {
    name: 'Y1'
    tier: 'Dynamic'
  }
  properties: {}
}

resource functionApp 'Microsoft.Web/sites@2021-03-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: funcHostingPlan.id
    siteConfig: {
      ftpsState: 'FtpsOnly'
      minTlsVersion: '1.2'
    }
    httpsOnly: true
  }
}
resource appSettings 'Microsoft.Web/sites/config@2022-03-01' = {
  name: 'appsettings'
  parent: functionApp
  properties: {
    APPINSIGHTS_INSTRUMENTATIONKEY: '@Microsoft.KeyVault(SecretUri=${appInsightInstrumentationKeySecret.properties.secretUri})'
    AzureWebJobsStorage: '@Microsoft.KeyVault(SecretUri=${azureWebJobsStorageSecret.properties.secretUri})'
    WEBSITE_CONTENTAZUREFILECONNECTIONSTRING: '@Microsoft.KeyVault(SecretUri=${azureWebJobsStorageSecret.properties.secretUri})'
    WEBSITE_CONTENTSHARE: toLower(functionAppName)
    FUNCTIONS_EXTENSION_VERSION: '~4'
    WEBSITE_NODE_DEFAULT_VERSION: '~10'
    FUNCTIONS_WORKER_RUNTIME: 'dotnet'
    StorageConnectionString: '@Microsoft.KeyVault(SecretUri=${storageConnectionStringSecret.properties.secretUri})'
    DestinationContainer: 'faces'
    'FaceAnalyzer:ServiceEndpoint': '@Microsoft.KeyVault(SecretUri=${cognitiveServiceEndpointSecret.properties.secretUri})'
    'FaceAnalyzer:ServiceKey': '@Microsoft.KeyVault(SecretUri=${cognitiveServiceApiKeySecret.properties.secretUri})'
    TopicEndpoint: '@Microsoft.KeyVault(SecretUri=${eventGridTopicEndpointSecret.properties.secretUri})'
    TopicKey: '@Microsoft.KeyVault(SecretUri=${eventGridTopicKeySecret.properties.secretUri})'
  }
  dependsOn:[
    keyVault
    appServiceKeyVaultAssignment
  ]
}

resource eventGridTopic 'Microsoft.EventGrid/topics@2022-06-15' = {
  name: eventGridTopicName
  location: location
  properties: {
    inputSchema: 'EventGridSchema'
    publicNetworkAccess: 'Enabled'
    disableLocalAuth: false
    dataResidencyBoundary: 'WithinGeopair'
  }
}

output eventGridTopicName string = eventGridTopic.name
