$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName

. "$scriptFolder\functions.ps1"


CleanBinAndObjFolders

BuildSolutions 'Release' | %{ if (-not $_) { Exit } }

RunTests 'Release'
