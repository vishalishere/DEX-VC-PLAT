#!/bin/bash
# © 2024 DecVCPlat. All rights reserved.
# DecVCPlat Azure Deployment Script

set -e

# Configuration
DECVCPLAT_ENVIRONMENT=${1:-prod}
DECVCPLAT_LOCATION=${2:-eastus}
DECVCPLAT_RESOURCE_GROUP="decvcplat-${DECVCPLAT_ENVIRONMENT}-rg"
DECVCPLAT_SUBSCRIPTION_ID=${AZURE_SUBSCRIPTION_ID}

echo "🚀 Starting DecVCPlat Azure deployment..."
echo "Environment: ${DECVCPLAT_ENVIRONMENT}"
echo "Location: ${DECVCPLAT_LOCATION}"
echo "Resource Group: ${DECVCPLAT_RESOURCE_GROUP}"

# Check Azure CLI login
if ! az account show > /dev/null 2>&1; then
    echo "❌ Please login to Azure CLI first: az login"
    exit 1
fi

# Set subscription
if [ -n "${DECVCPLAT_SUBSCRIPTION_ID}" ]; then
    echo "📋 Setting subscription: ${DECVCPLAT_SUBSCRIPTION_ID}"
    az account set --subscription "${DECVCPLAT_SUBSCRIPTION_ID}"
fi

# Create resource group
echo "📦 Creating resource group: ${DECVCPLAT_RESOURCE_GROUP}"
az group create \
    --name "${DECVCPLAT_RESOURCE_GROUP}" \
    --location "${DECVCPLAT_LOCATION}" \
    --tags "Application=DecVCPlat" "Environment=${DECVCPLAT_ENVIRONMENT}"

# Deploy Bicep template
echo "🏗️ Deploying DecVCPlat infrastructure..."
DECVCPLAT_DEPLOYMENT_NAME="decvcplat-deployment-$(date +%Y%m%d-%H%M%S)"

az deployment group create \
    --resource-group "${DECVCPLAT_RESOURCE_GROUP}" \
    --name "${DECVCPLAT_DEPLOYMENT_NAME}" \
    --template-file main.bicep \
    --parameters environmentName="${DECVCPLAT_ENVIRONMENT}" location="${DECVCPLAT_LOCATION}"

# Get deployment outputs
echo "📊 Retrieving deployment outputs..."
DECVCPLAT_AKS_NAME=$(az deployment group show \
    --resource-group "${DECVCPLAT_RESOURCE_GROUP}" \
    --name "${DECVCPLAT_DEPLOYMENT_NAME}" \
    --query properties.outputs.decvcplatAksClusterName.value -o tsv)

DECVCPLAT_ACR_NAME=$(az deployment group show \
    --resource-group "${DECVCPLAT_RESOURCE_GROUP}" \
    --name "${DECVCPLAT_DEPLOYMENT_NAME}" \
    --query properties.outputs.decvcplatContainerRegistryName.value -o tsv)

echo "✅ DecVCPlat infrastructure deployed successfully!"
echo "🎯 AKS Cluster: ${DECVCPLAT_AKS_NAME}"
echo "🏗️ Container Registry: ${DECVCPLAT_ACR_NAME}"

# Get AKS credentials
echo "🔐 Configuring kubectl access..."
az aks get-credentials \
    --resource-group "${DECVCPLAT_RESOURCE_GROUP}" \
    --name "${DECVCPLAT_AKS_NAME}" \
    --overwrite-existing

# Configure ACR authentication
echo "🔑 Configuring ACR authentication..."
az aks update \
    --resource-group "${DECVCPLAT_RESOURCE_GROUP}" \
    --name "${DECVCPLAT_AKS_NAME}" \
    --attach-acr "${DECVCPLAT_ACR_NAME}"

echo "🎉 DecVCPlat Azure deployment completed successfully!"
echo "📋 Next steps:"
echo "  1. Build and push Docker images to ACR"
echo "  2. Deploy Kubernetes manifests"
echo "  3. Configure DNS and SSL certificates"
