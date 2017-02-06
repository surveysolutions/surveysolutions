param([string]$BuildConfiguration='Release')

$ErrorActionPreference = "Stop"

$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName

. "$scriptFolder\functions.ps1"
try {
    RunTests $BuildConfiguration   
}
catch {
    Write-Host "##teamcity[message status='ERROR' text='Unexpected error occurred']"
    Write-Host "##teamcity[buildProblem description='Unexpected error occurred']"
    throw
}