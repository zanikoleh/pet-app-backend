# PetApp Backend - Terraform Infrastructure as Code

This directory contains Terraform configurations to deploy the PetApp backend infrastructure on Azure.

## ✨ Features

- **Azure Container Registry (ACR)**: Store and manage Docker images
- **Resource Group**: Organized Azure resources
- **PostgreSQL Database** (optional): For microservices data storage
- **Azure Container Instances** (optional): Lightweight deployment option
- **Remote State Storage** (optional): For team collaboration
- **Full Version Control**: Track all infrastructure changes

## 📋 Prerequisites

1. **Install Terraform**
   ```bash
   brew install terraform  # macOS
   # or visit https://www.terraform.io/downloads.html
   ```

2. **Install Azure CLI**
   ```bash
   brew install azure-cli  # macOS
   ```

3. **Azure Account** with appropriate permissions to create resources

4. **Authenticate with Azure**
   ```bash
   az login
   az account show  # Verify subscription
   ```

## 🚀 Quick Start

### 1. Clone/Copy Configuration
The Terraform files are located in `infrastructure/terraform/`:
```bash
cd infrastructure/terraform
```

### 2. Initialize Variables
```bash
# Copy the example file
cp terraform.tfvars.example terraform.tfvars

# Edit values (DON'T commit terraform.tfvars!)
nano terraform.tfvars
```

Update these values:
- `environment`: dev, staging, or prod
- `acr_name`: Your ACR name (must be unique in Azure)
- `location`: Azure region
- Database credentials (if `create_database = true`)

### 3. Initialize Terraform
```bash
terraform init
```

This downloads required providers and initializes the working directory.

### 4. Plan Deployment
```bash
terraform plan -out=tfplan
```

Review the resources that will be created.

### 5. Apply Configuration
```bash
terraform apply tfplan
```

Terraform will create resources on Azure. This may take 2-5 minutes.

### 6. View Outputs
```bash
terraform output
```

Example output:
```
acr_login_server = "oznkpetpp-e0g4bpfgckaxhdh4.azurecr.io"
acr_admin_username = "oznkpetpp"
resource_group_name = "petapp_rsrc_grp"
```

## 📁 File Structure

```
infrastructure/terraform/
├── main.tf                    # Primary resource definitions
├── variables.tf               # Variable declarations and validation
├── outputs.tf                 # Output values
├── terraform.tfvars.example   # Example variable values (copy & customize)
├── terraform.tfvars          # Your actual variables (DON'T commit!)
├── .gitignore                # Ignore sensitive files
└── README.md                  # This file
```

## 🔑 Key Files Explained

### `main.tf`
Defines all Azure resources:
- `azurerm_resource_group`: Container for all resources
- `azurerm_container_registry`: ACR for Docker images
- `azurerm_storage_account`: Optional state storage
- `azurerm_container_group`: Optional single-service deployment
- `azurerm_postgresql_server`: Optional database

### `variables.tf`
Declares all input variables with:
- Default values
- Type constraints
- Validation rules
- Descriptions

### `outputs.tf`
Exports useful values after deployment:
- ACR login server URL
- Admin credentials
- Database connection details
- Helpful commands

## 📋 Common Tasks

### Deploy ACR Only (Minimal)
```bash
terraform plan -out=tfplan
terraform apply tfplan
```

### Deploy ACR + Database
```bash
# Edit terraform.tfvars
sed -i 's/create_database = false/create_database = true/' terraform.tfvars

terraform plan -out=tfplan
terraform apply tfplan
```

### Deploy ACR + Demo Container Instance
```bash
# Edit terraform.tfvars
sed -i 's/create_aci_demo = false/create_aci_demo = true/' terraform.tfvars

terraform plan -out=tfplan
terraform apply tfplan
```

### Push Images to ACR
```bash
# Get login server from terraform output
ACR_SERVER=$(terraform output -raw acr_login_server)

# Login to ACR
az acr login --name $(terraform output -raw resource_group_name | cut -d'_' -f1)

# Or manually with credentials
ACR_USER=$(terraform output -raw acr_admin_username)
ACR_PASS=$(terraform output -raw acr_admin_password)
echo $ACR_PASS | docker login -u $ACR_USER --password-stdin $ACR_SERVER

# Tag and push images
docker tag pet-service:latest $ACR_SERVER/pet-service:0.1.0
docker push $ACR_SERVER/pet-service:0.1.0
```

