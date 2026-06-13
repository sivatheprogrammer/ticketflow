variable "subscription_id" {
  description = "Azure Subscription ID"
  type        = string
  sensitive   = true
}

variable "tenant_id" {
  description = "Azure Tenant ID"
  type        = string
  sensitive   = true
}

variable "location" {
  description = "Azure region for all resources"
  type        = string
  default     = "eastus"
}

variable "environment" {
  description = "Environment name"
  type        = string
  default     = "dev"
}

variable "project" {
  description = "Project name prefix for all resources"
  type        = string
  default     = "ticketflow"
}

variable "sql_admin_login" {
  description = "Azure SQL administrator login"
  type        = string
  default     = "tfadmin"
}

variable "sql_admin_password" {
  description = "Azure SQL administrator password"
  type        = string
  sensitive   = true
}

variable "suffix" {
  description = "Fixed suffix for globally unique resource names"
  type        = string
  default     = "siva04"
}
