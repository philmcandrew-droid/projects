# Build beer-app Dockerfile and run it. Inside the container nginx listens on 8080; host maps to 18080 by default.
param(
    [int] $HostPort = 18080
)
$ErrorActionPreference = "Stop"
$root = Split-Path $PSScriptRoot -Parent
$ctx = Join-Path $root "beer-app"
$image = "beer-app:local"

function Test-PodmanReady {
    try {
        podman info 2>$null | Out-Null
        return $LASTEXITCODE -eq 0
    } catch { return $false }
}

$env:Path = [System.Environment]::GetEnvironmentVariable("Path", "Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path", "User")

$engine = $null
if (Get-Command docker -ErrorAction SilentlyContinue) {
    try { docker info 2>$null | Out-Null; if ($LASTEXITCODE -eq 0) { $engine = "docker" } } catch {}
}
if (-not $engine -and (Get-Command podman -ErrorAction SilentlyContinue)) {
    if (Test-PodmanReady) { $engine = "podman" }
}

if (-not $engine) {
    Write-Host @"
No working container engine found.

  - Docker Desktop: https://docs.docker.com/desktop/ then re-run this script.
  - Podman on Windows: install WSL (PowerShell as Admin): wsl --install
    Reboot if prompted, then: podman machine init ; podman machine start

Build manually:
  docker build -t $image `"$ctx`"
  docker run --rm -p ${HostPort}:8080 $image
"@ -ForegroundColor Yellow
    exit 1
}

Write-Host "Using: $engine" -ForegroundColor Cyan
& $engine build -t $image $ctx
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "Starting http://127.0.0.1:$HostPort/ (Ctrl+C stops the container)" -ForegroundColor Green
& $engine run --rm -p "${HostPort}:8080" $image
