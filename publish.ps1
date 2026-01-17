param(
    [string]$Runtime = 'win-x64',
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

$outDir = Join-Path $root 'artifacts'
if (Test-Path $outDir) {
    Remove-Item -Recurse -Force $outDir
}
New-Item -ItemType Directory -Path $outDir | Out-Null

$project = Join-Path $root 'BlastWaveCSharp.csproj'

dotnet publish $project -c $Configuration -r $Runtime --self-contained false -o $outDir
