# Start OpenShift Local (CRC) and apply this repo. Run crc as a normal user (not elevated).
# Full OpenShift preset: crc start -p C:\path\to\pull-secret.txt
# MicroShift: crc config set preset microshift

$ErrorActionPreference = "Stop"
$env:Path = [System.Environment]::GetEnvironmentVariable("Path", "Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path", "User")

$repoRoot = Split-Path $PSScriptRoot -Parent
if (-not (Test-Path "$repoRoot\k8s\overlays\dev")) {
    Write-Error "Could not find k8s/overlays/dev under $repoRoot"
}

Write-Host "=== crc setup ===" -ForegroundColor Cyan
crc setup
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "=== crc start ===" -ForegroundColor Cyan
crc start
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "=== oc / kubectl env ===" -ForegroundColor Cyan
crc oc-env | Invoke-Expression

$useRoute = $env:OPENSHIFT_APPLY_ROUTE -eq "1"
$overlay = if ($useRoute) { "dev-openshift" } else { "dev" }
Write-Host "=== kubectl apply -k k8s/overlays/$overlay ===" -ForegroundColor Cyan
kubectl apply -k "$repoRoot\k8s\overlays\$overlay"

Write-Host "Done. OPENSHIFT_APPLY_ROUTE=1 uses dev-openshift (Route). crc status | crc console" -ForegroundColor Green
