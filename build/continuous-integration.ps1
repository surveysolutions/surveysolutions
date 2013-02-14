function IsSetupSolution($solution) {
    return $solution.EndsWith('Setup.sln')
}

function ShouldSolutionBeIgnored($solution) {
    return IsSetupSolution $solution
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

function BuildSolution($solution, $buildConfiguration) {
    $progressMessage = "Building solution $([array]::IndexOf($solutionsToBuild, $solution) + 1) of $($solutionsToBuild.Count) $solution"
    Write-Host "##teamcity[blockOpened name='$solution']"
    Write-Host "##teamcity[progressStart '$progressMessage']"

    C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe $solution /t:Rebuild /p:Configuration=$buildConfiguration

    $wasBuildSuccessfull = $LASTEXITCODE -eq 0

    if (-not $wasBuildSuccessfull) {
        Write-Host "##teamcity[message status='ERROR' text='failed to build $solution']"
    }

    Write-Host "##teamcity[progressFinish '$progressMessage']"
    Write-Host "##teamcity[blockClosed name='$solution']"

    return $wasBuildSuccessfull
}

function BuildSolutions($buildConfiguration) {
    Write-Host "##teamcity[blockOpened name='Building solutions']"

    $solutionsToBuild = GetSolutionsToBuild

    $countOfFailedSolutions = 0

    foreach ($solution in $solutionsToBuild) {

        $wasBuildSuccessfull = BuildSolution $solution $buildConfiguration

        if (-not $wasBuildSuccessfull) {
            $countOfFailedSolutions += 1
        }
    }

    if ($countOfFailedSolutions -gt 0) {
        Write-Host "##teamcity[buildStatus status='FAILURE' text='Failed to build $countOfFailedSolutions solution(s)']"
    }

    Write-Host "##teamcity[blockClosed name='Building solutions']"
}

BuildSolutions 'Debug'
