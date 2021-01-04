param(
    [ValidateSet("win-x64","linux-x64", "mac-os")]
    $r = "win-x64"
)

try{
    Push-Location ./src/Services/Export/WB.Services.Export.Host

    dotnet publish -c Release -r $r --output ../../../UI/WB.UI.Headquarters.Core/Export.Service `
        --version-suffix dev-build /p:BuildNumber=42 /p:PublishSingleFile=false /p:Selfcontained=false
} finally {
    Pop-Location
}

try {
    Push-Location ./src/UI/WB.UI.Headquarters.Core

    dotnet publish -c Release -r $r --output dist --version-suffix dev-build `
             -p:PublishSingleFile=true `
            /p:PublishTrimmed=False /p:BuildNumber=42  /p:Selfcontained=false
} finally{
    Pop-Location
}

