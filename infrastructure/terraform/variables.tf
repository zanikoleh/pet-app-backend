# Azure Region Variables
variable "location" {
  type        = string
  description = "Azure region where resources will be deployed"
  default     = "Poland Central"
}

variable "resource_group_name" {
  type        = string
  description = "Name of the Azure Resource Group"
  default     = "petapp_rsrc_grp"
}

# Environment Configuration
variable "environment" {
  type        = string
  description = "Environment name (dev, staging, prod)"
  default     = "dev"

  validation {
    condition     = contains(["dev", "staging", "prod"], var.environment)
    error_message = "Environment must be dev, staging, or prod."
  }
}

# ACR Configuration
variable "acr_name" {
  type        = string
  description = "Name of the Azure Container Registry"
  default     = "oznkpetpp"

  validation {
    condition     = can(regex("^[a-z0-9]+$", var.acr_name)) && length(var.acr_name) >= 5 && length(var.acr_name) <= 50
    error_message = "ACR name must be 5-50 lowercase alphanumeric characters."
  }
}

variable "acr_sku" {
  type        = string
  description = "SKU of the Azure Container Registry (Basic, Standard, Premium)"
  default     = "Basic"

  validation {
    condition     = contains(["Basic", "Standard", "Premium"], var.acr_sku)
    error_message = "ACR SKU must be Basic, Standard, or Premium."
  }
}

# Storage Account for State Configuration
variable "create_terraform_state_storage" {
  type        = bool
  description = "Whether to create storage account for remote Terraform state"
  default     = false
}

variable "storage_account_name" {
  type        = string
  description = "Name of the storage account for Terraform state"
  default     = "petstatestore"

  validation {
    condition     = can(regex("^[a-z0-9]+$", var.storage_account_name)) && length(var.storage_account_name) >= 3 && length(var.storage_account_name) <= 24
    error_message = "Storage account name must be 3-24 lowercase alphanumeric characters."
  }
}

# Container Instances Configuration
variable "create_aci_demo" {
  type        = bool
  description = "Whether to create Azure Container Instances for demo purposes"
  default     = false
}

variable "service_name" {
  type        = string
  description = "Name of the service to deploy"
  default     = "pet-service"
}

variable "image_version" {
  type        = string
  description = "Docker image version tag"
  default     = "0.1.0"
}

# Database Configuration
variable "create_database" {
  type        = bool
  description = "Whether to create PostgreSQL database"
  default     = false
}

variable "database_name" {
  type        = string
  description = "Name of the PostgreSQL server"
  default     = "petapp-db"

  validation {
    condition     = can(regex("^[a-z0-9-]+$", var.database_name)) && length(var.database_name) >= 3 && length(var.database_name) <= 63
    error_message = "Database name must be 3-63 lowercase alphanumeric characters and hyphens."
  }
}

variable "db_admin_username" {
  type        = string
  description = "PostgreSQL administrator username"
  default     = "psqladmin"
  sensitive   = true

  validation {
    condition     = length(var.db_admin_username) >= 1
    error_message = "Database admin username cannot be empty."
  }
}

variable "db_admin_password" {
  type        = string
  description = "PostgreSQL administrator password"
  sensitive   = true

  validation {
    condition     = length(var.db_admin_password) >= 8
    error_message = "Database password must be at least 8 characters."
  }
}

variable "db_sku_name" {
  type        = string
  description = "PostgreSQL server SKU (e.g., B_Gen5_1, GP_Gen5_2)"
  default     = "B_Gen5_1"
}

variable "db_storage_mb" {
  type        = number
  description = "PostgreSQL storage in MB"
  default     = 51200 # 50GB

  validation {
    condition     = var.db_storage_mb >= 51200 && var.db_storage_mb <= 1048576
    error_message = "Database storage must be between 50GB and 1TB."
  }
}

# Service Names (for future expansion)
variable "services" {
  type        = list(string)
  description = "List of microservices"
  default = [
    "pet-service",
    "identity-service",
    "file-service",
    "notification-service",
    "user-service",
    "gateway-api"
  ]
}
