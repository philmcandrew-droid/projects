# Build the beer-app container image (nginx static site for beer-can branding).
$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
$ctx = Join-Path $root "beer-app"

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Error "Docker is not on PATH. Install Docker Desktop or use: podman build -t beer-app:latest $ctx"
}

docker build -t beer-app:latest $ctx
Write-Host "Built beer-app:latest. Load into cluster e.g. kind load docker-image beer-app:latest --name kind" -ForegroundColor Green
