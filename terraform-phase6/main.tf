locals {
  suffix = var.suffix
  tags = {
    project     = var.project
    environment = var.environment
    managed_by  = "terraform"
  }
}

# ─────────────────────────────────────────
# Resource Group
# ─────────────────────────────────────────
resource "azurerm_resource_group" "main" {
  name     = "rg-${var.project}-${var.environment}-${local.suffix}"
  location = var.location
  tags     = local.tags
}

# ─────────────────────────────────────────
# Azure Container Registry
# ─────────────────────────────────────────
resource "azurerm_container_registry" "acr" {
  name                = "acr${var.project}${var.environment}${local.suffix}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  sku                 = "Basic"
  admin_enabled       = true
  tags                = local.tags
}

# ─────────────────────────────────────────
# Azure SQL Server + Database
# ─────────────────────────────────────────
resource "azurerm_mssql_server" "main" {
  name                         = "sql-${var.project}-${var.environment}-${local.suffix}"
  resource_group_name          = azurerm_resource_group.main.name
  location                     = azurerm_resource_group.main.location
  version                      = "12.0"
  administrator_login          = var.sql_admin_login
  administrator_login_password = var.sql_admin_password
  tags                         = local.tags
}

resource "azurerm_mssql_firewall_rule" "allow_azure" {
  name             = "AllowAzureServices"
  server_id        = azurerm_mssql_server.main.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

resource "azurerm_mssql_database" "main" {
  name        = "sqldb-${var.project}-${var.environment}"
  server_id   = azurerm_mssql_server.main.id
  collation   = "SQL_Latin1_General_CP1_CI_AS"
  sku_name    = "Basic"
  max_size_gb = 2
  tags        = local.tags
}

resource "azurerm_mssql_database" "identity" {
  name        = "sqldb-ticketflow-identity-dev"
  server_id   = azurerm_mssql_server.main.id
  collation   = "SQL_Latin1_General_CP1_CI_AS"
  sku_name    = "Basic"
  max_size_gb = 2
  tags        = local.tags
}

# ─────────────────────────────────────────
# Azure Service Bus (real — not emulator)
# ─────────────────────────────────────────
resource "azurerm_servicebus_namespace" "main" {
  name                = "sb-${var.project}-${var.environment}-${local.suffix}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  sku                 = "Standard"
  tags                = local.tags
}

resource "azurerm_servicebus_queue" "booking_created" {
  name         = "booking-created"
  namespace_id = azurerm_servicebus_namespace.main.id
}

resource "azurerm_servicebus_queue" "booking_confirmed" {
  name         = "booking-confirmed"
  namespace_id = azurerm_servicebus_namespace.main.id
}

# ─────────────────────────────────────────
# Log Analytics (required for ACA)
# ─────────────────────────────────────────
resource "azurerm_log_analytics_workspace" "main" {
  name                = "log-${var.project}-${var.environment}-${local.suffix}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  sku                 = "PerGB2018"
  retention_in_days   = 30
  tags                = local.tags
}

# ─────────────────────────────────────────
# Azure Container Apps Environment
# ─────────────────────────────────────────
resource "azurerm_container_app_environment" "main" {
  name                       = "cae-${var.project}-${var.environment}-${local.suffix}"
  resource_group_name        = azurerm_resource_group.main.name
  location                   = azurerm_resource_group.main.location
  log_analytics_workspace_id = azurerm_log_analytics_workspace.main.id
  tags                       = local.tags
}

# ─────────────────────────────────────────
# ACA — TicketFlow API
# ─────────────────────────────────────────
resource "azurerm_container_app" "api" {
  name                         = "ca-ticketflow-api-${local.suffix}"
  resource_group_name          = azurerm_resource_group.main.name
  container_app_environment_id = azurerm_container_app_environment.main.id
  revision_mode                = "Single"
  tags                         = local.tags

  registry {
    server               = azurerm_container_registry.acr.login_server
    username             = azurerm_container_registry.acr.admin_username
    password_secret_name = "acr-password"
  }

  secret {
    name  = "acr-password"
    value = azurerm_container_registry.acr.admin_password
  }

  secret {
    name  = "sql-connection"
    value = "Server=${azurerm_mssql_server.main.fully_qualified_domain_name};Database=${azurerm_mssql_database.main.name};User Id=${var.sql_admin_login};Password=${var.sql_admin_password};Encrypt=True;"
  }

  secret {
    name  = "servicebus-connection"
    value = azurerm_servicebus_namespace.main.default_primary_connection_string
  }

  template {
    container {
      name   = "ticketflow-api"
      image  = "${azurerm_container_registry.acr.login_server}/ticketflow-api:latest"
      cpu    = 0.5
      memory = "1Gi"

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = "Production"
      }
      env {
        name        = "ConnectionStrings__Default"
        secret_name = "sql-connection"
      }
      env {
        name        = "ServiceBus__ConnectionString"
        secret_name = "servicebus-connection"
      }
      env {
        name  = "AzureAd__TenantId"
        value = var.tenant_id
      }
    }

    min_replicas = 1
    max_replicas = 3
  }

  ingress {
    external_enabled = false
    target_port      = 8080
    traffic_weight {
      percentage      = 100
      latest_revision = true
    }
  }
}

