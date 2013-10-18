. .\build\functions.ps1

Get-ChildItem -Recurse `
    | %{ GetPathRelativeToCurrectLocation $_.FullName } `
    | ?{ $_.Length -gt 210 } `
    | %{ New-Object PSObject -Property @{ File = $_; ExcessiveCharacters = $_.Length - 210 } } `
    | Format-List `
