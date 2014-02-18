param([switch] $Deep)

$ErrorActionPreference = "Stop"

$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName

. "$scriptFolder\functions.ps1"


try {

    CheckPrerequisites | %{ if (-not $_) { Exit } }
    CleanBinAndObjFolders

if ($Deep) {
    BuildSolutions 'Release' -ClearBinAndObjFoldersBeforeEachSolution | %{ if (-not $_) { Exit } }
} else {
    BuildSolutions 'Release' | %{ if (-not $_) { Exit } }
}

    RunTests 'Release'

}
catch {
    Write-Host "##teamcity[message status='ERROR' text='Unexpected error occurred']"
    Write-Host "##teamcity[buildProblem description='Unexpected error occurred']"
    throw
}
