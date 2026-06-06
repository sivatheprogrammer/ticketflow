output "resource_group_name" {
  description = "Resource group name"
  value       = azurerm_resource_group.main.name
}

output "acr_login_server" {
  description = "ACR login server URL"
  value       = azurerm_container_registry.acr.login_server
}

output "acr_admin_username" {
  description = "ACR admin username"
  value       = azurerm_container_registry.acr.admin_username
  sensitive   = true
}

output "api_url" {
  description = "API App Service URL"
  value       = "https://${azurerm_linux_web_app.api.default_hostname}"
}

output "web_url" {
  description = "Angular App Service URL"
  value       = "https://${azurerm_linux_web_app.web.default_hostname}"
}

output "sql_server_fqdn" {
  description = "Azure SQL Server fully qualified domain name"
  value       = azurerm_mssql_server.main.fully_qualified_domain_name
}
