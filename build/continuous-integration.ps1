param([switch] $Deep)

$ErrorActionPreference = "Stop"

$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName

. "$scriptFolder\functions.ps1"


CleanBinAndObjFolders

if ($Deep) {
    BuildSolutions 'Release' -ClearBinAndObjFoldersBeforeEachSolution | %{ if (-not $_) { Exit } }
} else {
    BuildSolutions 'Release' | %{ if (-not $_) { Exit } }
}

RunTests 'Release'
