$targetFolder = 'ready-for-reimport'

Write-Host `
"
This script is intended to transform income packages with errors (sent from IN to SV)
to format which can be put back to the income packages folder and reprocessed.
Just copy it to folder with not processed sync packages (usually App_Data\IncomingData\IncomingDataWithErrors) and run.
It will create a subfolder '$targetFolder' and will put sync packages in format ready for reimport there.

It is strongly recommended not to use this script on production site!!!
In such case, create a copy of files and run this script on that copy.
"

New-Item -ItemType Directory -Force -Path ".\$targetFolder" >$null

Get-ChildItem '.' -Filter '*.sync' | `
Foreach-Object {

    $filename = $_.Name

    $json = (Get-Content $filename) -join "`n" | ConvertFrom-Json

    if (-not $json.IsCompressed) {
        throw [System.Exception] "File content is not compressed: $filename"
    }

    $json.Content | Out-File ".\$targetFolder\$filename"

    Write-Host "$targetFolder\$filename"

}


