[CmdletBinding()]
param(
    [string]$Registry = $env:REGISTRY,

    [string]$RepositoryPrefix = $(if ($env:REPOSITORY_PREFIX) { $env:REPOSITORY_PREFIX } else { "chema-vnext" }),

    [string]$Tag = $(if ($env:TAG) { $env:TAG } else { "latest" }),

    [string]$Platform = $env:PLATFORM,

    [switch]$Push,

    [switch]$DryRun
)

$ErrorActionPreference = "Stop"

$ScriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$RepoRoot = Resolve-Path (Join-Path $ScriptDirectory "..")

if ($null -eq $Registry) {
    $Registry = ""
}
if ($null -eq $RepositoryPrefix) {
    $RepositoryPrefix = "chema-vnext"
}

$Registry = $Registry.TrimEnd("/")
$RepositoryPrefix = $RepositoryPrefix.Trim("/")
if ([string]::IsNullOrWhiteSpace($RepositoryPrefix)) {
    throw "RepositoryPrefix cannot be empty."
}
if ([string]::IsNullOrWhiteSpace($Tag)) {
    throw "Tag cannot be empty."
}

$Images = @(
    @{ Name = "httpapi-host"; Dockerfile = "src/CheMa.VNext.HttpApi.Host/Dockerfile" },
    @{ Name = "gateway"; Dockerfile = "src/CheMa.VNext.Gateway/Dockerfile" },
    @{ Name = "blazor"; Dockerfile = "src/CheMa.VNext.Blazor/Dockerfile" },
    @{ Name = "background-worker"; Dockerfile = "src/CheMa.VNext.BackgroundWorker/Dockerfile" },
    @{ Name = "dbmigrator"; Dockerfile = "src/CheMa.VNext.DbMigrator/Dockerfile" }
)

function Get-ImageName {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Name
    )

    $image = "$RepositoryPrefix/$($Name):$Tag"
    if (-not [string]::IsNullOrWhiteSpace($Registry)) {
        $image = "$Registry/$image"
    }

    return $image
}

function Invoke-CommandLine {
    param(
        [Parameter(Mandatory = $true)]
        [string]$FilePath,

        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    Write-Host "+ $FilePath $($Arguments -join ' ')"
    if (-not $DryRun) {
        & $FilePath @Arguments
        if ($LASTEXITCODE -ne 0) {
            throw "Command failed with exit code ${LASTEXITCODE}: $FilePath $($Arguments -join ' ')"
        }
    }
}

foreach ($item in $Images) {
    $imageName = Get-ImageName -Name $item.Name
    $dockerfilePath = Join-Path $RepoRoot $item.Dockerfile

    $arguments = @("build", "-f", $dockerfilePath, "-t", $imageName)
    if (-not [string]::IsNullOrWhiteSpace($Platform)) {
        $arguments += @("--platform", $Platform)
    }
    $arguments += $RepoRoot.Path

    Invoke-CommandLine -FilePath "docker" -Arguments $arguments

    if ($Push) {
        Invoke-CommandLine -FilePath "docker" -Arguments @("push", $imageName)
    }
}
