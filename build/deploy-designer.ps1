$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName

. "$scriptFolder\functions.ps1"


CleanBinAndObjFolders

BuildSolution '.\src\Designer.sln' 'Release' | %{ if (-not $_) { Exit } }

RunTests 'Release'

& (GetPathToMSBuild) '.\src\UI\Designer\WB.UI.Designer\WB.UI.Designer.csproj' '/t:Package' '/p:Configuration=Release'
