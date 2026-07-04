output "acr_login_server" {
  description = "ACR login server URL"
  value       = azurerm_container_registry.acr.login_server
}

output "acr_admin_username" {
  description = "ACR admin username"
  value       = azurerm_container_registry.acr.admin_username
  sensitive   = true
}

output "gateway_url" {
  description = "Public URL for the ACA Gateway"
  value       = "https://${azurerm_container_app.gateway.ingress[0].fqdn}"
}

output "sql_server_fqdn" {
  description = "Azure SQL Server FQDN"
  value       = azurerm_mssql_server.main.fully_qualified_domain_name
}

output "servicebus_connection_string" {
  description = "Azure Service Bus connection string"
  value       = azurerm_servicebus_namespace.main.default_primary_connection_string
  sensitive   = true
}

output "resource_group_name" {
  description = "Resource group name"
  value       = azurerm_resource_group.main.name
}