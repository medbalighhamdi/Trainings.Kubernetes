<#
.SYNOPSIS
Uninstall Git PR helper

.DESCRIPTION
Removes the 'pr' git alias (local and optional global)
Deletes create-pr.ps1 script from the repo
#>

param (
    [switch]$Global
)

# -----------------------------
# Determine repo root
# -----------------------------
$RepoRoot = (git rev-parse --show-toplevel | Out-String).Trim()
if (-not $RepoRoot) {
    Write-Host "ERROR: Not inside a git repository." -ForegroundColor Red
    exit 1
}

# -----------------------------
# Remove Git alias
# -----------------------------
if ($Global) {
    git config --global --unset alias.pr
    Write-Host "Git alias 'pr' removed globally."
} else {
    git config --local --unset alias.pr
    Write-Host "Git alias 'pr' removed locally."
}

Write-Host "✅ Uninstall complete."
