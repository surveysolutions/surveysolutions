#--------------------------------------------------

$root = 'C:\Users\Anatoliy\Desktop\malawi\test1'
$zipFileName = '3.18_2.zip'

if (-not (Test-Path "$root\$zipFileName")){
    Write-Host "Sync package not found: $root\$zipFileName"
    Write-Host 'Please edit this script and specify correct folder and sync package file'
    Exit
}

#--------------------------------------------------

Add-Type -AssemblyName System.IO.Compression.FileSystem
function Unzip-File
{
    param([string]$zipfile, [string]$outpath)

    [System.IO.Compression.ZipFile]::ExtractToDirectory($zipfile, $outpath)
}

Function DeGZip-File{
    Param(
        $infile,
        $outfile = ($infile -replace '\.gz$','')
        )

    $input = New-Object System.IO.FileStream $inFile, ([IO.FileMode]::Open), ([IO.FileAccess]::Read), ([IO.FileShare]::Read)
    $output = New-Object System.IO.FileStream $outFile, ([IO.FileMode]::Create), ([IO.FileAccess]::Write), ([IO.FileShare]::None)
    $gzipStream = New-Object System.IO.Compression.GzipStream $input, ([IO.Compression.CompressionMode]::Decompress)

    $buffer = New-Object byte[](1024)
    while($true){
        $read = $gzipstream.Read($buffer, 0, 1024)
        if ($read -le 0){break}
        $output.Write($buffer, 0, $read)
        }

    $gzipStream.Close()
    $output.Close()
    $input.Close()
}

#--------------------------------------------------

Function Get-JsonObjectLength {
    param($jsonObject)

    $jsonString = ConvertTo-Json -Compress $jsonObject
    return $jsonString.Length
}

Function Get-ShortTypeName {
    param([string] $fullTypeName)

    return $fullTypeName `
        -replace 'WB.Core.SharedKernels.DataCollection.Events.Interview.', '' `
        -replace ', WB.Core.SharedKernels.DataCollection', ''
}

#--------------------------------------------------

cd $root

$packageName = [System.IO.Path]::GetFileNameWithoutExtension($zipFileName)

Write-Host 'Unzipping package...'

Unzip-File "$root\$zipFileName" $root

$syncPackage = @(dir "$root\$packageName\*.sync" | %{ $_.FullName })[0]

Write-Host 'Reading json package file...'

$packageJson = (Get-Content $syncPackage) -join "`n" | ConvertFrom-Json

$isCompressed = $packageJson.IsCompressed
if (-not $isCompressed) {
    Write-Host 'I support only compressed content. Please upgrade me to support any.'
    Exit
}

$base64Content = $packageJson.Content

$compressedContentBytes = [System.Convert]::FromBase64String($base64Content)

Write-Host 'Writing gzip content...'

[io.file]::WriteAllBytes("$root\$packageName.content.json.gz", $compressedContentBytes)

Write-Host 'Extracting json content...'

DeGZip-File "$root\$packageName.content.json.gz"

Write-Host 'Reading json content file...'

$contentJson = (Get-Content -Encoding Unicode "$root\$packageName.content.json") -join "`n" | ConvertFrom-Json

Write-Host 'Extracting and writing event types...'

$contentJson.'$values' `
    | %{ "$(Get-ShortTypeName $_.Payload.'$type') $(Get-JsonObjectLength $_.Payload)" } `
    > "$root\$packageName.event-types.txt"
