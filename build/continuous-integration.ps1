$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName

. "$scriptFolder\functions.ps1"


CleanBinAndObjFolders

BuildSolutions 'Debug' | %{ if (-not $_) { Exit } }

RunTests 'Debug'
