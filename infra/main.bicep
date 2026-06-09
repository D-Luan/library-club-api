  @description('Base name used for Azure resources.')
  param appName string

  @description('Azure region.')
  param location string = resourceGroup().location

  @description('App Service Plan SKU.')
  param appServiceSku string = 'F1'

  @description('Azure SQL admin login.')
  param sqlAdminLogin string

  @secure()
  @description('Azure SQL admin password.')
  param sqlAdminPassword string

  @description('Optional client IP allowed to access Azure SQL. Leave empty to skip.')
  param allowedClientIp string = ''

  @description('Keep the web app always warm. Use false for Free/learning environments.')
  param alwaysOn bool = false

  var appServicePlanName = '${appName}-plan'
  var webAppName = appName
  var sqlServerName = '${appName}-sql'
  var sqlDatabaseName = '${appName}-db'
  var logAnalyticsName = '${appName}-log'
  var appInsightsName = '${appName}-appi'
  var sqlServerFqdn = sqlServer.properties.fullyQualifiedDomainName

  var defaultConnectionString = concat(join([
    'Server=tcp:${sqlServerFqdn},1433'
    'Initial Catalog=${sqlDatabase.name}'
    'Persist Security Info=False'
    'User ID=${sqlAdminLogin}'
    'Password=${sqlAdminPassword}'
    'MultipleActiveResultSets=False'
    'Encrypt=True'
    'TrustServerCertificate=False'
    'Connection Timeout=30'
  ], ';'), ';')

  resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
    name: appServicePlanName
    location: location
    sku: {
      name: appServiceSku
    }
    kind: 'linux'
    properties: {
      reserved: true
    }
  }

  resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
    name: logAnalyticsName
    location: location
    properties: {
      sku: {
        name: 'PerGB2018'
      }
      retentionInDays: 30
    }
  }

  resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
    name: appInsightsName
    location: location
    kind: 'web'
    properties: {
      Application_Type: 'web'
      WorkspaceResourceId: logAnalytics.id
    }
  }

  resource sqlServer 'Microsoft.Sql/servers@2023-08-01-preview' = {
    name: sqlServerName
    location: location
    properties: {
      administratorLogin: sqlAdminLogin
      administratorLoginPassword: sqlAdminPassword
      publicNetworkAccess: 'Enabled'
    }
  }

  resource allowAzureServicesFirewallRule 'Microsoft.Sql/servers/firewallRules@2023-08-01-preview' = {
    parent: sqlServer
    name: 'AllowAzureServices'
    properties: {
      startIpAddress: '0.0.0.0'
      endIpAddress: '0.0.0.0'
    }
  }

  resource allowClientFirewallRule 'Microsoft.Sql/servers/firewallRules@2023-08-01-preview' = if(!empty(allowedClientIp)) {
    parent: sqlServer
    name: 'AllowClientIp'
    properties: {
      startIpAddress: allowedClientIp
      endIpAddress: allowedClientIp
    }
  }

  resource sqlDatabase 'Microsoft.Sql/servers/databases@2023-08-01-preview' = {
    parent: sqlServer
    name: sqlDatabaseName
    location: location
    sku: {
      name: 'Basic'
      tier: 'Basic'
      capacity: 5
    }
    properties: {
        maxSizeBytes: 2147483648
    }
  }

  resource webApp 'Microsoft.Web/sites@2023-12-01' = {
    name: webAppName
    location: location
    kind: 'app,linux'
    properties: {
      serverFarmId: appServicePlan.id
      httpsOnly: true
      siteConfig: {
        linuxFxVersion: 'DOTNETCORE|10.0'
        alwaysOn: alwaysOn
        ftpsState: 'Disabled'
        minTlsVersion: '1.2'
      }
    }
  }

  resource webAppSettings 'Microsoft.Web/sites/config@2023-12-01' = {
    parent: webApp
    name: 'appsettings'
    properties: {
      APPLICATIONINSIGHTS_CONNECTION_STRING: appInsights.properties.ConnectionString
      ASPNETCORE_ENVIRONMENT: 'Production'
    }
  }

  resource webAppConnectionStrings 'Microsoft.Web/sites/config@2023-12-01' = {
    parent: webApp
    name: 'connectionstrings'
    properties: {
      DefaultConnection: {
        value: defaultConnectionString
        type: 'SQLAzure'
      }
    }
  }

  output webAppName string = webApp.name
  output webAppUrl string = 'https://${webApp.properties.defaultHostName}'
  output sqlServerName string = sqlServer.name
  output sqlDatabaseName string = sqlDatabase.name
  output applicationInsightsName string = appInsights.name
