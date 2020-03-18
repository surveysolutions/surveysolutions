$projects = @(
    @{ Path = "src\Services\Export\WB.Services.Export.Host"; Profile = "Export.Host"; Url="applicationUrl" }
    @{ Path = "src\UI\WB.UI.Designer"; Profile = "WB.UI.Designer" ; Url="launchUrl"}
    @{ Path = "src\UI\WB.UI.Headquarters.Core"; Profile = "WB.UI.Headquarters.Core" ; Url="launchUrl"}
    @{ Path = "src\UI\WB.UI.WebTester"; Profile = "WB.UI.WebTester" ; Url="launchUrl"}
)


Push-Location src\UI\WB.UI.Frontend
try {
   #$& yarn
   #& yarn dev
} finally {
    Pop-Location
}

Get-Job | Remove-Job -Force

$projects | % {
    $path = "$($_.Path)\Properties\launchSettings.json"
    $settings = gc $path | ConvertFrom-Json
    $url = $settings.profiles `
        | Select -ExpandProperty $_.Profile `
        | Select -ExpandProperty $_.Url 

    Write-Host "App: $($_.Profile) Url: $url"

    Start-Job -ArgumentList $_.Profile,$_.Path  -ScriptBlock {
        param($profile, $path)
        "Starting $profile in $path"
        Set-Location $using:PWD/$path
        
        # & dotnet run --launch-profile $profile
        Start-Process -FilePath dotnet -ArgumentList 'run','--launch-profile',$profile -Wait
    } | out-null
}

Get-Job | Receive-Job

# https://stackoverflow.com/a/28237896/41483
function pause ($message)
{
    # Check if running Powershell ISE
    if ($psISE)
    {
        Add-Type -AssemblyName System.Windows.Forms
        [System.Windows.Forms.MessageBox]::Show("$message")
    }
    else
    {
        Write-Host "$message" -ForegroundColor Yellow
        $x = $host.ui.RawUI.ReadKey("NoEcho,IncludeKeyDown")
    }
}

try {
    pause "Press any key to exit"
    Get-Job | Remove-Job -Force
} finally {
   taskkill /im:dotnet.exe /f
}
