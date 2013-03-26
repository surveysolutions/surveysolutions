$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName

. "$scriptFolder\functions.ps1"


function Deploy($Solution, $Project, $BuildConfiguration, $SourceFolder, $TargetFolder) {

    CleanBinAndObjFolders

    BuildSolution $Solution $BuildConfiguration | %{ if (-not $_) { Exit } }

    RunTests $BuildConfiguration

    & (GetPathToMSBuild) $Project '/t:Package' "/p:Configuration=$BuildConfiguration" | Write-Host

    Remove-Item "$TargetFolder\*" -Force -Recurse

    Copy-Item "$SourceFolder\*" $TargetFolder -Recurse
}


Deploy `
    -Solution 'src\Designer.sln' `
    -Project 'src\UI\Designer\WB.UI.Designer\WB.UI.Designer.csproj' `
    -BuildConfiguration 'Release' `
    -SourceFolder 'src\UI\Designer\WB.UI.Designer\obj\Release\Package\PackageTmp' `
    -TargetFolder '\\192.168.3.113\Web\Designer' `
