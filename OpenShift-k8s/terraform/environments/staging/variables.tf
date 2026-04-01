variable "environment" {
  type        = string
  description = "Deployment environment name."
  default     = "staging"
}

variable "project_name" {
  type        = string
  description = "Short project identifier for tagging and naming."
  default     = "openshift-k8s"
}
