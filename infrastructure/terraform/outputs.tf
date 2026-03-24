# Resource Group Outputs
output "resource_group_name" {
  value       = azurerm_resource_group.pet_app.name
  description = "Name of the created resource group"
}

output "resource_group_id" {
  value       = azurerm_resource_group.pet_app.id
  description = "ID of the created resource group"
}

# ACR Outputs
output "acr_login_server" {
  value       = azurerm_container_registry.acr.login_server
  description = "Login server URL for Azure Container Registry"
}

output "acr_id" {
  value       = azurerm_container_registry.acr.id
  description = "ID of the Azure Container Registry"
}

output "acr_admin_username" {
  value       = azurerm_container_registry.acr.admin_username
  sensitive   = false
  description = "Admin username for ACR"
}

output "acr_admin_password" {
  value       = azurerm_container_registry.acr.admin_password
  sensitive   = true
  description = "Admin password for ACR (sensitive)"
}

output "acr_sku" {
  value       = azurerm_container_registry.acr.sku
  description = "SKU of the Azure Container Registry"
}

# Storage Account Outputs (if created)
output "storage_account_id" {
  value       = var.create_terraform_state_storage ? azurerm_storage_account.terraform_state[0].id : null
  description = "ID of the storage account (if created)"
}

output "storage_account_name" {
  value       = var.create_terraform_state_storage ? azurerm_storage_account.terraform_state[0].name : null
  description = "Name of the storage account (if created)"
}

# ACI Outputs (if created)
output "aci_fqdn" {
  value       = var.create_aci_demo ? azurerm_container_group.pet_service[0].fqdn : null
  description = "FQDN of the Container Instance (if created)"
}

output "aci_ip_address" {
  value       = var.create_aci_demo ? azurerm_container_group.pet_service[0].ip_address : null
  description = "Public IP address of the Container Instance (if created)"
}

# Database Outputs (if created)
output "database_server_fqdn" {
  value       = var.create_database ? azurerm_postgresql_server.pet_app_db[0].fqdn : null
  description = "FQDN of the PostgreSQL server (if created)"
}

output "database_server_id" {
  value       = var.create_database ? azurerm_postgresql_server.pet_app_db[0].id : null
  description = "ID of the PostgreSQL server (if created)"
}

output "database_administrator_login" {
  value       = var.create_database ? azurerm_postgresql_server.pet_app_db[0].administrator_login : null
  sensitive   = true
  description = "Administrator login for the database (if created)"
}

# Useful Commands
output "docker_push_example" {
  value       = "docker tag <image-name> ${azurerm_container_registry.acr.login_server}/<repository-name>:0.1.0"
  description = "Example docker tag command for ACR"
}

output "docker_login_command" {
  value       = "az acr login --name ${var.acr_name}"
  description = "Command to login to ACR using Azure CLI"
}

output "next_steps" {
  value = <<-EOT
    ✅ Infrastructure deployed successfully!

    Next steps:
    1. Tags and push your images:
       docker tag pet-service:latest ${azurerm_container_registry.acr.login_server}/pet-service:0.1.0
       docker push ${azurerm_container_registry.acr.login_server}/pet-service:0.1.0

    2. To list repositories:
       az acr repository list --name ${var.acr_name}

    3. To pull an image:
       docker pull ${azurerm_container_registry.acr.login_server}/<repo>:0.1.0

    4. To enable remote state (production):
       Uncomment the backend block in main.tf and run:
       terraform init -migrate-state
  EOT
  description = "Next steps after deployment"
}
