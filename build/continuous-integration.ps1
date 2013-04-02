$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName
$ErrorActionPreference = "Stop"

. "$scriptFolder\functions.ps1"


CleanBinAndObjFolders

BuildSolutions 'Debug' | %{ if (-not $_) { Exit } }

RunTests 'Debug'
