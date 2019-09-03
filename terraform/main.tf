data "azurerm_client_config" "current" {}

locals {
  domain                = "${var.application}-${var.environment}"
  servicebus_queue_name = "${var.application}-servicebus-queue-client-data"

  shared_tags = {
    application  = var.application
    deployment   = "terraform"
  }

  app_tags = merge(local.shared_tags, { "environment" = var.environment })
}

# ======================================================================================
# Resource Groups
# ======================================================================================

resource "azurerm_resource_group" "resource_group" {
  location = var.location
  name     = local.domain
  tags     = local.app_tags
}

# ======================================================================================
# Storage
# ======================================================================================

resource "azurerm_storage_account" "storage_account" {
  account_replication_type = "LRS"
  account_tier             = "Standard"
  account_kind             = "StorageV2"
  location                 = azurerm_resource_group.resource_group.location
  name                     = "${replace(local.domain, "-", "")}sa"
  resource_group_name      = azurerm_resource_group.resource_group.name
  tags                     = local.app_tags
}

resource "azurerm_storage_container" "storage_container_jsons" {
  container_access_type = "private"
  name                  = "jsons"
  resource_group_name   = "${azurerm_resource_group.resource_group.name}"
  storage_account_name  = "${azurerm_storage_account.storage_account.name}"
}

# ======================================================================================
# Functions App Service Plan
# ======================================================================================

resource "azurerm_app_service_plan" "app_service_plan_functions" {
  kind                = "FunctionApp"
  location            = azurerm_resource_group.resource_group.location
  name                = "${local.domain}-app-service-plan-functions"
  resource_group_name = azurerm_resource_group.resource_group.name
  sku {
    size = "Y1"
    tier = "Dynamic"
  }
  tags = local.app_tags
}

# ======================================================================================
# FunctionApp
# ======================================================================================

resource "azurerm_function_app" "function_app" {
  name  = "${local.domain}-function-app"

  app_settings = {
    # Runtime configuration
    FUNCTIONS_WORKER_RUNTIME        = "dotnet"
    APPINSIGHTS_INSTRUMENTATIONKEY  = azurerm_application_insights.application_insights.instrumentation_key
    # Azure Functions configuration
    KeyVaultName                    = azurerm_key_vault.key_vault.name
    StorageConnectionString         = azurerm_storage_account.storage_account.primary_connection_string
    StorageBlobContainerName        = "jsons"
    ServiceBusConnectionString      = azurerm_servicebus_namespace.servicebus_namespace.default_primary_connection_string
    ServiceBusQueueName             = local.servicebus_queue_name
  }

  app_service_plan_id = azurerm_app_service_plan.app_service_plan_functions.id

  identity {
    type = "SystemAssigned" # to access to the KeyVault
  }

  location                  = azurerm_resource_group.resource_group.location
  resource_group_name       = azurerm_resource_group.resource_group.name
  storage_connection_string = azurerm_storage_account.storage_account.primary_connection_string
  tags                      = local.app_tags
  # dotnet core version (app_settings.FUNCTIONS_EXTENSION_VERSION never set/updated)
  version                   = "~2"
}

# ======================================================================================
# Application Insights
# ======================================================================================

resource "azurerm_application_insights" "application_insights" {
  name                = "${local.domain}-app-insights"
  location            = azurerm_resource_group.resource_group.location
  resource_group_name = azurerm_resource_group.resource_group.name
  application_type    = "Web"
  tags                = local.app_tags
}

# ======================================================================================
# Service Bus + Queue
# ======================================================================================

resource "azurerm_servicebus_namespace" "servicebus_namespace" {
  name                = "${local.domain}-servicebus"
  resource_group_name = azurerm_resource_group.resource_group.name
  location            = azurerm_resource_group.resource_group.location
  sku                 = "Standard"
  tags                = local.app_tags
}

resource "azurerm_servicebus_queue" "servicebus_queue" {
  name                = local.servicebus_queue_name
  resource_group_name = azurerm_resource_group.resource_group.name
  namespace_name      = azurerm_servicebus_namespace.servicebus_namespace.name
  enable_partitioning = true
}

# ======================================================================================
# KeyVault
# ======================================================================================

resource "azurerm_key_vault" "key_vault" {
  name                        = "${local.domain}-keyvault"
  location                    = azurerm_resource_group.resource_group.location
  resource_group_name         = azurerm_resource_group.resource_group.name
  tenant_id                   = data.azurerm_client_config.current.tenant_id
  enabled_for_disk_encryption = true
  sku_name                    = "standard"

  # Important: this access_policy MUST BE DEFINED INSIDE the key_vault resource otherwise
  # the process will fail the first time the secrets are added (adding secrets before access_policy created)
  access_policy {
    tenant_id = data.azurerm_client_config.current.tenant_id
    object_id = data.azurerm_client_config.current.service_principal_object_id

    key_permissions = [
      "get",
      "list",
      "create",
      "delete",
    ]

    secret_permissions = [
      "get",
      "list",
      "set",
      "delete",
    ]
  }

  lifecycle {
    ignore_changes = [access_policy]
  }

  tags = local.app_tags
}

# Important: this access_policy MUST BE DEFINED OUTSIDE the key_vault resource otherwise
# the process will fail while reading "azurerm_app_service.app_service_backend.identity.0.principal_id"
resource "azurerm_key_vault_access_policy" "key_vault_access_policy_function_app" {
  key_vault_id = azurerm_key_vault.key_vault.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = azurerm_function_app.function_app.identity[0].principal_id

  key_permissions = [
    "get",
    "list",
  ]

  secret_permissions = [
    "get",
    "list",
  ]
}

# ======================================================================================
# KeyVault (Secrets)
# ======================================================================================

resource "azurerm_key_vault_secret" "key_vault_secret_basic_authentication_function_f1" {
  name         = "Basic--Authentication--Function--F1"
  value        = "${base64encode(format("%s:%s", var.function_f1_username, var.function_f1_password))}"
  key_vault_id = azurerm_key_vault.key_vault.id
}
