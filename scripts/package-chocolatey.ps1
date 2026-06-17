[CmdletBinding()]
param(
    [string]$Version,
    [string]$Runtime = "win-x64",
    [string]$Configuration = "Release",
    [switch]$SkipTests
)

$ErrorActionPreference = "Stop"

function Invoke-Native {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath,

        [Parameter(ValueFromRemainingArguments = $true)]
        [string[]]$Arguments
    )

    & $FilePath @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "$FilePath exited with code $LASTEXITCODE."
    }
}

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$projectPath = Join-Path $repoRoot "src\CopyX.csproj"
$templatePath = Join-Path $repoRoot "chocolatey\copyx.nuspec"
$artifactRoot = Join-Path $repoRoot "artifacts\chocolatey"
$toolsDir = Join-Path $artifactRoot "tools"
$nuspecPath = Join-Path $artifactRoot "copyx.nuspec"

if ([string]::IsNullOrWhiteSpace($Version)) {
    [xml]$project = Get-Content -Path $projectPath
    $Version = $project.Project.PropertyGroup.Version
}

if ([string]::IsNullOrWhiteSpace($Version)) {
    throw "Package version was not supplied and could not be read from $projectPath."
}

$choco = Get-Command choco -ErrorAction SilentlyContinue
if ($null -eq $choco) {
    throw "Chocolatey CLI was not found. Install Chocolatey or run this script on a machine with choco.exe available."
}

if (-not $SkipTests) {
    Invoke-Native dotnet test (Join-Path $repoRoot "CopyX.sln") --configuration $Configuration
}

if (Test-Path $artifactRoot) {
    Remove-Item -LiteralPath $artifactRoot -Recurse -Force
}

New-Item -ItemType Directory -Path $toolsDir | Out-Null

Invoke-Native dotnet publish $projectPath `
    --configuration $Configuration `
    --runtime $Runtime `
    --self-contained true `
    --output $toolsDir `
    /p:PublishSingleFile=true `
    /p:PublishTrimmed=false `
    /p:DebugType=None `
    /p:DebugSymbols=false `
    /p:Version=$Version

$nuspec = Get-Content -Path $templatePath -Raw
$nuspec = $nuspec.Replace("__VERSION__", $Version)
Set-Content -Path $nuspecPath -Value $nuspec -Encoding UTF8

Invoke-Native choco pack $nuspecPath --out $artifactRoot
