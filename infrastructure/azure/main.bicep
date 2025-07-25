// © 2024 DecVCPlat. All rights reserved.
// DecVCPlat Azure Infrastructure as Code

@description('Environment name (dev, staging, prod)')
param environmentName string = 'prod'

@description('Location for all resources')
param location string = resourceGroup().location

@description('Unique suffix for resource naming')
param uniqueSuffix string = uniqueString(resourceGroup().id)

// Variables
var decvcplatResourcePrefix = 'decvcplat-${environmentName}-${uniqueSuffix}'
var decvcplatTags = {
  Application: 'DecVCPlat'
  Environment: environmentName
  Owner: 'DecVCPlat-Team'
  CostCenter: 'DecVCPlat-Operations'
}

// Azure Container Registry
resource decvcplatContainerRegistry 'Microsoft.ContainerRegistry/registries@2023-01-01-preview' = {
  name: '${decvcplatResourcePrefix}acr'
  location: location
  tags: decvcplatTags
  sku: {
    name: 'Premium'
  }
  properties: {
    adminUserEnabled: true
    publicNetworkAccess: 'Enabled'
    zoneRedundancy: 'Enabled'
  }
}

// Azure Kubernetes Service
resource decvcplatAksCluster 'Microsoft.ContainerService/managedClusters@2023-08-01' = {
  name: '${decvcplatResourcePrefix}-aks'
  location: location
  tags: decvcplatTags
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    dnsPrefix: '${decvcplatResourcePrefix}-dns'
    agentPoolProfiles: [
      {
        name: 'decvcplat'
        count: 3
        vmSize: 'Standard_D4s_v3'
        osDiskSizeGB: 128
        osType: 'Linux'
        mode: 'System'
        enableAutoScaling: true
        minCount: 3
        maxCount: 10
      }
    ]
    networkProfile: {
      networkPlugin: 'azure'
      networkPolicy: 'azure'
      serviceCidr: '10.0.0.0/16'
      dnsServiceIP: '10.0.0.10'
    }
    addonProfiles: {
      azureKeyvaultSecretsProvider: {
        enabled: true
      }
      monitoring: {
        enabled: true
        config: {
          logAnalyticsWorkspaceResourceID: decvcplatLogAnalytics.id
        }
      }
    }
  }
}

// Log Analytics Workspace
resource decvcplatLogAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: '${decvcplatResourcePrefix}-logs'
  location: location
  tags: decvcplatTags
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: 90
  }
}

// Application Insights
resource decvcplatAppInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: '${decvcplatResourcePrefix}-insights'
  location: location
  tags: decvcplatTags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: decvcplatLogAnalytics.id
  }
}

// Azure SQL Database
resource decvcplatSqlServer 'Microsoft.Sql/servers@2022-05-01-preview' = {
  name: '${decvcplatResourcePrefix}-sql'
  location: location
  tags: decvcplatTags
  properties: {
    administratorLogin: 'decvcplatadmin'
    administratorLoginPassword: 'DecVCPlat@2024!'
    publicNetworkAccess: 'Enabled'
  }
}

resource decvcplatUserDatabase 'Microsoft.Sql/servers/databases@2022-05-01-preview' = {
  parent: decvcplatSqlServer
  name: 'DecVCPlatUsers'
  location: location
  tags: decvcplatTags
  sku: {
    name: 'S2'
    tier: 'Standard'
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
  }
}

resource decvcplatProjectDatabase 'Microsoft.Sql/servers/databases@2022-05-01-preview' = {
  parent: decvcplatSqlServer
  name: 'DecVCPlatProjects'
  location: location
  tags: decvcplatTags
  sku: {
    name: 'S2'
    tier: 'Standard'
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
  }
}

resource decvcplatVotingDatabase 'Microsoft.Sql/servers/databases@2022-05-01-preview' = {
  parent: decvcplatSqlServer
  name: 'DecVCPlatVoting'
  location: location
  tags: decvcplatTags
  sku: {
    name: 'S2'
    tier: 'Standard'
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
  }
}

resource decvcplatFundingDatabase 'Microsoft.Sql/servers/databases@2022-05-01-preview' = {
  parent: decvcplatSqlServer
  name: 'DecVCPlatFunding'
  location: location
  tags: decvcplatTags
  sku: {
    name: 'S2'
    tier: 'Standard'
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CS_AS'
  }
}

resource decvcplatNotificationDatabase 'Microsoft.Sql/servers/databases@2022-05-01-preview' = {
  parent: decvcplatSqlServer
  name: 'DecVCPlatNotifications'
  location: location
  tags: decvcplatTags
  sku: {
    name: 'S2'
    tier: 'Standard'
  }
  properties: {
    collation: 'SQL_Latin1_General_CP1_CI_AS'
  }
}

// Key Vault
resource decvcplatKeyVault 'Microsoft.KeyVault/vaults@2023-02-01' = {
  name: '${decvcplatResourcePrefix}-kv'
  location: location
  tags: decvcplatTags
  properties: {
    sku: {
      family: 'A'
      name: 'premium'
    }
    tenantId: subscription().tenantId
    accessPolicies: []
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 90
  }
}

// Storage Account for documents and static assets
resource decvcplatStorage 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: '${decvcplatResourcePrefix}storage'
  location: location
  tags: decvcplatTags
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    publicNetworkAccess: 'Enabled'
    allowBlobPublicAccess: true
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
  }
}

// CDN Profile
resource decvcplatCdnProfile 'Microsoft.Cdn/profiles@2023-05-01' = {
  name: '${decvcplatResourcePrefix}-cdn'
  location: 'Global'
  tags: decvcplatTags
  sku: {
    name: 'Standard_Microsoft'
  }
}

resource decvcplatCdnEndpoint 'Microsoft.Cdn/profiles/endpoints@2023-05-01' = {
  parent: decvcplatCdnProfile
  name: '${decvcplatResourcePrefix}-endpoint'
  location: 'Global'
  tags: decvcplatTags
  properties: {
    origins: [
      {
        name: 'decvcplat-origin'
        properties: {
          hostName: '${decvcplatStorage.name}.blob.core.windows.net'
          httpPort: 80
          httpsPort: 443
          originHostHeader: '${decvcplatStorage.name}.blob.core.windows.net'
        }
      }
    ]
    isHttpAllowed: false
    isHttpsAllowed: true
    queryStringCachingBehavior: 'IgnoreQueryString'
    isCompressionEnabled: true
    contentTypesToCompress: [
      'text/plain'
      'text/html'
      'text/css'
      'application/x-javascript'
      'text/javascript'
      'application/json'
    ]
  }
}

// Outputs
output decvcplatResourceGroupName string = resourceGroup().name
output decvcplatAksClusterName string = decvcplatAksCluster.name
output decvcplatContainerRegistryName string = decvcplatContainerRegistry.name
output decvcplatSqlServerName string = decvcplatSqlServer.name
output decvcplatKeyVaultName string = decvcplatKeyVault.name
output decvcplatStorageAccountName string = decvcplatStorage.name
output decvcplatCdnEndpointUrl string = decvcplatCdnEndpoint.properties.hostName
