function CleanFolders($Filter) {
    $progressMessage = "Cleaning $Filter folders"
    Write-Host "##teamcity[blockOpened name='$Filter']"
    Write-Host "##teamcity[progressStart '$progressMessage']"

    Get-ChildItem -Filter $Filter -Recurse | ?{ $_.Attributes -match 'Directory' } | ?{ $_.FullName -notmatch '\\.hg\\' } `
        | Remove-Item -Force -Recurse -Verbose

    Write-Host "##teamcity[progressFinish '$progressMessage']"
    Write-Host "##teamcity[blockClosed name='$Filter']"
}

function CleanBinAndObjFolders() {
    Write-Host "##teamcity[blockOpened name='Cleaning folders']"

    CleanFolders 'bin'
    CleanFolders 'obj'

    Write-Host "##teamcity[blockClosed name='Cleaning folders']"
}


function IsSetupSolution($Solution) {
    return $Solution.EndsWith('Setup.sln')
}

function ShouldSolutionBeIgnored($Solution) {
    return IsSetupSolution $Solution
}

function GetSolutionsToBuild() {
    $foundSolutions = Get-ChildItem -Filter *.sln -Recurse | %{ $_.FullName.Substring((Get-Location).Path.Length + 1) }
    $solutionsToIgnore = $foundSolutions | ?{ ShouldSolutionBeIgnored $_ }
    $solutionsToBuild = $foundSolutions | ?{ -not (ShouldSolutionBeIgnored $_) }

    if ($solutionsToIgnore.Count -gt 0) {
        Write-Host "##teamcity[message status='WARNING' text='Ignored $($solutionsToIgnore.Count) solution(s): $([string]::Join(', ', $solutionsToIgnore))']"
    }

    return $solutionsToBuild
}

function BuildSolution($Solution, $BuildConfiguration) {
    $progressMessage = "Building solution $([array]::IndexOf($solutionsToBuild, $Solution) + 1) of $($solutionsToBuild.Count) $Solution"
    Write-Host "##teamcity[blockOpened name='$Solution']"
    Write-Host "##teamcity[progressStart '$progressMessage']"

    C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe $Solution /t:Rebuild /p:Configuration=$BuildConfiguration | Write-Host

    $wasBuildSuccessfull = $LASTEXITCODE -eq 0

    if (-not $wasBuildSuccessfull) {
        Write-Host "##teamcity[message status='ERROR' text='failed to build $Solution']"
    }

    Write-Host "##teamcity[progressFinish '$progressMessage']"
    Write-Host "##teamcity[blockClosed name='$Solution']"

    return $wasBuildSuccessfull
}

function BuildSolutions($BuildConfiguration) {
    Write-Host "##teamcity[blockOpened name='Building solutions']"

    $solutionsToBuild = GetSolutionsToBuild

    $countOfFailedSolutions = 0

    foreach ($solution in $solutionsToBuild) {

        $wasBuildSuccessfull = BuildSolution $solution $BuildConfiguration

        if (-not $wasBuildSuccessfull) {
            $countOfFailedSolutions += 1
        }
    }

    if ($countOfFailedSolutions -gt 0) {
        Write-Host "##teamcity[buildStatus status='FAILURE' text='Failed to build $countOfFailedSolutions solution(s)']"
    }

    Write-Host "##teamcity[blockClosed name='Building solutions']"
}


CleanBinAndObjFolders

BuildSolutions 'Debug'
