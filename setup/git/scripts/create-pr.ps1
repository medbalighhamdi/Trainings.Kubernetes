param (
    [string]$TargetBranch = "develop"
)

# -----------------------------
# GET REPO URL
# -----------------------------
$RepoUrl = (git config --get remote.origin.url | Out-String).Trim()

if (-not $RepoUrl) {
    Write-Host "ERROR: Not a git repository." -ForegroundColor Red
    exit 1
}

# Convert SSH to HTTPS if needed
if ($RepoUrl -match "^git@github.com:") {
    $RepoUrl = $RepoUrl -replace "^git@github.com:", "https://github.com/"
}

$RepoUrl = $RepoUrl -replace "\.git$", ""

# -----------------------------
# GET SOURCE BRANCH (FIXED)
# -----------------------------
$SourceBranch = (git rev-parse --abbrev-ref HEAD | Out-String).Trim()

if (-not $SourceBranch) {
    Write-Host "ERROR: Could not determine source branch." -ForegroundColor Red
    exit 1
}

# -----------------------------
# SELECT TEMPLATE
# -----------------------------
$Template = "feature.md"

if ($SourceBranch -like "fix/*" -or $SourceBranch -like "bugfix/*") {
    $Template = "fix.md"
}
elseif ($SourceBranch -like "hotfix/*") {
    $Template = "hotfix.md"
}
elseif ($SourceBranch -like "feature/*") {
    $Template = "feature.md"
}

# -----------------------------
# BUILD URL (FIXED)
# -----------------------------
$PrUrl = "$RepoUrl/compare/$TargetBranch...$SourceBranch`?template=$Template"

Write-Host ""
Write-Host "Source Branch : $SourceBranch"
Write-Host "Target Branch : $TargetBranch"
Write-Host "Template      : $Template"
Write-Host ""
Write-Host "Opening PR URL:"
Write-Host $PrUrl
Write-Host ""

# -----------------------------
# OPEN BROWSER
# -----------------------------
Start-Process $PrUrl
