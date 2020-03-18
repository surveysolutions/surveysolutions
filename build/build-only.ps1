param([string]$VersionPrefix,
    [INT]$BuildNumber,
    [string]$KeystorePassword,
    [string]$BuildConfiguration = "Release",
    [string]$branch = "master",
    [switch]$nostatic,
    [switch]$noandroid,
    [switch]$nosupport)

$ErrorActionPreference = "Stop"

$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName

. "$scriptFolder\functions.ps1"

$ProjectHeadquarters = 'src\UI\WB.UI.Headquarters.Core\WB.UI.Headquarters.csproj'
$ProjectWebTester = 'src\UI\WB.UI.WebTester\WB.UI.WebTester.csproj'
$MainSolution = 'src\WB without Xamarin.sln'
$SupportToolSolution = 'src\Tools\support\support.sln'

"VersionPrefix: $VersionPrefix, BuildNumber: $BuildNumber, branch: $branch, nostatic: $nostatic, noandroid: $noandroid" | Out-Host

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

   # $buildArgs = @("/p:BuildNumber=$BuildNumber", "/p:VersionSuffix=$branch")

    $buildSuccessful = $true # BuildSolution -Solution $MainSolution -BuildConfiguration $BuildConfiguration -BuildArgs $buildArgs
    if ($buildSuccessful) { 

        New-Item "$artifactsFolder\stats" -Type Directory -Force | Out-Null

        if($nostatic -eq $False){
        
            BuildStaticContent "WB.UI.Frontend" "src\UI\WB.UI.Frontend" @("build") | % { if (-not $_) {
                Log-Error 'Unexpected error occurred in BuildStaticContent while build static content for WB.UI.FrontEnd'
                Exit 
            }}
            
        }

        if($noandroid.IsPresent -eq $False) 
        {
            $PackageName = 'WBCapi.apk'
            . "$scriptFolder\build-android-package.ps1" `
                -VersionName $versionString `
                -VersionCode $BuildNumber `
                -BuildConfiguration $BuildConfiguration `
                -KeystorePassword $KeystorePassword `
                -KeystoreName 'WBCapi.keystore' `
                -KeystoreAlias 'wbcapipublish' `
                -CapiProject 'src\UI\Interviewer\WB.UI.Interviewer\WB.UI.Interviewer.csproj' `
                -OutFileName "$(Split-Path $ProjectHeadquarters)\Client\$PackageName" `
                -NoCleanUp `
                -ExcludeExtra:$true | % { if (-not $_) { Exit } }

            $ExtPackageName = 'WBCapi.Ext.apk'
            . "$scriptFolder\build-android-package.ps1" `
                -VersionName $versionString `
                -VersionCode $BuildNumber `
                -BuildConfiguration $BuildConfiguration `
                -KeystorePassword $KeystorePassword `
                -KeystoreName 'WBCapi.keystore' `
                -KeystoreAlias 'wbcapipublish' `
                -CapiProject 'src\UI\Interviewer\WB.UI.Interviewer\WB.UI.Interviewer.csproj' `
                -OutFileName "$(Split-Path $ProjectHeadquarters)\Client\$ExtPackageName" `
                -NoCleanUp `
                -ExcludeExtra:$false | % { if (-not $_) { Exit } }

            $SuperPackageName = 'Supervisor.apk'
            . "$scriptFolder\build-android-package.ps1" `
                -VersionName $versionString `
                -VersionCode $BuildNumber `
                -BuildConfiguration $BuildConfiguration `
                -KeystorePassword $KeystorePassword `
                -KeystoreName 'WBCapi.keystore' `
                -KeystoreAlias 'wbcapipublish' `
                -CapiProject 'src\UI\Supervisor\WB.UI.Supervisor\WB.UI.Supervisor.csproj' `
                -OutFileName "$(Split-Path $ProjectHeadquarters)\Client\$SuperPackageName" `
                -NoCleanUp `
                -ExcludeExtra:$false | % { if (-not $_) { Exit } }
        }

        Log-Block "Building HQ web package and support tool" {
            BuildAspNetCoreWebPackage $ProjectHeadquarters $BuildConfiguration $BuildNumber $branch
        }
        
        "BuildAspNetCoreWebPackage $ProjectWebTester $BuildConfiguration $BuildNumber $branch" | Write-Verbose
        BuildAspNetCoreWebPackage $ProjectWebTester $BuildConfiguration $BuildNumber $branch
        
        Log-Block "Collecting/building artifacts" {
            AddArtifacts $ProjectHeadquarters $BuildConfiguration -folder "Headquarters"
            AddArtifacts $ProjectWebTester $BuildConfiguration -folder "WebTester"
        }

        Write-Host "##teamcity[publishArtifacts '$artifactsFolder']"
    }
}
catch {
    Log-Error Unexpected error occurred
    throw
}
