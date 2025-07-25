@description('TLS/SSL configuration for DecVCPlat platform')
param location string = resourceGroup().location
param environment string = 'dev'
param keyVaultName string
param applicationGatewayName string = 'decvcplat-appgw-${environment}'
param frontDoorName string = 'decvcplat-fd-${environment}'

// Key Vault for SSL certificates
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

// SSL Certificate for DecVCPlat domain
resource sslCertificate 'Microsoft.KeyVault/vaults/certificates@2023-07-01' = {
  parent: keyVault
  name: 'decvcplat-ssl-cert'
  properties: {
    certificatePolicy: {
      issuerParameters: {
        name: 'Self'
      }
      keyProperties: {
        exportable: true
        keySize: 2048
        keyType: 'RSA'
        reuseKey: false
      }
      lifetimeActions: [
        {
          action: {
            actionType: 'AutoRenew'
          }
          trigger: {
            daysBeforeExpiry: 30
          }
        }
      ]
      secretProperties: {
        contentType: 'application/x-pkcs12'
      }
      x509CertificateProperties: {
        keyUsage: [
          'cRLSign'
          'dataEncipherment'
          'digitalSignature'
          'keyAgreement'
          'keyCertSign'
          'keyEncipherment'
        ]
        subject: 'CN=decvcplat.com'
        subjectAlternativeNames: {
          dnsNames: [
            'decvcplat.com'
            'www.decvcplat.com'
            'api.decvcplat.com'
            '*.decvcplat.com'
          ]
        }
        validityInMonths: 12
      }
    }
  }
}

