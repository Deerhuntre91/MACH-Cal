<#
Publish script for MACH Cal: builds a single-file, self-contained EXE for win-x64
Usage (PowerShell):
  .\publish_winx64.ps1
Outputs:
  ./publish-output/MACH_Cal-win-x64.zip  (contains the single EXE and supporting files)
#>

param(
	[string]$Configuration = 'Release',
	[string]$Runtime = 'win-x64',
	[string]$ProjectPath = 'MACH Cal\MACH Cal.csproj'
)

Write-Host "Publishing project: $ProjectPath (Configuration=$Configuration, Runtime=$Runtime)"

# Ensure dotnet is available
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
	Write-Error "dotnet CLI not found in PATH. Install .NET SDK 10 and try again."
	exit 1
}

$publishDir = Join-Path -Path $PWD -ChildPath "publish-temp"
if (Test-Path $publishDir) { Remove-Item $publishDir -Recurse -Force }

$publishArgs = @(
	'publish',
	$ProjectPath,
	'-c', $Configuration,
	'-r', $Runtime,
	'--self-contained', 'true',
	'-p:PublishSingleFile=true',
	'-p:IncludeAllContentForSelfExtract=true',
	'-p:PublishTrimmed=false',
	'-p:DebugType=None',
	'-o', $publishDir
)

Write-Host "Running: dotnet $($publishArgs -join ' ')"
$proc = Start-Process dotnet -ArgumentList $publishArgs -NoNewWindow -Wait -PassThru
if ($proc.ExitCode -ne 0) {
	Write-Error "dotnet publish failed with exit code $($proc.ExitCode)"
	exit $proc.ExitCode
}

# Prepare output zip
$outputDir = Join-Path -Path $PWD -ChildPath "publish-output"
if (-not (Test-Path $outputDir)) { New-Item -ItemType Directory -Path $outputDir | Out-Null }

$exeName = 'MACH Cal.exe'
# If the project assembly name differs, try to detect the exe name
$exeCandidate = Get-ChildItem -Path $publishDir -Filter '*.exe' | Select-Object -First 1
if ($exeCandidate) { $exeName = $exeCandidate.Name }

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$zipPath = Join-Path $outputDir "MACH_Cal-$Runtime-$timestamp.zip"

Write-Host "Creating zip: $zipPath"
Compress-Archive -Path (Join-Path $publishDir '*') -DestinationPath $zipPath -Force

Write-Host "Publish complete. Output: $zipPath"
Write-Host "You can distribute the zip file. The single-file EXE will be inside it."

exit 0
