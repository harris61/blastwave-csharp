param(
    [string]$Runtime = 'win-x64',
    [string]$Configuration = 'Release',
    [string]$OutDir,
    [bool]$SelfContained = $true,
    [bool]$SingleFile = $true
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

$outDir = if ([string]::IsNullOrWhiteSpace($OutDir)) {
    Join-Path $root 'artifacts'
} else {
    if ([System.IO.Path]::IsPathRooted($OutDir)) { $OutDir } else { Join-Path $root $OutDir }
}
if (Test-Path $outDir) {
    Remove-Item -Recurse -Force $outDir
}
New-Item -ItemType Directory -Path $outDir | Out-Null

$project = Join-Path $root 'BlastWaveCSharp.csproj'

if ($SelfContained) {
    $selfContainedArg = 'true'
} else {
    $selfContainedArg = 'false'
}

$publishArgs = @($project, '-c', $Configuration, '-r', $Runtime, '--self-contained', $selfContainedArg)
if ($SingleFile) {
    $publishArgs += '-p:PublishSingleFile=true'
}

if ([string]::IsNullOrWhiteSpace($OutDir)) {
    dotnet publish @publishArgs
} else {
    dotnet publish @publishArgs -o $outDir
}