// Application Gateway with SSL termination
resource applicationGateway 'Microsoft.Network/applicationGateways@2023-09-01' = {
  name: applicationGatewayName
  location: location
  properties: {
    sku: {
      name: 'WAF_v2'
      tier: 'WAF_v2'
      capacity: 2
    }
    gatewayIPConfigurations: [
      {
        name: 'appGatewayIpConfig'
        properties: {
          subnet: {
            id: '/subscriptions/{subscription-id}/resourceGroups/{rg-name}/providers/Microsoft.Network/virtualNetworks/{vnet-name}/subnets/{subnet-name}'
          }
        }
      }
    ]
    sslCertificates: [
      {
        name: 'decvcplat-ssl-cert'
        properties: {
          keyVaultSecretId: sslCertificate.properties.secretId
        }
      }
    ]
    frontendIPConfigurations: [
      {
        name: 'appGwPublicFrontendIp'
        properties: {
          publicIPAddress: {
            id: '/subscriptions/{subscription-id}/resourceGroups/{rg-name}/providers/Microsoft.Network/publicIPAddresses/{pip-name}'
          }
        }
      }
    ]
    frontendPorts: [
      {
        name: 'port_80'
        properties: {
          port: 80
        }
      }
      {
        name: 'port_443'
        properties: {
          port: 443
        }
      }
    ]
    backendAddressPools: [
      {
        name: 'decvcplat-backend-pool'
        properties: {
          backendAddresses: [
            {
              fqdn: 'decvcplat-aks.${location}.cloudapp.azure.com'
            }
          ]
        }
      }
    ]
    backendHttpSettingsCollection: [
      {
        name: 'decvcplat-https-settings'
        properties: {
          port: 443
          protocol: 'Https'
          cookieBasedAffinity: 'Disabled'
          pickHostNameFromBackendAddress: true
          requestTimeout: 30
          probe: {
            id: '/subscriptions/{subscription-id}/resourceGroups/{rg-name}/providers/Microsoft.Network/applicationGateways/${applicationGatewayName}/probes/decvcplat-https-probe'
          }
        }
      }
    ]
    httpListeners: [
      {
        name: 'decvcplat-http-listener'
        properties: {
          frontendIPConfiguration: {
            id: '/subscriptions/{subscription-id}/resourceGroups/{rg-name}/providers/Microsoft.Network/applicationGateways/${applicationGatewayName}/frontendIPConfigurations/appGwPublicFrontendIp'
          }
          frontendPort: {
            id: '/subscriptions/{subscription-id}/resourceGroups/{rg-name}/providers/Microsoft.Network/applicationGateways/${applicationGatewayName}/frontendPorts/port_80'
          }
          protocol: 'Http'
          hostName: 'decvcplat.com'
        }
      }
      {
        name: 'decvcplat-https-listener'
        properties: {
          frontendIPConfiguration: {
            id: '/subscriptions/{subscription-id}/resourceGroups/{rg-name}/providers/Microsoft.Network/applicationGateways/${applicationGatewayName}/frontendIPConfigurations/appGwPublicFrontendIp'
          }
          frontendPort: {
            id: '/subscriptions/{subscription-id}/resourceGroups/{rg-name}/providers/Microsoft.Network/applicationGateways/${applicationGatewayName}/frontendPorts/port_443'
          }
          protocol: 'Https'
          hostName: 'decvcplat.com'
          sslCertificate: {
            id: '/subscriptions/{subscription-id}/resourceGroups/{rg-name}/providers/Microsoft.Network/applicationGateways/${applicationGatewayName}/sslCertificates/decvcplat-ssl-cert'
          }
        }
      }
    ]
    requestRoutingRules: [
      {
        name: 'decvcplat-http-redirect-rule'
        properties: {
          ruleType: 'Basic'
          priority: 100
          httpListener: {
            id: '/subscriptions/{subscription-id}/resourceGroups/{rg-name}/providers/Microsoft.Network/applicationGateways/${applicationGatewayName}/httpListeners/decvcplat-http-listener'
          }
          redirectConfiguration: {
            id: '/subscriptions/{subscription-id}/resourceGroups/{rg-name}/providers/Microsoft.Network/applicationGateways/${applicationGatewayName}/redirectConfigurations/decvcplat-https-redirect'
          }
        }
      }
      {
        name: 'decvcplat-https-routing-rule'
        properties: {
          ruleType: 'Basic'
          priority: 200
          httpListener: {
            id: '/subscriptions/{subscription-id}/resourceGroups/{rg-name}/providers/Microsoft.Network/applicationGateways/${applicationGatewayName}/httpListeners/decvcplat-https-listener'
          }
          backendAddressPool: {
            id: '/subscriptions/{subscription-id}/resourceGroups/{rg-name}/providers/Microsoft.Network/applicationGateways/${applicationGatewayName}/backendAddressPools/decvcplat-backend-pool'
          }
          backendHttpSettings: {
            id: '/subscriptions/{subscription-id}/resourceGroups/{rg-name}/providers/Microsoft.Network/applicationGateways/${applicationGatewayName}/backendHttpSettingsCollection/decvcplat-https-settings'
          }
        }
      }
    ]
    redirectConfigurations: [
      {
        name: 'decvcplat-https-redirect'
        properties: {
          redirectType: 'Permanent'
          targetListener: {
            id: '/subscriptions/{subscription-id}/resourceGroups/{rg-name}/providers/Microsoft.Network/applicationGateways/${applicationGatewayName}/httpListeners/decvcplat-https-listener'
          }
          includePath: true
          includeQueryString: true
        }
      }
    ]
    probes: [
      {
        name: 'decvcplat-https-probe'
        properties: {
          protocol: 'Https'
          host: 'decvcplat.com'
          path: '/health'
          interval: 30
          timeout: 30
          unhealthyThreshold: 3
          pickHostNameFromBackendHttpSettings: false
          minServers: 0
          match: {
            statusCodes: [
              '200-399'
            ]
          }
        }
      }
    ]
    webApplicationFirewallConfiguration: {
      enabled: true
      firewallMode: 'Prevention'
      ruleSetType: 'OWASP'
      ruleSetVersion: '3.2'
      disabledRuleGroups: []
      requestBodyCheck: true
      maxRequestBodySizeInKb: 128
      fileUploadLimitInMb: 100
    }
  }
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '/subscriptions/{subscription-id}/resourceGroups/{rg-name}/providers/Microsoft.ManagedIdentity/userAssignedIdentities/{identity-name}': {}
    }
  }
}

// Azure Front Door for global SSL termination and CDN
resource frontDoor 'Microsoft.Cdn/profiles@2023-05-01' = {
  name: frontDoorName
  location: 'Global'
  sku: {
    name: 'Premium_AzureFrontDoor'
  }
  properties: {
    originResponseTimeoutSeconds: 60
  }
}

// Front Door endpoint
resource frontDoorEndpoint 'Microsoft.Cdn/profiles/afdEndpoints@2023-05-01' = {
  parent: frontDoor
  name: 'decvcplat-endpoint'
  location: 'Global'
  properties: {
    enabledState: 'Enabled'
  }
}

// Origin group for backend services
resource originGroup 'Microsoft.Cdn/profiles/originGroups@2023-05-01' = {
  parent: frontDoor
  name: 'decvcplat-origin-group'
  properties: {
    loadBalancingSettings: {
      sampleSize: 4
      successfulSamplesRequired: 3
      additionalLatencyInMilliseconds: 50
    }
    healthProbeSettings: {
      probePath: '/health'
      probeRequestType: 'HEAD'
      probeProtocol: 'Https'
      probeIntervalInSeconds: 100
    }
  }
}

