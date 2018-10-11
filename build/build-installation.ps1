param(
#      [string]$cmd,
      [string]$HQSourcePath, 
	  [switch]$noDestCleanup)

$ErrorActionPreference = "Stop"

$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName
. "$scriptFolder\functions.ps1"

$InstallationProject = 'src\Installation\SurveySolutions\SurveySolutionsBootstrap\SurveySolutionsBootstrap.wixproj'

$sourceCleanup = $False

$workdir = Get-Location
if ($HQSourcePath -eq "") {
	$HQSourcePath = Join-Path $workdir "HQpackage"
	$sourceCleanup = $True
}

#Set-Location $HQSourcePath
$sitePatha = (Get-ChildItem -recurse | Where-Object {$_.PSIsContainer -eq $true -and $_.Name -match "PackageTmp"}).FullName

$HQsitePath = Join-path $workdir "HQwork"
if (!(Test-Path $HQsitePath)) {
	New-Item $HQsitePath -ItemType Directory
	New-Item (Join-Path $HQsitePath "App_Data") -ItemType Directory
}

$supportPath = Join-path $workdir "SupportPackage"
$targetSupportPath = Join-path $HQsitePath "Support"

Copy-Item $sitePatha\* $HQsitePath -Force -Recurse
Remove-Item "$HQsitePath\HostMap.config"

Copy-Item -Path $supportPath -Destination $targetSupportPath -Force -Recurse

$file = (Get-ChildItem -Path $HQsitePath -recurse | Where-Object {$_.Name -match "WB.UI.Headquarters.dll"})
$version = [Reflection.AssemblyName]::GetAssemblyName($file.FullName).Version

setupExportService

& (GetPathToMSBuild) $InstallationProject '/t:Build' "/p:HarvestDir=$HQsitePath" "/p:HarvestDirectory=$HQsitePath" "/p:Configuration=Release" "/p:Platform=x64" "/p:SurveySolutionsVersion=$version" | Write-Host

$wasBuildSuccessfull = $LASTEXITCODE -eq 0

if (-not $wasBuildSuccessfull) {
    Write-Host "##teamcity[message status='ERROR' text='Failed to build installation']"
    if (-not $MultipleSolutions) {
        Write-Host "##teamcity[buildProblem description='Failed to build installation']"
    }
}

Set-Location $workdir

if (!($noDestCleanup)) {
	Remove-Item $HQsitePath -Force -Recurse
}
if ($sourceCleanup) {
	Remove-Item $HQSourcePath -Force -Recurse
}

Write-Host "##teamcity[publishArtifacts 'SurveySolutions.msi']"

function setupExportService() {
    Copy-Item $HQSourcePath\ExportService\* $HQsitePath\.bin\Export -Force -Recurse

    $exportSettingsPath = "$HQsitePath\.bin\Export\appsettings.json"
    
    $exportSettings = Get-Content $exportSettingsPath -raw | ConvertFrom-Json
    $exportSettings.ConnectionStrings.DefaultConnection = "{FROM_INSTALLER}"
    
    $exportSettings | ConvertTo-Json | set-content $exportSettingsPath    
}