# ─────────────────────────────────────────
# ACA — TicketFlow Identity API
# ─────────────────────────────────────────
resource "azurerm_container_app" "identity" {
  name                         = "ca-ticketflow-identity-${local.suffix}"
  resource_group_name          = azurerm_resource_group.main.name
  container_app_environment_id = azurerm_container_app_environment.main.id
  revision_mode                = "Single"
  tags                         = local.tags

  registry {
    server               = azurerm_container_registry.acr.login_server
    username             = azurerm_container_registry.acr.admin_username
    password_secret_name = "acr-password"
  }

  secret {
    name  = "acr-password"
    value = azurerm_container_registry.acr.admin_password
  }

  secret {
    name  = "sql-connection"
    value = "Server=${azurerm_mssql_server.main.fully_qualified_domain_name};Database=${azurerm_mssql_database.main.name};User Id=${var.sql_admin_login};Password=${var.sql_admin_password};Encrypt=True;"
  }

  template {
    container {
      name   = "ticketflow-identity"
      image  = "${azurerm_container_registry.acr.login_server}/ticketflow-identity:latest"
      cpu    = 0.25
      memory = "0.5Gi"

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = "Production"
      }
      env {
        name        = "ConnectionStrings__Default"
        secret_name = "sql-connection"
      }
      env {
        name  = "AzureAd__TenantId"
        value = var.tenant_id
      }
    }

    min_replicas = 1
    max_replicas = 2
  }

  ingress {
    external_enabled = false
    target_port      = 8080
    traffic_weight {
      percentage      = 100
      latest_revision = true
    }
  }
}

# ─────────────────────────────────────────
# ACA — TicketFlow Gateway (public ingress)
# ─────────────────────────────────────────
resource "azurerm_container_app" "gateway" {
  name                         = "ca-ticketflow-gateway-${local.suffix}"
  resource_group_name          = azurerm_resource_group.main.name
  container_app_environment_id = azurerm_container_app_environment.main.id
  revision_mode                = "Single"
  tags                         = local.tags

  registry {
    server               = azurerm_container_registry.acr.login_server
    username             = azurerm_container_registry.acr.admin_username
    password_secret_name = "acr-password"
  }

  secret {
    name  = "acr-password"
    value = azurerm_container_registry.acr.admin_password
  }

  template {
    container {
      name   = "ticketflow-gateway"
      image  = "${azurerm_container_registry.acr.login_server}/ticketflow-gateway:latest"
      cpu    = 0.25
      memory = "0.5Gi"

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = "Production"
      }
      env {
        name  = "ReverseProxy__Clusters__api-cluster__Destinations__main-api__Address"
        value = "https://${azurerm_container_app.api.ingress[0].fqdn}"
      }
      env {
        name  = "ReverseProxy__Clusters__identity-cluster__Destinations__identity-api__Address"
        value = "https://${azurerm_container_app.identity.ingress[0].fqdn}"
      }
    }

    min_replicas = 1
    max_replicas = 2
  }

  ingress {
    external_enabled = true
    target_port      = 8080
    traffic_weight {
      percentage      = 100
      latest_revision = true
    }
  }
}

# ─────────────────────────────────────────
# AKS Cluster (demonstration track)
# ─────────────────────────────────────────
resource "azurerm_kubernetes_cluster" "main" {
  name                = "aks-${var.project}-${var.environment}-${local.suffix}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  dns_prefix          = "${var.project}-${local.suffix}"
  tags                = local.tags

  default_node_pool {
    name       = "default"
    node_count = 1
    vm_size    = "Standard_D2s_v3"
  }

  identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_role_assignment" "aks_acr_pull" {
  principal_id                     = azurerm_kubernetes_cluster.main.kubelet_identity[0].object_id
  role_definition_name             = "AcrPull"
  scope                            = azurerm_container_registry.acr.id
  skip_service_principal_aad_check = true
}
