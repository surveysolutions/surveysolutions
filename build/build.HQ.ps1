param([string]$VersionPrefix,
    [INT]$BuildNumber,
    [string]$BuildConfiguration = "Release",
    [string]$branch = "master")

$ErrorActionPreference = "Stop"

$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName

. "$scriptFolder\functions.ps1"

$ProjectHeadquarters = 'src\UI\Headquarters\WB.UI.Headquarters\WB.UI.Headquarters.csproj'
$MainSolution = 'src\WB without Xamarin.sln'

versionCheck
Load-DevEnvVariables

$versionString = (GetVersionString 'src')
Log-Block "Update project version" {
    UpdateProjectVersion $BuildNumber -ver $versionString -branch $branch
    Write-Host "##teamcity[setParameter name='system.VersionString' value='$versionString']"
}

$artifactsFolder = (Get-Location).Path + "\Artifacts"
Log-Message "Artifacts Folder: $artifactsFolder"

try {

    $buildArgs = @("/p:BuildNumber=$BuildNumber", "/p:VersionSuffix=$branch")

    $buildSuccessful = BuildSolution -Solution $MainSolution -BuildConfiguration $BuildConfiguration -BuildArgs $buildArgs
    if ($buildSuccessful) { 

        Log-Block "Building HQ Package" {
            BuildWebPackage $ProjectHeadquarters $BuildConfiguration | % { if (-not $_) { Exit } }
        }

        Log-Block "Building web packages and support tool" {
            BuildWebPackage $ProjectHeadquarters $BuildConfiguration | % { if (-not $_) { Exit } }
        }

        Log-Block "Collecting/building artifacts" {
            AddArtifacts $ProjectHeadquarters $BuildConfiguration -folder "Headquarters"
        }

        Write-Host "##teamcity[publishArtifacts '$artifactsFolder']"
    }
}
catch {
    Log-Error Unexpected error occurred
    throw
}
