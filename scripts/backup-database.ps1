[CmdletBinding()]
param([string]$DatabasePath = (Join-Path $PSScriptRoot '..\backend\MinhaPrimeiraAPI\Database\tarefas.db'), [string]$BackupDirectory = (Join-Path $PSScriptRoot '..\backups'), [switch]$ApiStopped)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if (-not $ApiStopped) { throw 'Pare a API e execute novamente com -ApiStopped antes de criar um backup.' }
& dotnet run --project (Join-Path $PSScriptRoot 'DatabaseMaintenance') -- backup $DatabasePath $BackupDirectory
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
