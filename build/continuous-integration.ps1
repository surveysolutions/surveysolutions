$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName
$ErrorActionPreference = "Stop"

. "$scriptFolder\functions.ps1"


CleanBinAndObjFolders

BuildSolutions 'Release' | %{ if (-not $_) { Exit } }

RunTests 'Release'