### List Repositories
```bash
az acr repository list --name $(terraform output -raw resource_group_name | cut -d'_' -f1)
```

### Delete Infrastructure (Destroy)
```bash
terraform destroy
```
⚠️ This will delete all resources! Backups are your responsibility.

## 🔐 Security Best Practices

### 1. Never Commit Sensitive Files
```bash
# .gitignore should include:
terraform.tfvars
*.tfstate
*.tfstate.*
.terraform/
```

### 2. Use Azure Key Vault for Secrets (Production)
For production environments, integrate Azure Key Vault:
```hcl
data "azurerm_key_vault_secret" "db_password" {
  name         = "db-password"
  key_vault_id = data.azurerm_key_vault.vault.id
}
```

### 3. Enable Remote State with Encryption (Production)
Uncomment the backend block in `main.tf`:
```hcl
backend "azurerm" {
  resource_group_name  = "petapp_rsrc_grp"
  storage_account_name = "petstatestore"
  container_name       = "tfstate"
  key                  = "prod.terraform.tfstate"
}
```

Then migrate state:
```bash
terraform init -migrate-state
```

### 4. Use Role-Based Access Control (RBAC)
Limit Azure CLI user permissions to only necessary resources.

## 🐛 Troubleshooting

### Error: "Error: Insufficient privileges to complete the operation"
```bash
# Ensure you're authenticated with correct subscription
az account show
az account set --subscription "<subscription-id>"
```

### Error: "Invalid resource group name"
- ACR names must be 5-50 characters, lowercase alphanumeric only
- Resource group names cannot contain certain special characters
- Check `variables.tf` for validation rules

### Error: "Storage account name already exists"
- Storage account names must be globally unique in Azure
- Change `storage_account_name` in `terraform.tfvars`

### State Lock
If Terraform seems stuck:
```bash
terraform force-unlock <LOCK_ID>
```

## 🔄 Updating Infrastructure

### Change Environment
```bash
# Edit terraform.tfvars
terraform.tfvars: environment = "prod"

# Plan and apply
terraform plan -out=tfplan
terraform apply tfplan
```

### Scale Database
```bash
# Edit terraform.tfvars
db_sku_name = "GP_Gen5_4"

terraform plan -out=tfplan
terraform apply tfplan
```

## 📊 Monitoring Costs

```bash
# Estimate costs before applying
terraform plan -json | grep -i "cost" 

# Or use Azure CLI
az account billing update --enable-cli-tracking
az costmanagement query create --type "Usage"
```

## 📚 Resources

- [Terraform Azure Provider Docs](https://registry.terraform.io/providers/hashicorp/azurerm/latest/docs)
- [Azure Container Registry Docs](https://docs.microsoft.com/en-us/azure/container-registry/)
- [Terraform Best Practices](https://www.terraform.io/docs/cloud/guides/recommended-practices.html)
- [Azure Naming Conventions](https://docs.microsoft.com/en-us/azure/cloud-adoption-framework/ready/azure-best-practices/naming-and-tagging)

## 🤝 Contributing

When modifying Terraform files:
1. Run `terraform fmt` to format code
2. Run `terraform validate` to check syntax
3. Document changes in git commits
4. Never directly modify Azure resources; use Terraform

```bash
# Format Terraform files
terraform fmt -recursive

# Validate syntax
terraform validate

# Check for best practices
terraform plan -destroy  # preview cleanup
```

## 📝 State Management

### Local State (Development)
Default behavior - state stored in `.terraform/` directory.

### Remote State (Production)
```bash
# After enabling backend in main.tf
terraform init -migrate-state

# View remote state
terraform state pull | jq '.'

# List resources
terraform state list
```

## ✅ Deployment Checklist

- [ ] Install Terraform and Azure CLI
- [ ] Authentication with Azure (`az login`)
- [ ] Copy `terraform.tfvars.example` to `terraform.tfvars`
- [ ] Update `terraform.tfvars` with your values
- [ ] Run `terraform validate`
- [ ] Run `terraform plan`
- [ ] Review the plan output
- [ ] Run `terraform apply`
- [ ] Verify outputs with `terraform output`
- [ ] Test ACR connectivity
- [ ] Push test image to ACR
- [ ] Add to `.gitignore` before committing

---

**Last Updated**: March 23, 2026

For issues or questions, refer to [Azure Terraform Provider Documentation](https://registry.terraform.io/providers/hashicorp/azurerm/latest)
