$log = Join-Path $PSScriptRoot "crc-setup-log.txt"
$env:Path = [System.Environment]::GetEnvironmentVariable("Path", "Machine") + ";" + [System.Environment]::GetEnvironmentVariable("Path", "User")
"=== $(Get-Date) ===" | Out-File $log -Encoding utf8
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
"Admin: $isAdmin" | Out-File $log -Append -Encoding utf8
"--- whoami /groups (Hyper-V related) ---" | Out-File $log -Append -Encoding utf8
cmd /c "whoami /groups" | Select-String -Pattern "Hyper-V|S-1-5-32" | Out-File $log -Append -Encoding utf8
"--- crc setup ---" | Out-File $log -Append -Encoding utf8
& crc setup 2>&1 | Out-File $log -Append -Encoding utf8
"exit: $LASTEXITCODE" | Out-File $log -Append -Encoding utf8
