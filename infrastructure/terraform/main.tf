terraform {
  required_version = ">= 1.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0"
    }
  }

  # Uncomment to use remote state (recommended for production)
  # backend "azurerm" {
  #   resource_group_name  = "petapp_rsrc_grp"
  #   storage_account_name = "petstatestore"
  #   container_name       = "tfstate"
  #   key                  = "prod.terraform.tfstate"
  # }
}

provider "azurerm" {
  features {}
}

# Resource Group
resource "azurerm_resource_group" "pet_app" {
  name     = var.resource_group_name
  location = var.location

  tags = {
    Environment = var.environment
    Project     = "PetApp"
    ManagedBy   = "Terraform"
  }
}

# Azure Container Registry
resource "azurerm_container_registry" "acr" {
  name                = var.acr_name
  resource_group_name = azurerm_resource_group.pet_app.name
  location            = azurerm_resource_group.pet_app.location
  sku                 = var.acr_sku
  admin_enabled       = true

  tags = {
    Component = "ContainerRegistry"
    Purpose   = "MicroservicesRegistry"
  }
}

# Storage Account for Terraform State (optional, for production)
resource "azurerm_storage_account" "terraform_state" {
  count                    = var.create_terraform_state_storage ? 1 : 0
  name                     = var.storage_account_name
  resource_group_name      = azurerm_resource_group.pet_app.name
  location                 = azurerm_resource_group.pet_app.location
  account_tier             = "Standard"
  account_replication_type = "LRS"

  tags = {
    Component = "StateStorage"
  }
}

resource "azurerm_storage_container" "terraform_state_container" {
  count                 = var.create_terraform_state_storage ? 1 : 0
  name                  = "tfstate"
  storage_account_name  = azurerm_storage_account.terraform_state[0].name
  container_access_type = "private"
}

# Optional: Azure Container Instances for simple deployments (lightweight alternative to AKS)
resource "azurerm_container_group" "pet_service" {
  count               = var.create_aci_demo ? 1 : 0
  name                = "${var.service_name}-aci-group"
  location            = azurerm_resource_group.pet_app.location
  resource_group_name = azurerm_resource_group.pet_app.name
  os_type             = "Linux"
  ip_address_type     = "Public"
  restart_policy      = "Always"

  container {
    name   = var.service_name
    image  = "${azurerm_container_registry.acr.login_server}/${var.service_name}:${var.image_version}"
    cpu    = "1"
    memory = "1.5"

    environment_variables = {
      ENVIRONMENT = var.environment
    }

    ports {
      port     = 5000
      protocol = "TCP"
    }
  }

  image_registry_credential {
    username = azurerm_container_registry.acr.admin_username
    password = azurerm_container_registry.acr.admin_password
    server   = azurerm_container_registry.acr.login_server
  }

  tags = {
    Component = "PetService"
  }

  depends_on = [azurerm_container_registry.acr]
}

# Optional: Azure Database for PostgreSQL (for microservices)
resource "azurerm_postgresql_server" "pet_app_db" {
  count               = var.create_database ? 1 : 0
  name                = var.database_name
  location            = azurerm_resource_group.pet_app.location
  resource_group_name = azurerm_resource_group.pet_app.name

  administrator_login          = var.db_admin_username
  administrator_login_password = var.db_admin_password

  sku_name                     = var.db_sku_name
  storage_mb                   = var.db_storage_mb
  backup_retention_days        = 7
  ssl_enforcement_enabled      = true
  version                      = "11"

  tags = {
    Component = "Database"
  }
}

# Firewall rule to allow Azure services
resource "azurerm_postgresql_firewall_rule" "allow_azure_services" {
  count               = var.create_database ? 1 : 0
  name                = "AllowAzureServices"
  resource_group_name = azurerm_resource_group.pet_app.name
  server_name         = azurerm_postgresql_server.pet_app_db[0].name
  start_ip_address    = "0.0.0.0"
  end_ip_address      = "0.0.0.0"
}
