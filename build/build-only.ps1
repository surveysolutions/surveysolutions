param([string]$VersionPrefix,
    [INT]$BuildNumber,
    [string]$KeystorePassword,
    [string]$BuildConfiguration = "Release")

$ErrorActionPreference = "Stop"

$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName

. "$scriptFolder\functions.ps1"

$ProjectWebInterview = 'src\UI\Headquarters\WB.UI.Headquarters.Interview'
$ProjectDesigner = 'src\UI\Designer\WB.UI.Designer\WB.UI.Designer.csproj'
$ProjectHeadquarters = 'src\UI\Headquarters\WB.UI.Headquarters\WB.UI.Headquarters.csproj'
$MainSolution = 'src\WB.sln'
$SupportToolSolution = 'src\Tools\support\support.sln'

versionCheck

$versionString = (GetVersionString 'src\core')
UpdateProjectVersion $BuildNumber -ver $versionString
Write-Host "##teamcity[setParameter name='system.VersionString' value='$versionString']"

$artifactsFolder = (Get-Location).Path + "\Artifacts"

Write-Host "Artifacts Folder: $artifactsFolder"

If (Test-Path "$artifactsFolder") {
    Remove-Item "$artifactsFolder" -Force -Recurse
}

New-Item $artifactsFolder -Type Directory -Force


try {
    $buildSuccessful = BuildSolution `
        -Solution $MainSolution `
        -BuildConfiguration $BuildConfiguration
    if ($buildSuccessful) { 

        BuildStaticContent "Designer Questionnaire" "src\UI\Designer\WB.UI.Designer\questionnaire" | % { if (-not $_) { 
            Write-Host "##teamcity[message status='ERROR' text='Unexpected error occurred in BuildStaticContent']"
            Write-Host "##teamcity[buildProblem description='Failed to build static content for Designer']"
            Exit 
        }}
        
        BuildStaticContent "Hq Deps" "src\UI\Headquarters\WB.UI.Headquarters\Dependencies" | % { if (-not $_) {
                Write-Host "##teamcity[message status='ERROR' text='Unexpected error occurred in BuildStaticContent']"
                Write-Host "##teamcity[buildProblem description='Failed to build static content for HQ']"
                Exit 
            }}

        BuildStaticContent "Hq UI" "src\UI\Headquarters\WB.UI.Headquarters" | % { if (-not $_) {
                Write-Host "##teamcity[message status='ERROR' text='Unexpected error occurred in BuildStaticContent']"
                Write-Host "##teamcity[buildProblem description='Failed to build static content for HQ']"
                Exit 
        } else {
            Move-Item "..\WB.UI.Headquarters\Dependencies\build\stats.html" "$artifactsFolder\Hq.UI.stats.html" -ErrorAction SilentlyContinue
        }}

        BuildStaticContent "Web Interview" $ProjectWebInterview | % { if (-not $_) { 
            Write-Host "##teamcity[message status='ERROR' text='Unexpected error occurred in BuildStaticContent']"
            Write-Host "##teamcity[buildProblem description='Failed to build Web interview application']"
            Exit
        } else {
            Move-Item "..\WB.UI.Headquarters\InterviewApp\stats.html" "$artifactsFolder\WebInterview.stats.html" -ErrorAction SilentlyContinue
        }}

        RunConfigTransform $ProjectDesigner $BuildConfiguration
        RunConfigTransform $ProjectHeadquarters $BuildConfiguration

        $ExtPackageName = 'WBCapi.Ext.apk'
        . "$scriptFolder\build-android-package.ps1" `
            -VersionName $versionString `
            -VersionCode $BuildNumber `
            -BuildConfiguration $BuildConfiguration `
            -KeystorePassword $KeystorePassword `
            -KeystoreName 'WBCapi.keystore' `
            -KeystoreAlias 'wbcapipublish' `
            -CapiProject 'src\UI\Interviewer\WB.UI.Interviewer\WB.UI.Interviewer.csproj' `
            -OutFileName $ExtPackageName `
            -ExcludeExtra $false | % { if (-not $_) { Exit } }

         CopyCapi -Project $ProjectHeadquarters -source $ExtPackageName -cleanUp $true | % { if (-not $_) { Exit } }

        #remove leftovers after previous build

        #CleanFolders 'bin' | %{ if (-not $_) { Exit } }
        #CleanFolders 'obj' | %{ if (-not $_) { Exit } }

        $PackageName = 'WBCapi.apk'
        . "$scriptFolder\build-android-package.ps1" `
            -VersionName $versionString `
            -VersionCode $BuildNumber `
            -BuildConfiguration $BuildConfiguration `
            -KeystorePassword $KeystorePassword `
            -KeystoreName 'WBCapi.keystore' `
            -KeystoreAlias 'wbcapipublish' `
            -CapiProject 'src\UI\Interviewer\WB.UI.Interviewer\WB.UI.Interviewer.csproj' `
            -OutFileName $PackageName `
            -ExcludeExtra $true | % { if (-not $_) { Exit } }

        CopyCapi -Project $ProjectHeadquarters -source $PackageName -cleanUp $false | % { if (-not $_) { Exit } }

        BuildWebPackage $ProjectHeadquarters $BuildConfiguration | % { if (-not $_) { Exit } }
        BuildWebPackage $ProjectDesigner $BuildConfiguration | % { if (-not $_) { Exit } }

        BuildAndDeploySupportTool $SupportToolSolution $BuildConfiguration | % { if (-not $_) { Exit } }

        AddArtifacts $ProjectDesigner $BuildConfiguration -folder "Designer"
        AddArtifacts $ProjectHeadquarters $BuildConfiguration -folder "Headquarters"

        Write-Host "##teamcity[publishArtifacts '$artifactsFolder']"
    }
}
catch {
    Write-Host "##teamcity[message status='ERROR' text='Unexpected error occurred']"
    Write-Host "##teamcity[buildProblem description='Unexpected error occurred']"
    throw
}