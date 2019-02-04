param([string]$VersionPrefix,
    [INT]$BuildNumber,
    [string]$KeystorePassword,
    [string]$BuildConfiguration = "Release",
    [string]$branch = "master")

$ErrorActionPreference = "Stop"

$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName

. "$scriptFolder\functions.ps1"

$ProjectDesigner = 'src\UI\Designer\WB.UI.Designer\WB.UI.Designer.csproj'
$ProjectHeadquarters = 'src\UI\Headquarters\WB.UI.Headquarters\WB.UI.Headquarters.csproj'
$ProjectWebTester = 'src\UI\WB.UI.WebTester\WB.UI.WebTester.csproj'
$MainSolution = 'src\WB without Xamarin.sln'
$SupportToolSolution = 'src\Tools\support\support.sln'

versionCheck

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
    $buildSuccessful = BuildSolution -Solution $MainSolution -BuildConfiguration $BuildConfiguration
    if ($buildSuccessful) { 

        New-Item "$artifactsFolder\stats" -Type Directory -Force | Out-Null

        BuildStaticContent "Designer Questionnaire" "src\UI\Designer\WB.UI.Designer\questionnaire" | % { if (-not $_) { 
            Log-Error 'Unexpected error occurred in BuildStaticContent while build static content for Designer'
            Exit 
        }}
        
        BuildStaticContent "Hq Deps" "src\UI\Headquarters\WB.UI.Headquarters\Dependencies" | % { if (-not $_) {
            Log-Error 'Unexpected error occurred in BuildStaticContent while build static content for HQ Deps'
            Exit 
        }}

        BuildStaticContent "Hq App" "src\UI\Headquarters\WB.UI.Headquarters\HqApp" | % { if (-not $_) {
            Log-Error 'Unexpected error occurred in BuildStaticContent while build static content for HQ App'
            Exit 
        } else {
            Move-Item ".\dist\stats.html" "$artifactsFolder\stats\HqApp.html" -ErrorAction SilentlyContinue
            Move-Item ".\dist\shared_vendor.stats.html" "$artifactsFolder\stats\HqApp.vendor.html" -ErrorAction SilentlyContinue
            New-Item "$artifactsFolder\coverage" -Type Directory -Force
            Move-Item ".\.coverage" "$artifactsFolder\coverage\hqapp" -ErrorAction SilentlyContinue
        }}

        CreateZip "$artifactsFolder\stats" "$artifactsFolder\stats.zip"
        CreateZip "$artifactsFolder\coverage" "$artifactsFolder\coverage.zip"

        Remove-Item -Path "$artifactsFolder\stats" -Recurse -Force -ErrorAction SilentlyContinue
        Remove-Item -Path "$artifactsFolder\coverage" -Recurse -Force -ErrorAction SilentlyContinue

        Log-Block "Run configuration transformations" {
            RunConfigTransform $ProjectDesigner $BuildConfiguration
            RunConfigTransform $ProjectHeadquarters $BuildConfiguration
            RunConfigTransform $ProjectWebTester $BuildConfiguration
        }

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
            -ExcludeExtra $true | % { if (-not $_) { Exit } }

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
            -branch $branch `
            -NoCleanUp `
            -ExcludeExtra $false | % { if (-not $_) { Exit } }

        #remove leftovers after previous build

        #CleanFolders 'bin' | %{ if (-not $_) { Exit } }
        #CleanFolders 'obj' | %{ if (-not $_) { Exit } }

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
            -ExcludeExtra $false | % { if (-not $_) { Exit } }

        Log-Block "Building web packages and support tool" {
            BuildWebPackage $ProjectHeadquarters $BuildConfiguration | % { if (-not $_) { Exit } }
            BuildWebPackage $ProjectDesigner $BuildConfiguration | % { if (-not $_) { Exit } }
            BuildWebPackage $ProjectWebTester $BuildConfiguration | % { if (-not $_) { Exit } }
            BuildAndDeploySupportTool $SupportToolSolution $BuildConfiguration | % { if (-not $_) { Exit } }
        }

        AddArtifacts $ProjectDesigner $BuildConfiguration -folder "Designer"
        AddArtifacts $ProjectHeadquarters $BuildConfiguration -folder "Headquarters"
        AddArtifacts $ProjectWebTester $BuildConfiguration -folder "WebTester"

        Write-Host "##teamcity[publishArtifacts '$artifactsFolder']"
    }
}
catch {
    Log-Error Unexpected error occurred
    throw
}
