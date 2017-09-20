$k6 = "https://github.com/loadimpact/k6/releases/download/v0.17.2/k6-v0.17.2-win64.zip"
$temp = ".\.tmp"

New-Item $temp -Type Directory -ErrorAction SilentlyContinue

try {
    Invoke-WebRequest -Uri $k6 -OutFile "$temp\k6.zip"
    Expand-Archive "$temp\k6.zip" -DestinationPath $temp -Force

    Get-ChildItem -Path $temp -Filter *.exe -Recurse -ErrorAction SilentlyContinue -Force `
        | Move-Item -Destination ".\k6.exe" -Force
} finally {
    Remove-Item $temp -Recurse -Force
}