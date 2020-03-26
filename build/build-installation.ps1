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

$InstallationProject = 'src\Installation\SurveySolutions\SurveySolutionsBootstrap\SurveySolutionsBootstrap.wixproj'
$MainInstallationSolution = 'src\Installation\SurveySolutions\SurveySolutionsBootstrap.sln'

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

Copy-Item $sitePatha\* $HQsitePath\Site -Force -Recurse
#Remove-Item "$HQsitePath\HostMap.config"

Copy-Item $HQSourcePath\ExportService $HQsitePath\ExportService -Force -Recurse
Copy-Item $HQSourcePath\Client $HQsitePath\Site\Client -Force -Recurse
#Copy-Item -Path $supportPath -Destination $targetSupportPath -Force -Recurse

$file = (Get-ChildItem -Path $HQsitePath\Site -recurse | Where-Object {$_.Name -match "WB.UI.Headquarters.exe"})
$versionOfProduct = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($file.FullName)
$version = $newVersion = "{0}{1}.{2}.{3}.{4}" -f $versionOfProduct.ProductMajorPart, $versionOfProduct.ProductMinorPart.ToString("00"), $versionOfProduct.ProductBuildPart, $versionOfProduct.ProductPrivatePart, $BuildNumber
$productFileVersion = $versionOfProduct.FileVersion

#    [Reflection.AssemblyName]::GetAssemblyName($file.FullName).Version

# Cleaning up slack configuration section from config
#  $hqConfig = "$HQsitePath\Web.config"
#  [xml]$xml = Get-Content $hqConfig
#  $node = $xml.SelectSingleNode("//slack")
#  $node.ParentNode.RemoveChild($node)
#  $xml.save($hqConfig)

$installationArgs = @(
    $InstallationProject;
    '/t:Build';
    "/p:HarvestDir=$HQsitePath";
    "/p:HarvestDirectory=$HQsitePath";
    "/p:Configuration=Release";
    "/p:Platform=x86";
    "/p:SurveySolutionsVersion=$version";
	"/p:HqVersion=$productFileVersion";
)

$pathToMsBuild = GetPathToMSBuild


#Log-Block "Restore nuget" {
#        nuget restore $MainInstallationSolution
#    }


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
