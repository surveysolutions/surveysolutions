$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName

. "$scriptFolder\functions.ps1"

CleanBinAndObjFolders

BuildSolutions 'Debug'

RunTests 'Debug'
