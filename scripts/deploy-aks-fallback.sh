#!/bin/bash
set -e

# AKS Deployment Fallback Script
# This script provides a more robust AKS deployment with better error handling

echo "=== AKS Deployment Fallback Script ==="
echo "Resource Group: $1"
echo "Location: $2"
echo "VM Size: $3" 
echo "ACR Name: $4"

RG_NAME="$1"
LOCATION="$2"
VM_SIZE="$3"
ACR_NAME="$4"
AKS_NAME="aks-cluster-poc"

# Function to check resource provider registration
check_resource_providers() {
    echo "=== Checking Resource Providers ==="
    
    PROVIDERS=("Microsoft.ContainerService" "Microsoft.Compute" "Microsoft.Network")
    
    for PROVIDER in "${PROVIDERS[@]}"; do
        STATUS=$(az provider show --namespace "$PROVIDER" --query "registrationState" -o tsv 2>/dev/null || echo "NotFound")
        echo "Provider $PROVIDER: $STATUS"
        
        if [[ "$STATUS" != "Registered" ]]; then
            echo "Registering provider $PROVIDER..."
            az provider register --namespace "$PROVIDER"
        fi
    done
}

# Function to validate VM size availability
validate_vm_size() {
    echo "=== Validating VM Size Availability ==="
    
    AVAILABLE=$(az vm list-sizes --location "$LOCATION" --query "[?name=='$VM_SIZE'].name" -o tsv)
    
    if [[ -z "$AVAILABLE" ]]; then
        echo "❌ VM size $VM_SIZE is not available in location $LOCATION"
        echo "Available sizes:"
        az vm list-sizes --location "$LOCATION" --query "[].name" -o table | head -20
        return 1
    else
        echo "✅ VM size $VM_SIZE is available in $LOCATION"
    fi
}

# Function to get appropriate Kubernetes version
get_kubernetes_version() {
    echo "=== Getting Kubernetes Version ==="
    
    # Get latest patch version of 1.29
    K8S_VERSION=$(az aks get-versions --location "$LOCATION" --query "orchestrators[?orchestratorVersion starts_with('1.29')].orchestratorVersion | [-1]" -o tsv 2>/dev/null || echo "")
    
    if [[ -z "$K8S_VERSION" ]]; then
        # Fallback to latest stable version
        K8S_VERSION=$(az aks get-versions --location "$LOCATION" --query "orchestrators[-2].orchestratorVersion" -o tsv)
        echo "⚠️ Using fallback version: $K8S_VERSION"
    else
        echo "✅ Using Kubernetes version: $K8S_VERSION"
    fi
    
    echo "$K8S_VERSION"
}

# Function to create AKS using CLI directly (bypass Bicep)
create_aks_direct() {
    local k8s_version="$1"
    
    echo "=== Creating AKS Cluster Directly ==="
    
    az aks create \
        --resource-group "$RG_NAME" \
        --name "$AKS_NAME" \
        --location "$LOCATION" \
        --kubernetes-version "$k8s_version" \
        --node-count 2 \
        --node-vm-size "$VM_SIZE" \
        --enable-managed-identity \
        --generate-ssh-keys \
        --attach-acr "$ACR_NAME" \
        --network-plugin kubenet \
        --service-cidr 10.0.0.0/16 \
        --dns-service-ip 10.0.0.10 \
        --load-balancer-sku standard \
        --no-wait
    
    echo "✅ AKS creation initiated (running in background)"
    
    # Check status
    echo "Checking deployment status..."
    for i in {1..30}; do
        STATUS=$(az aks show --resource-group "$RG_NAME" --name "$AKS_NAME" --query "provisioningState" -o tsv 2>/dev/null || echo "Creating")
        echo "Status check $i/30: $STATUS"
        
        if [[ "$STATUS" == "Succeeded" ]]; then
            echo "✅ AKS cluster created successfully!"
            return 0
        elif [[ "$STATUS" == "Failed" ]]; then
            echo "❌ AKS cluster creation failed!"
            return 1
        fi
        
        sleep 30
    done
    
    echo "⚠️ AKS creation timeout - check Azure portal for status"
    return 1
}

# Main execution
main() {
    check_resource_providers
    validate_vm_size
    
    K8S_VERSION=$(get_kubernetes_version)
    
    # Check if AKS already exists
    if az aks show --resource-group "$RG_NAME" --name "$AKS_NAME" >/dev/null 2>&1; then
        echo "✅ AKS cluster already exists"
        exit 0
    fi
    
    # Try Bicep first, then fallback to direct CLI
    echo "=== Attempting Bicep Deployment ==="
    if az deployment group create \
        --resource-group "$RG_NAME" \
        --template-file bicep/aks.bicep \
        --parameters location="$LOCATION" vmSize="$VM_SIZE" acrName="$ACR_NAME" kubernetesVersion="$K8S_VERSION" \
        --no-prompt; then
        echo "✅ Bicep deployment successful!"
    else
        echo "⚠️ Bicep deployment failed, trying direct CLI approach..."
        create_aks_direct "$K8S_VERSION"
    fi
}

# Validate arguments
if [[ $# -ne 4 ]]; then
    echo "Usage: $0 <resource-group> <location> <vm-size> <acr-name>"
    exit 1
fi

main "$@"
