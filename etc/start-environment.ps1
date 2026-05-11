[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$ScriptDirectory = Split-Path -Parent $MyInvocation.MyCommand.Path
$StartScript = Join-Path $ScriptDirectory "start.ps1"

& $StartScript -Target environment
