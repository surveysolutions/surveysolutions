param(
    [string]$HQSourcePath,
    [string]$ClientPath,
    [string]$ProductFileVersion)

$ErrorActionPreference = "Stop"

$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName

. "$scriptFolder\functions.ps1"


$HQsitePath = Join-Path (Get-Location) "Work"
Log-Message "Working folder : $HQsitePath"

if (!(Test-Path $HQsitePath)) {
	New-Item $HQsitePath -ItemType Directory
	New-Item $HQsitePath\Site -ItemType Directory
    New-Item $HQsitePath\Site\Client -ItemType Directory
}

Copy-Item $HQSourcePath\* $HQsitePath\Site -Force -Recurse -Exclude '*.pdb'
Copy-Item $ClientPath\*.apk $HQsitePath\Site\Client -Force

if (-not $PSBoundParameters.ContainsKey('ProductFileVersion')) {
    $files = (Get-ChildItem -Path $HQsitePath\Site -recurse | Where-Object {$_.Name -match "WB.UI.Headquarters.dll" -or $_.Name -match "WB.UI.Headquarters.exe" })

    Log-Message "files for version check : $files"

    foreach($file in $files) {
        Log-Message "Checking version for: $file.FullName"
        $versionOfProduct = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($file.FullName)
        
        if(($versionOfProduct.FileVersion -eq '') -or ($null -eq $versionOfProduct.FileVersion)) {
            continue
        }

        break;
    }

    $productFileVersion = $versionOfProduct.FileVersion
}

Log-Message "Version from file: $productFileVersion"


$InstallationProject = "$scriptFolder\..\src\SurveySolutionsBootstrap\SurveySolutionsBootstrap.wixproj"

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
