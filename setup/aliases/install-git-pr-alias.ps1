# -----------------------------
# Script: setup-pr-alias.ps1
# Purpose: Create a portable git alias 'pr' that runs create-pr.ps1
# -----------------------------

# -----------------------------
# Determine repo root
# -----------------------------
$RepoRoot = (git rev-parse --show-toplevel | Out-String).Trim()
if (-not $RepoRoot) {
    Write-Host "ERROR: Not inside a git repository." -ForegroundColor Red
    exit 1
}

# -----------------------------
# Define path to create-pr.ps1 (relative to repo root)
# -----------------------------
$ScriptRelativePath = "setup/aliases/scripts/create-pr.ps1"
$ScriptFullPath = Join-Path $RepoRoot $ScriptRelativePath

if (-not (Test-Path $ScriptFullPath)) {
    Write-Host "ERROR: create-pr.ps1 not found at $ScriptFullPath" -ForegroundColor Red
    exit 1
}

# -----------------------------
# Set git alias 'pr'
# -----------------------------
# Use relative path to repo root
$AliasCommand = "!powershell -ExecutionPolicy Bypass -File './$ScriptRelativePath'"

git config --local alias.pr $AliasCommand

Write-Host "✅ Git alias 'pr' successfully created!"
Write-Host "Usage examples:"
Write-Host "  git pr           # Opens PR with default target branch"
Write-Host "  git pr develop   # Opens PR targeting 'develop'"
