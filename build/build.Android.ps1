param(
    [INT]   $BuildNumber,
    [string]$KeystorePassword,
    [string]$BuildConfiguration = "Release",
    [string]$branch = "master",
    [string]$AppCenterKey = $NULL,
    [string]$GoogleMapKey = $NULL,
    [switch]$noSupervisor,
    [switch]$noLiteInterviewer,
    [switch]$noExtInterviewer)

$ErrorActionPreference = "Stop"

$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName

. "$scriptFolder\functions.ps1"

versionCheck
"GIT_BRANCH: $ENV:GIT_BRANCH" | Out-Host
Load-DevEnvVariables

$versionString = (GetVersionString 'src')

Log-Block "Update project version" {
    UpdateProjectVersion $BuildNumber -ver $versionString -branch $branch
    Write-Host "##teamcity[setParameter name='system.VersionString' value='$versionString']"
}

$artifactsFolder = (Get-Location).Path + "\Artifacts"
Log-Message "Artifacts Folder: $artifactsFolder"

If (Test-Path "$artifactsFolder") {
    Remove-Item "$artifactsFolder" -Force -Recurse | Out-Null
}

New-Item $artifactsFolder -Type Directory -Force | Out-Null

try {

    if ($noLiteInterviewer.IsPresent -eq $False) {
        $PackageName = 'WBCapi.apk'
        . "$scriptFolder\build-android-package.ps1" `
            -VersionName $versionString `
            -VersionCode $BuildNumber `
            -BuildConfiguration $BuildConfiguration `
            -KeystorePassword $KeystorePassword `
            -KeystoreName 'WBCapi.keystore' `
            -AppCenterKey $AppCenterKey `
            -GoogleMapKey $GoogleMapKey `
            -KeystoreAlias 'wbcapipublish' `
            -CapiProject 'src\UI\Interviewer\WB.UI.Interviewer\WB.UI.Interviewer.csproj' `
            -OutFileName "$artifactsFolder\$PackageName" `
            -NoCleanUp `
            -ExcludeExtra:$true | % { if (-not $_) { Exit } }
    }
    
    if ($noSupervisor.IsPresent -eq $False) {
        $SuperPackageName = 'Supervisor.apk'
        . "$scriptFolder\build-android-package.ps1" `
            -VersionName $versionString `
            -VersionCode $BuildNumber `
            -BuildConfiguration $BuildConfiguration `
            -KeystorePassword $KeystorePassword `
            -KeystoreName 'WBCapi.keystore' `
            -AppCenterKey $AppCenterKey `
            -GoogleMapKey $GoogleMapKey `
            -KeystoreAlias 'wbcapipublish' `
            -CapiProject 'src\UI\Supervisor\WB.UI.Supervisor\WB.UI.Supervisor.csproj' `
            -OutFileName "$artifactsFolder\$SuperPackageName" `
            -NoCleanUp `
            -ExcludeExtra:$false | % { if (-not $_) { Exit } }
    }

    if ($noExtInterviewer.IsPresent -eq $False) {
        $ExtPackageName = 'WBCapi.Ext.apk'
        . "$scriptFolder\build-android-package.ps1" `
            -VersionName $versionString `
            -VersionCode $BuildNumber `
            -BuildConfiguration $BuildConfiguration `
            -KeystorePassword $KeystorePassword `
            -KeystoreName 'WBCapi.keystore' `
            -AppCenterKey $AppCenterKey `
            -GoogleMapKey $GoogleMapKey `
            -KeystoreAlias 'wbcapipublish' `
            -CapiProject 'src\UI\Interviewer\WB.UI.Interviewer\WB.UI.Interviewer.csproj' `
            -OutFileName "$artifactsFolder\$ExtPackageName" `
            -branch $branch `
            -NoCleanUp `
            -ExcludeExtra:$false | % { if (-not $_) { Exit } }
    }

    Write-Host "##teamcity[publishArtifacts '$artifactsFolder']"
}
catch {
    Log-Error Unexpected error occurred
    throw
}
