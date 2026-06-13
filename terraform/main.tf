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
  name     = "rg-${var.project}-${var.environment}"
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
# App Service Plan (Linux)
# ─────────────────────────────────────────
resource "azurerm_service_plan" "main" {
  name                = "asp-${var.project}-${var.environment}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  os_type             = "Linux"
  sku_name            = "B2"
  tags                = local.tags
}

# ─────────────────────────────────────────
# App Service — .NET API
# ─────────────────────────────────────────
resource "azurerm_linux_web_app" "api" {
  name                = "app-${var.project}-api-${var.environment}-${local.suffix}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  service_plan_id     = azurerm_service_plan.main.id
  tags                = local.tags

  site_config {
    always_on = true

    application_stack {
      docker_image_name        = "ticketflow-api:latest"
      docker_registry_url      = "https://${azurerm_container_registry.acr.login_server}"
      docker_registry_username = azurerm_container_registry.acr.admin_username
      docker_registry_password = azurerm_container_registry.acr.admin_password
    }
  }

  app_settings = {
    ASPNETCORE_ENVIRONMENT               = "Production"
    ConnectionStrings__Default           = "Server=${azurerm_mssql_server.main.fully_qualified_domain_name};Database=${azurerm_mssql_database.main.name};User Id=${var.sql_admin_login};Password=${var.sql_admin_password};Encrypt=True;"
    ConnectionStrings__Redis             = "localhost:6379,abortConnect=false"
    AzureAd__TenantId                    = var.tenant_id
    WEBSITES_ENABLE_APP_SERVICE_STORAGE  = "false"
  }

  identity {
    type = "SystemAssigned"
  }
}

# ─────────────────────────────────────────
# App Service — Angular Frontend
# ─────────────────────────────────────────
resource "azurerm_linux_web_app" "web" {
  name                = "app-${var.project}-web-${var.environment}-${local.suffix}"
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  service_plan_id     = azurerm_service_plan.main.id
  tags                = local.tags

  site_config {
    always_on = true

    application_stack {
      docker_image_name        = "ticketflow-web:latest"
      docker_registry_url      = "https://${azurerm_container_registry.acr.login_server}"
      docker_registry_username = azurerm_container_registry.acr.admin_username
      docker_registry_password = azurerm_container_registry.acr.admin_password
    }
  }

  app_settings = {
    WEBSITES_ENABLE_APP_SERVICE_STORAGE = "false"
  }
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