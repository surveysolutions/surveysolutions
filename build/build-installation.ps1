param(
#      [string]$cmd,
      [string]$HQSourcePath, 
	  [switch]$noDestCleanup,
	  [INT]   $BuildNumber)

$ErrorActionPreference = "Stop"

$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName

. "$scriptFolder\functions.ps1"

Add-Type -AssemblyName System.Web.Extensions

$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName
. "$scriptFolder\functions.ps1"

$sourceCleanup = $False

$workdir = Get-Location
if ($HQSourcePath -eq "") {
	$HQSourcePath = Join-Path $workdir "HQpackage"
	$sourceCleanup = $True
}

$InstallationProject = 'src\SurveySolutionsBootstrap\SurveySolutionsBootstrap.wixproj'
$MainInstallationSolution = 'src\SurveySolutionsBootstrap.sln'

#Set-Location $HQSourcePath
$sitePatha = (Get-ChildItem $HQSourcePath -recurse | Where-Object {$_.PSIsContainer -eq $true -and $_.Name -match "dist"}).FullName

$HQsitePath = Join-path $workdir "HQwork"

#remove old files
if (!($noDestCleanup)) {
	if (Test-Path $HQsitePath){
		Remove-Item $HQsitePath -Force -Recurse
	}
}

if (!(Test-Path $HQsitePath)) {
	New-Item $HQsitePath -ItemType Directory
	New-Item $HQsitePath\Site -ItemType Directory
#	New-Item (Join-Path $HQsitePath "App_Data") -ItemType Directory
}

#$supportPath = Join-path $workdir "SupportPackage"
#$targetSupportPath = Join-path $HQsitePath "Support"

Copy-Item $sitePatha\* $HQsitePath\Site -Force -Recurse -Exclude '*.pdb'
Rename-Item $HQsitePath\Site\web.config $HQsitePath\Site\Web.config

#Remove-Item "$HQsitePath\HostMap.config"

Copy-Item $HQSourcePath\Client $HQsitePath\Site\Client -Force -Recurse -Exclude '*.pdb'

$files = (Get-ChildItem -Path $HQsitePath\Site -recurse | Where-Object {$_.Name -match "WB.UI.Headquarters.dll" <# -or $_.Name -match "WB.UI.Headquarters.exe" #>})

foreach($file in $files) {
    $versionOfProduct = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($file.FullName)
    
    if(($versionOfProduct.FileVersion -eq '') -or ($null -eq $versionOfProduct.FileVersion)) {
        continue
    }

    break;
}
# $version = $newVersion = "{0}{1}.{2}.{3}.{4}" -f $versionOfProduct.ProductMajorPart, $versionOfProduct.ProductMinorPart.ToString("00"), $versionOfProduct.ProductBuildPart, $versionOfProduct.ProductPrivatePart, $BuildNumber
$productFileVersion = $versionOfProduct.FileVersion

$installationArgs = @(
    $InstallationProject;
    '/t:Build';
    "/p:HarvestDir=$HQsitePath";
    "/p:HarvestDirectory=$HQsitePath";
    "/p:Configuration=Release";
    "/p:Platform=x86";
    "/p:SurveySolutionsVersion=$productFileVersion";
	"/p:HqVersion=$productFileVersion";
)

$pathToMsBuild = GetPathToMSBuild

Log-Message "Calling build from $pathToMsBuild with params: $installationArgs" 

& (GetPathToMSBuild) $installationArgs | Write-Host

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
