param([switch] $Deep)

$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName
$ErrorActionPreference = "Stop"

. "$scriptFolder\functions.ps1"


CleanBinAndObjFolders

if ($Deep) {
    BuildSolutions 'Release' -ClearBinAndObjFoldersBeforeEachSolution | %{ if (-not $_) { Exit } }
} else {
    BuildSolutions 'Release' | %{ if (-not $_) { Exit } }
}

RunTests 'Release'
