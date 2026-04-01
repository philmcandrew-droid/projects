# Quick local checks (Terraform + all Kustomize overlays). Requires terraform and kubectl on PATH.
$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
$env:Path = [System.Environment]::GetEnvironmentVariable("Path", "Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path", "User")

Push-Location $root

Write-Host "== terraform fmt (check) ==" -ForegroundColor Cyan
Push-Location terraform\environments\dev
terraform fmt -check -recursive ..\..\..
Pop-Location

foreach ($env in @("dev", "staging", "prod")) {
    Write-Host "== terraform init/validate ($env) ==" -ForegroundColor Cyan
    Push-Location "terraform\environments\$env"
    if (-not (Test-Path "versions.tf")) { Pop-Location; continue }
    terraform init -backend=false
    terraform validate
    Pop-Location
}

foreach ($overlay in @("dev", "staging", "prod", "dev-openshift")) {
    Write-Host "== kubectl kustomize k8s/overlays/$overlay ==" -ForegroundColor Cyan
    & kubectl kustomize "k8s\overlays\$overlay" | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "kubectl kustomize failed for overlay $overlay (exit $LASTEXITCODE)" }
}

Write-Host "All checks passed." -ForegroundColor Green
Pop-Location
