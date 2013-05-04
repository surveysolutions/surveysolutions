$ErrorActionPreference = "Stop"

$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName

. "$scriptFolder\deployment-functions.ps1"


try {

    Deploy `
        -Solution 'RavenQuestionnaire.Web\RavenQuestionnaire.Web.sln' `
        -Project 'RavenQuestionnaire.Web\Web.Supervisor\Web.Supervisor.csproj' `
        -BuildConfiguration 'Release' `
        -SourceFolder 'RavenQuestionnaire.Web\Web.Supervisor\obj\Release\Package\PackageTmp' `
        -TargetFolder '\\192.168.3.113\Dev\Supervisor' `

}
catch {
    Write-Host "##teamcity[message status='ERROR' text='Unexpected error occurred']"
    Write-Host "##teamcity[buildStatus status='FAILURE' text='Unexpected error occurred']"
}
