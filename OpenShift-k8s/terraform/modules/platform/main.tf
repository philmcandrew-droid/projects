# Platform module: extend with cluster provisioning (ROSA, ARO, self-managed),
# networking, identity, and shared services. Keep modules small and composable.

terraform {
  required_providers {
    kubernetes = {
      source = "hashicorp/kubernetes"
    }
  }
}

# Example: wire the Kubernetes provider after cluster exists (kubeconfig or in-cluster).
# provider "kubernetes" { config_path = var.kubeconfig_path }

locals {
  common_tags = {
    environment = var.environment
    project     = var.project
    managed_by  = "terraform"
  }
}
