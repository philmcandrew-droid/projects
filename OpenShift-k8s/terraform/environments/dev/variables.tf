variable "environment" {
  type        = string
  description = "Deployment environment name (e.g. dev, staging, prod)."
  default     = "dev"
}

variable "project_name" {
  type        = string
  description = "Short project identifier for tagging and naming."
  default     = "openshift-k8s"
}
