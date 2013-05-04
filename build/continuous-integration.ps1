$ErrorActionPreference = "Stop"

$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName

. "$scriptFolder\functions.ps1"


try {

    CleanBinAndObjFolders

    BuildSolutions 'Release' | %{ if (-not $_) { Exit } }

    RunTests 'Release'

}
catch {
    Write-Host "##teamcity[message status='ERROR' text='Unexpected error occurred']"
    Write-Host "##teamcity[buildStatus status='FAILURE' text='Unexpected error occurred']"
}
