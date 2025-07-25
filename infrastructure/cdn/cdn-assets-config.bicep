@description('CDN configuration for DecVCPlat static assets')
param location string = resourceGroup().location
param environment string = 'dev'
param storageAccountName string
param cdnProfileName string = 'decvcplat-cdn-${environment}'
param cdnEndpointName string = 'decvcplat-assets-${environment}'

// Storage account for static assets
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  name: storageAccountName
}

// CDN Profile for asset delivery
resource cdnProfile 'Microsoft.Cdn/profiles@2023-05-01' = {
  name: cdnProfileName
  location: location
  sku: {
    name: 'Standard_Microsoft'
  }
  properties: {
    originResponseTimeoutSeconds: 60
  }
}

// CDN Endpoint for static assets
resource cdnEndpoint 'Microsoft.Cdn/profiles/endpoints@2023-05-01' = {
  parent: cdnProfile
  name: cdnEndpointName
  location: location
  properties: {
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
      'application/javascript'
      'application/json'
      'application/xml'
      'text/xml'
      'image/svg+xml'
      'application/font-woff'
      'application/font-woff2'
      'font/woff'
      'font/woff2'
      'application/vnd.ms-fontobject'
      'font/ttf'
      'font/otf'
    ]
    origins: [
      {
        name: 'decvcplat-storage-origin'
        properties: {
          hostName: '${storageAccountName}.blob.core.windows.net'
          httpPort: 80
          httpsPort: 443
          originHostHeader: '${storageAccountName}.blob.core.windows.net'
          priority: 1
          weight: 1000
          enabled: true
        }
      }
    ]
    deliveryPolicy: {
      rules: [
        {
          name: 'StaticAssetsRule'
          order: 1
          conditions: [
            {
              name: 'UrlPath'
              parameters: {
                operator: 'BeginsWith'
                matchValues: [
                  '/assets/'
                  '/images/'
                  '/fonts/'
                  '/css/'
                  '/js/'
                ]
                negateCondition: false
                transforms: []
              }
            }
          ]
          actions: [
            {
              name: 'CacheExpiration'
              parameters: {
                cacheBehavior: 'Override'
                cacheType: 'All'
                cacheDuration: '30.00:00:00' // 30 days
              }
            }
            {
              name: 'CacheKeyQueryString'
              parameters: {
                queryStringBehavior: 'IgnoreSpecifiedQueryStrings'
                queryParameters: 'utm_source,utm_medium,utm_campaign,utm_term,utm_content'
              }
            }
            {
              name: 'ModifyResponseHeader'
              parameters: {
                headerAction: 'Append'
                headerName: 'Cache-Control'
                value: 'public, immutable, max-age=2592000'
              }
            }
          ]
        }
        {
          name: 'ImageOptimizationRule'
          order: 2
          conditions: [
            {
              name: 'UrlFileExtension'
              parameters: {
                operator: 'Equal'
                matchValues: [
                  'jpg'
                  'jpeg'
                  'png'
                  'gif'
                  'webp'
                  'svg'
                  'ico'
                ]
                negateCondition: false
                transforms: [
                  'Lowercase'
                ]
              }
            }
          ]
          actions: [
            {
              name: 'CacheExpiration'
              parameters: {
                cacheBehavior: 'Override'
                cacheType: 'All'
                cacheDuration: '365.00:00:00' // 1 year
              }
            }
            {
              name: 'ModifyResponseHeader'
              parameters: {
                headerAction: 'Append'
                headerName: 'Cache-Control'
                value: 'public, immutable, max-age=31536000'
              }
            }
          ]
        }
        {
          name: 'FontOptimizationRule'
          order: 3
          conditions: [
            {
              name: 'UrlFileExtension'
              parameters: {
                operator: 'Equal'
                matchValues: [
                  'woff'
                  'woff2'
                  'ttf'
                  'otf'
                  'eot'
                ]
                negateCondition: false
                transforms: [
                  'Lowercase'
                ]
              }
            }
          ]
          actions: [
            {
              name: 'CacheExpiration'
              parameters: {
                cacheBehavior: 'Override'
                cacheType: 'All'
                cacheDuration: '365.00:00:00' // 1 year
              }
            }
            {
              name: 'ModifyResponseHeader'
              parameters: {
                headerAction: 'Append'
                headerName: 'Access-Control-Allow-Origin'
                value: 'https://decvcplat.com'
              }
            }
            {
              name: 'ModifyResponseHeader'
              parameters: {
                headerAction: 'Append'
                headerName: 'Cache-Control'
                value: 'public, immutable, max-age=31536000'
              }
            }
          ]
        }
        {
          name: 'APIResponseRule'
          order: 4
          conditions: [
            {
              name: 'UrlPath'
              parameters: {
                operator: 'BeginsWith'
                matchValues: [
                  '/api/'
                ]
                negateCondition: false
                transforms: []
              }
            }
          ]
          actions: [
            {
              name: 'CacheExpiration'
              parameters: {
                cacheBehavior: 'Override'
                cacheType: 'All'
                cacheDuration: '00:05:00' // 5 minutes
              }
            }
          ]
        }
        {
          name: 'SecurityHeadersRule'
          order: 5
          conditions: [
            {
              name: 'RequestScheme'
              parameters: {
                operator: 'Equal'
                matchValues: [
                  'HTTPS'
                ]
                negateCondition: false
                transforms: []
              }
            }
          ]
          actions: [
            {
              name: 'ModifyResponseHeader'
              parameters: {
                headerAction: 'Append'
                headerName: 'Strict-Transport-Security'
                value: 'max-age=31536000; includeSubDomains; preload'
              }
            }
            {
              name: 'ModifyResponseHeader'
              parameters: {
                headerAction: 'Append'
                headerName: 'X-Content-Type-Options'
                value: 'nosniff'
              }
            }
            {
              name: 'ModifyResponseHeader'
              parameters: {
                headerAction: 'Append'
                headerName: 'X-Frame-Options'
                value: 'SAMEORIGIN'
              }
            }
            {
              name: 'ModifyResponseHeader'
              parameters: {
                headerAction: 'Append'
                headerName: 'X-XSS-Protection'
                value: '1; mode=block'
              }
            }
            {
              name: 'ModifyResponseHeader'
              parameters: {
                headerAction: 'Append'
                headerName: 'Referrer-Policy'
                value: 'strict-origin-when-cross-origin'
              }
            }
          ]
        }
      ]
    }
  }
}

