# Serve beer-app static files locally (no Docker/K8s). Default port 18080 — 8080 is often busy.
param(
    [int] $Port = 18080
)
$ErrorActionPreference = "Stop"
$static = Join-Path (Split-Path $PSScriptRoot -Parent) "beer-app\static"
if (-not (Test-Path $static)) { Write-Error "Missing: $static" }

Write-Host "Beer app: http://127.0.0.1:$Port/  (Ctrl+C to stop)" -ForegroundColor Green
Set-Location $static
python -m http.server $Port
