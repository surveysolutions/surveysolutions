$scriptFolder = (Get-Item $MyInvocation.MyCommand.Path).Directory.FullName

. "$scriptFolder\deployment-functions.ps1"


Deploy `
    -Solution 'src\Designer.sln' `
    -Project 'src\UI\Designer\WB.UI.Designer\WB.UI.Designer.csproj' `
    -BuildConfiguration 'Release' `
    -SourceFolder 'src\UI\Designer\WB.UI.Designer\obj\Release\Package\PackageTmp' `
    -TargetFolder '\\192.168.3.113\Web\Designer' `
