module "platform" {
  source = "../../modules/platform"

  environment = var.environment
  project     = var.project_name
}
