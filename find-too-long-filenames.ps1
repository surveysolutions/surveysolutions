. .\build\functions.ps1

Get-ChildItem -Recurse `
    | %{ GetPathRelativeToCurrectLocation $_.FullName } `
    | ?{ $_.Length -gt 219 } `
    | %{ New-Object PSObject -Property @{ File = $_; ExcessiveCharacters = $_.Length - 219 } } `
    | Format-List `
