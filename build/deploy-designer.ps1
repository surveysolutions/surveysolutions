$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName

. "$scriptFolder\functions.ps1"

CleanBinAndObjFolders

BuildSolution '.\src\Designer.sln' 'Release' | %{ if (-not $_) { Exit } }

RunTests 'Release'