// Origin pointing to Application Gateway
resource origin 'Microsoft.Cdn/profiles/originGroups/origins@2023-05-01' = {
  parent: originGroup
  name: 'decvcplat-appgw-origin'
  properties: {
    hostName: 'decvcplat.com'
    httpPort: 80
    httpsPort: 443
    originHostHeader: 'decvcplat.com'
    priority: 1
    weight: 1000
    enabledState: 'Enabled'
    enforceCertificateNameCheck: true
  }
}

// Route for HTTPS traffic
resource route 'Microsoft.Cdn/profiles/afdEndpoints/routes@2023-05-01' = {
  parent: frontDoorEndpoint
  name: 'decvcplat-https-route'
  properties: {
    customDomains: [
      {
        id: customDomain.id
      }
    ]
    originGroup: {
      id: originGroup.id
    }
    supportedProtocols: [
      'Http'
      'Https'
    ]
    patternsToMatch: [
      '/*'
    ]
    forwardingProtocol: 'HttpsOnly'
    linkToDefaultDomain: 'Enabled'
    httpsRedirect: 'Enabled'
  }
  dependsOn: [
    origin
  ]
}

// Custom domain for Front Door
resource customDomain 'Microsoft.Cdn/profiles/customDomains@2023-05-01' = {
  parent: frontDoor
  name: 'decvcplat-custom-domain'
  properties: {
    hostName: 'decvcplat.com'
    tlsSettings: {
      certificateType: 'ManagedCertificate'
      minimumTlsVersion: 'TLS12'
    }
  }
}

// Security policy for custom domain
resource securityPolicy 'Microsoft.Cdn/profiles/securityPolicies@2023-05-01' = {
  parent: frontDoor
  name: 'decvcplat-security-policy'
  properties: {
    parameters: {
      type: 'WebApplicationFirewall'
      wafPolicy: {
        id: wafPolicy.id
      }
      associations: [
        {
          domains: [
            {
              id: customDomain.id
            }
          ]
          patternsToMatch: [
            '/*'
          ]
        }
      ]
    }
  }
}

// WAF Policy for Front Door
resource wafPolicy 'Microsoft.Network/FrontDoorWebApplicationFirewallPolicies@2022-05-01' = {
  name: 'decvcplatWafPolicy'
  location: 'Global'
  sku: {
    name: 'Premium_AzureFrontDoor'
  }
  properties: {
    policySettings: {
      enabledState: 'Enabled'
      mode: 'Prevention'
      redirectUrl: 'https://decvcplat.com/blocked'
      customBlockResponseStatusCode: 403
      customBlockResponseBody: 'Access denied by DecVCPlat security policy'
    }
    customRules: {
      rules: [
        {
          name: 'RateLimitRule'
          priority: 1
          ruleType: 'RateLimitRule'
          rateLimitDurationInMinutes: 1
          rateLimitThreshold: 100
          matchConditions: [
            {
              matchVariable: 'RemoteAddr'
              operator: 'IPMatch'
              matchValue: [
                '0.0.0.0/0'
              ]
            }
          ]
          action: 'Block'
        }
        {
          name: 'GeoBlockRule'
          priority: 2
          ruleType: 'MatchRule'
          matchConditions: [
            {
              matchVariable: 'RemoteAddr'
              operator: 'GeoMatch'
              matchValue: [
                'CN'
                'RU'
                'KP'
              ]
            }
          ]
          action: 'Block'
        }
      ]
    }
    managedRules: {
      managedRuleSets: [
        {
          ruleSetType: 'Microsoft_DefaultRuleSet'
          ruleSetVersion: '2.1'
          ruleGroupOverrides: []
        }
        {
          ruleSetType: 'Microsoft_BotManagerRuleSet'
          ruleSetVersion: '1.0'
          ruleGroupOverrides: []
        }
      ]
    }
  }
}

// Data encryption at rest configuration
resource diskEncryptionSet 'Microsoft.Compute/diskEncryptionSets@2023-04-02' = {
  name: 'decvcplat-disk-encryption-${environment}'
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    activeKey: {
      sourceVault: {
        id: keyVault.id
      }
      keyUrl: '${keyVault.properties.vaultUri}keys/decvcplat-disk-encryption-key'
    }
    encryptionType: 'EncryptionAtRestWithCustomerKey'
  }
}

// Key Vault access policy for disk encryption
resource keyVaultAccessPolicy 'Microsoft.KeyVault/vaults/accessPolicies@2023-07-01' = {
  parent: keyVault
  name: 'add'
  properties: {
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: diskEncryptionSet.identity.principalId
        permissions: {
          keys: [
            'get'
            'wrapKey'
            'unwrapKey'
          ]
        }
      }
    ]
  }
}

output applicationGatewayId string = applicationGateway.id
output frontDoorId string = frontDoor.id
output sslCertificateSecretId string = sslCertificate.properties.secretId
output diskEncryptionSetId string = diskEncryptionSet.id
output wafPolicyId string = wafPolicy.id