// Custom domain for CDN
resource cdnCustomDomain 'Microsoft.Cdn/profiles/endpoints/customDomains@2023-05-01' = {
  parent: cdnEndpoint
  name: 'assets-decvcplat-com'
  properties: {
    hostName: 'assets.decvcplat.com'
    httpsParameters: {
      certificateSource: 'Cdn'
      protocolType: 'TLS12'
      minimumTlsVersion: 'TLS12'
    }
  }
}

// Storage containers for different asset types
resource assetsContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  name: '${storageAccountName}/default/assets'
  properties: {
    publicAccess: 'Blob'
    metadata: {
      purpose: 'DecVCPlat static assets'
      environment: environment
    }
  }
}

resource imagesContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  name: '${storageAccountName}/default/images'
  properties: {
    publicAccess: 'Blob'
    metadata: {
      purpose: 'DecVCPlat images and media'
      environment: environment
    }
  }
}

resource fontsContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  name: '${storageAccountName}/default/fonts'
  properties: {
    publicAccess: 'Blob'
    metadata: {
      purpose: 'DecVCPlat web fonts'
      environment: environment
    }
  }
}

resource documentsContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-01-01' = {
  name: '${storageAccountName}/default/documents'
  properties: {
    publicAccess: 'None'
    metadata: {
      purpose: 'DecVCPlat project documents (private)'
      environment: environment
    }
  }
}

// CDN purge automation with Logic App
resource cdnPurgeLogicApp 'Microsoft.Logic/workflows@2019-05-01' = {
  name: 'decvcplat-cdn-purge-${environment}'
  location: location
  properties: {
    definition: {
      '$schema': 'https://schema.management.azure.com/providers/Microsoft.Logic/schemas/2016-06-01/workflowdefinition.json#'
      contentVersion: '1.0.0.0'
      parameters: {}
      triggers: {
        manual: {
          type: 'Request'
          kind: 'Http'
          inputs: {
            schema: {
              type: 'object'
              properties: {
                paths: {
                  type: 'array'
                  items: {
                    type: 'string'
                  }
                }
              }
            }
          }
        }
      }
      actions: {
        'Purge-CDN-Content': {
          type: 'Http'
          inputs: {
            method: 'POST'
            uri: 'https://management.azure.com/subscriptions/@{subscription().subscriptionId}/resourceGroups/@{resourceGroup().name}/providers/Microsoft.Cdn/profiles/${cdnProfileName}/endpoints/${cdnEndpointName}/purge?api-version=2023-05-01'
            headers: {
              'Content-Type': 'application/json'
            }
            body: {
              contentPaths: '@triggerBody()?[\'paths\']'
            }
            authentication: {
              type: 'ManagedServiceIdentity'
            }
          }
        }
        'Response': {
          type: 'Response'
          kind: 'Http'
          inputs: {
            statusCode: 200
            body: {
              status: 'success'
              message: 'CDN purge completed'
              timestamp: '@utcNow()'
            }
          }
          runAfter: {
            'Purge-CDN-Content': [
              'Succeeded'
            ]
          }
        }
      }
    }
  }
  identity: {
    type: 'SystemAssigned'
  }
}

// Role assignment for CDN purge
resource cdnPurgeRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(cdnProfile.id, cdnPurgeLogicApp.id, 'CDN Endpoint Contributor')
  scope: cdnProfile
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '426e0c7f-0c7e-4658-b36f-ff54d6c29b45') // CDN Endpoint Contributor
    principalId: cdnPurgeLogicApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// Application Insights for CDN monitoring
resource cdnApplicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'decvcplat-cdn-insights-${environment}'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Request_Source: 'rest'
    WorkspaceResourceId: '/subscriptions/${subscription().subscriptionId}/resourceGroups/${resourceGroup().name}/providers/Microsoft.OperationalInsights/workspaces/decvcplat-logs-${environment}'
  }
}

// Diagnostic settings for CDN
resource cdnDiagnostics 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: 'decvcplat-cdn-diagnostics'
  scope: cdnEndpoint
  properties: {
    workspaceId: '/subscriptions/${subscription().subscriptionId}/resourceGroups/${resourceGroup().name}/providers/Microsoft.OperationalInsights/workspaces/decvcplat-logs-${environment}'
    logs: [
      {
        category: 'CoreAnalytics'
        enabled: true
        retentionPolicy: {
          enabled: true
          days: 30
        }
      }
    ]
    metrics: [
      {
        category: 'AllMetrics'
        enabled: true
        retentionPolicy: {
          enabled: true
          days: 30
        }
      }
    ]
  }
}

output cdnProfileId string = cdnProfile.id
output cdnEndpointId string = cdnEndpoint.id
output cdnEndpointHostName string = cdnEndpoint.properties.hostName
output cdnCustomDomainHostName string = cdnCustomDomain.properties.hostName
output cdnPurgeLogicAppUrl string = cdnPurgeLogicApp.properties.accessEndpoint
output storageContainers array = [
  assetsContainer.name
  imagesContainer.name
  fontsContainer.name
  documentsContainer.name
]
