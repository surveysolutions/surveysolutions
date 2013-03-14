function GetPathRelativeToCurrectLocation($FullPath) {
    return $FullPath.Substring((Get-Location).Path.Length + 1)
}

function GetPathToMSBuild() {
    return 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe'
}


function CleanFolders($Filter) {
    $progressMessage = "Cleaning $Filter folders"
    Write-Host "##teamcity[blockOpened name='$Filter']"
    Write-Host "##teamcity[progressStart '$progressMessage']"

    $folders = Get-ChildItem -Filter $Filter -Recurse | ?{ $_.Attributes -match 'Directory' } | ?{ $_.FullName -notmatch '\\.hg\\' } | %{ GetPathRelativeToCurrectLocation $_.FullName }

    if ($folders -ne $null) {
        foreach ($folder in $folders) {
            Write-Host $folder
            Remove-Item $folder -Force -Recurse
        }
    }

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
    $foundSolutions = Get-ChildItem -Filter *.sln -Recurse | %{ GetPathRelativeToCurrectLocation $_.FullName }
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

    & (GetPathToMSBuild) $Solution '/t:Build' "/p:Configuration=$BuildConfiguration" | Write-Host

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

    if ($solutionsToBuild -ne $null) {
        foreach ($solution in $solutionsToBuild) {

            $wasBuildSuccessfull = BuildSolution $solution $BuildConfiguration

            if (-not $wasBuildSuccessfull) {
                $countOfFailedSolutions += 1
            }
        }
    }

    $wereAllSolutionsBuiltSuccessfully = $countOfFailedSolutions -eq 0
    if (-not $wereAllSolutionsBuiltSuccessfully) {
        Write-Host "##teamcity[buildStatus status='FAILURE' text='Failed to build $countOfFailedSolutions solution(s)']"
    }

    Write-Host "##teamcity[blockClosed name='Building solutions']"

    return $wereAllSolutionsBuiltSuccessfully
}


function GetProjectsWithTests() {
    return Get-ChildItem -Filter *Test*.csproj -Recurse | %{ GetPathRelativeToCurrectLocation $_.FullName }
}

function GetOutputAssembly($Project, $BuildConfiguration) {
    $projectFileInfo = Get-Item $Project
    $fullPathToAssembly = "$($projectFileInfo.DirectoryName)\bin\$BuildConfiguration\$($projectFileInfo.BaseName).dll"

    return GetPathRelativeToCurrectLocation $fullPathToAssembly
}

function RunTestsFromProject($Project, $BuildConfiguration) {
    Write-Host "##teamcity[blockOpened name='$Project']"

    $assembly = GetOutputAssembly $Project $BuildConfiguration

    if (-not (Test-Path $assembly)) {

        Write-Host "##teamcity[message status='WARNING' text='Expected tests assembly $assembly is missing']"

    } else {

        Write-Host "##teamcity[progressStart 'Running tests from $assembly']"

        $resultXml = (Get-Item $assembly).BaseName + '.NUnit-Result.xml'
        .\packages\NUnit.Runners.2.6.2\tools\nunit-console.exe $assembly /result=$resultXml /nologo /nodots | Write-Host
        Write-Host "##teamcity[importData type='nunit' path='$resultXml']"

        Write-Host "##teamcity[progressFinish 'Running tests from $assembly']"
    }

    Write-Host "##teamcity[blockClosed name='$Project']"
}

function RunTests($BuildConfiguration) {
    Write-Host "##teamcity[blockOpened name='Running tests']"

    $projects = GetProjectsWithTests

    if ($projects -ne $null) {
        foreach ($project in $projects) {
            RunTestsFromProject $project $BuildConfiguration
        }
    }

    Write-Host "##teamcity[blockClosed name='Running tests']"
}
