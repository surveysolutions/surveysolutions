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

$ProjectHeadquarters = 'src\UI\Headquarters\WB.UI.Headquarters\WB.UI.Headquarters.csproj'
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

    $buildArgs = @("/p:BuildNumber=$BuildNumber", "/p:VersionSuffix=$branch")

    $buildSuccessful = BuildSolution -Solution $MainSolution -BuildConfiguration $BuildConfiguration -BuildArgs $buildArgs
    if ($buildSuccessful) { 

        New-Item "$artifactsFolder\stats" -Type Directory -Force | Out-Null

        if($nostatic -eq $False){
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
        }

        Log-Block "Run configuration transformations" {
            RunConfigTransform $ProjectHeadquarters $BuildConfiguration
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
            BuildWebPackage $ProjectHeadquarters $BuildConfiguration | % { if (-not $_) { Exit } }

            if($nosupport.IsPresent -eq $False) {
                BuildAndDeploySupportTool $SupportToolSolution $BuildConfiguration | % { if (-not $_) { Exit } }
            }
        }

        "BuildAspNetCoreWebPackage $ProjectWebTester $BuildConfiguration $BuildNumber $branch" | Write-Verbose
        BuildAspNetCoreWebPackage $ProjectWebTester $BuildConfiguration $BuildNumber $branch | ForEach-Object { if (-not $_) { Exit 1 } }
        
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
