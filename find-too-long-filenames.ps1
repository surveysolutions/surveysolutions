. .\build\functions.ps1

Get-ChildItem -Recurse `
    | %{ GetPathRelativeToCurrectLocation $_.FullName } `
    | ?{ $_.Length -gt 222 } `
    | %{ New-Object PSObject -Property @{ File = $_; ExcessiveCharacters = $_.Length - 222 } } `
    | Format-List `
