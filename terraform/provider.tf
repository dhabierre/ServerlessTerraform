terraform {
  required_version = "= 0.12.7"
  # ========================================================================
  # For local execution testing 
  #  1. comment backend section
  #  2. use following commands
  # ========================================================================
  # az login
  # terraform init
  # terraform validate -var-file="variables.local.tfvars"
  # terraform plan -var-file="variables.local.tfvars" -out="out.plan"
  # terraform apply "out.plan"
  # ========================================================================
  backend "azurerm" {
    storage_account_name = "__application__sharedtfsa"
    container_name       = "terraform"
    key                  = "terraform-__environment__.tfstate"
    access_key           = "__tf_storage_account_key__"
  }
}

provider "azurerm" {
  version = "= 1.33.1"
}
