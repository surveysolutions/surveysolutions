. .\build\functions.ps1

Get-ChildItem -Filter *.csproj -Recurse `
    | %{ GetPathRelativeToCurrectLocation $_.FullName } `
    | %{
        $project = $_
        $incorrectReferences =  @(
            ([xml] (Get-Content $_)).Project.ItemGroup.Reference.HintPath | ?{ $_ -ne $null -and $_.ToLower().Contains('\bin\') }
        )
        return $incorrectReferences | %{ New-Object PSObject -Property @{ `
            Project = $project;
            IncorrectReference = $_;
        }}
    } `
    | Format-List `
