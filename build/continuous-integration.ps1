function IsSetupSolution($solution) {
    return $solution.EndsWith('Setup.sln')
}

Write-Host "##teamcity[blockOpened name='Building solutions']"

$foundSolutions = Get-ChildItem -Filter *.sln -Recurse | %{ $_.FullName.Substring((Get-Location).Path.Length + 1) }

$ignoredSolutions = $foundSolutions | ?{ IsSetupSolution($_) }

if ($ignoredSolutions.Count -gt 0) {
    Write-Host "##teamcity[message status='WARNING' text='Ignored $($ignoredSolutions.Count) solution(s): $([string]::Join(', ', $ignoredSolutions))']"
}

$solutionsToBuild = $foundSolutions | ?{ -not (IsSetupSolution($_)) }

$countOfFailedSolutions = 0

foreach ($solution in $solutionsToBuild) {
    $progressMessage = "Building solution $([array]::IndexOf($solutionsToBuild, $solution) + 1) of $($solutionsToBuild.Count) $solution"

    Write-Host "##teamcity[blockOpened name='$solution']"
    Write-Host "##teamcity[progressStart '$progressMessage']"

    C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe $solution /t:Rebuild /p:Configuration=Debug

    if ($LASTEXITCODE -ne 0) {
        Write-Host "##teamcity[message status='ERROR' text='failed to build $solution']"
        $countOfFailedSolutions += 1
    }

    Write-Host "##teamcity[progressFinish '$progressMessage']"
    Write-Host "##teamcity[blockClosed name='$solution']"
}

if ($countOfFailedSolutions -gt 0) {
    Write-Host "##teamcity[buildStatus status='FAILURE' text='Failed to build $countOfFailedSolutions solution(s)']"
}

Write-Host "##teamcity[blockClosed name='Building solutions']"
