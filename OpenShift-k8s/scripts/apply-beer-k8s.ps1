# Apply dev overlay (hello + beer). Requires a cluster context and beer-app:latest available to nodes (build + load/push first).
$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
$env:Path = [System.Environment]::GetEnvironmentVariable("Path", "Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path", "User")

kubectl apply -k (Join-Path $root "k8s\overlays\dev")
Write-Host "Beer service: kubectl -n app-dev port-forward svc/beer 8080:8080  (then open http://127.0.0.1:8080)" -ForegroundColor Green
