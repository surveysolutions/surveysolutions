param( [string]$DeployFolder )

$ErrorActionPreference = "Stop"

$TargetDeployFolder = '\\192.168.3.113\Web\Designer'

if($DeployFolder)
{$TargetDeployFolder = $DeployFolder} 

$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName

. "$scriptFolder\deployment-functions.ps1"


try {

    Deploy `
        -Solution 'src\Designer.sln' `
        -Project 'src\UI\Designer\WB.UI.Designer\WB.UI.Designer.csproj' `
        -BuildConfiguration 'Release' `
        -SourceFolder 'src\UI\Designer\WB.UI.Designer\obj\Release\Package\PackageTmp' `
        -TargetFolder $TargetDeployFolder 

}
catch {
    Write-Host "##teamcity[message status='ERROR' text='Unexpected error occurred']"
    Write-Host "##teamcity[buildStatus status='FAILURE' text='Unexpected error occurred']"
    throw
}
