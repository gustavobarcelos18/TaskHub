[CmdletBinding()]
param([Parameter(Mandatory)][string]$BackupPath, [string]$DatabasePath = (Join-Path $PSScriptRoot '..\backend\MinhaPrimeiraAPI\Database\tarefas.db'), [switch]$ApiStopped)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
if (-not $ApiStopped) { throw 'Pare a API e execute novamente com -ApiStopped antes de restaurar um backup.' }
& dotnet run --project (Join-Path $PSScriptRoot 'DatabaseMaintenance') -- restore $BackupPath $DatabasePath
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
