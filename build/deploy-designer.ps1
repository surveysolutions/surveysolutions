$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName

. "$scriptFolder\functions.ps1"

CleanBinAndObjFolders

BuildSolution '.\src\Designer.sln' 'Release'

RunTests 'Release'
