$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName
$ErrorActionPreference = "Stop"

. "$scriptFolder\deployment-functions.ps1"


Deploy `
    -Solution 'src\Supervisor.sln' `
    -Project 'RavenQuestionnaire.Web\Web.Supervisor\Web.Supervisor.csproj' `
    -BuildConfiguration 'Deploy-dev' `
    -SourceFolder 'RavenQuestionnaire.Web\Web.Supervisor\obj\Deploy-dev\Package\PackageTmp' `
    -TargetFolder '\\192.168.3.113\Dev\Supervisor' `
