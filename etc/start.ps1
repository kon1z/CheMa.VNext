[CmdletBinding()]
param(
    [ValidateSet("environment", "services", "all")]
    [string]$Target = "all",

    [switch]$Build,

    [switch]$Detached,

    [switch]$Local
)

$ErrorActionPreference = "Stop"

$ScriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$ComposeDirectory = Join-Path $ScriptDirectory "docker-compose"
$EnvFile = Join-Path $ComposeDirectory ".env"
$ComposeFile = Join-Path $ComposeDirectory "docker-compose.yml"
$LocalComposeFile = Join-Path $ComposeDirectory "docker-compose.local.yml"
$InfrastructureFile = Join-Path $ComposeDirectory "docker-compose.infrastructure.yml"

function Invoke-DockerCompose {
    param(
        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    & docker compose @Arguments
}

function Start-Environment {
    $arguments = @("--env-file", $EnvFile, "-f", $InfrastructureFile, "up", "-d")
    Invoke-DockerCompose -Arguments $arguments
}

function Start-Services {
    $serviceComposeFile = $ComposeFile
    if ($Local) {
        $serviceComposeFile = $LocalComposeFile
    }

    $arguments = @("--env-file", $EnvFile, "-f", $serviceComposeFile, "up")

    if ($Build) {
        $arguments += "--build"
    }

    if ($Detached) {
        $arguments += "-d"
    }

    $arguments += @("dbmigrator", "httpapi-host", "background-worker", "gateway", "blazor")

    Invoke-DockerCompose -Arguments $arguments
}

if (-not (Test-Path $EnvFile)) {
    throw "Environment file not found: $EnvFile"
}

switch ($Target) {
    "environment" {
        Start-Environment
    }
    "services" {
        Start-Services
    }
    "all" {
        Start-Environment
        Start-Services
    }
}
