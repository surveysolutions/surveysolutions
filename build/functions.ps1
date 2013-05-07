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

function BuildSolution($Solution, $BuildConfiguration, [switch] $MultipleSolutions, $IndexOfSolution = 0, $CountOfSolutions = 1) {
    $progressMessage = if ($MultipleSolutions) { "Building solution $($IndexOfSolution + 1) of $CountOfSolutions $Solution" } else { "Building solution $Solution" }
    $blockMessage = if ($MultipleSolutions) { $Solution } else { "Building solution $Solution" }

    Write-Host "##teamcity[blockOpened name='$blockMessage']"
    Write-Host "##teamcity[progressStart '$progressMessage']"

    & (GetPathToMSBuild) $Solution '/t:Build' "/p:Configuration=$BuildConfiguration" | Write-Host

    $wasBuildSuccessfull = $LASTEXITCODE -eq 0

    if (-not $wasBuildSuccessfull) {
        Write-Host "##teamcity[message status='ERROR' text='Failed to build solution $Solution']"

        if (-not $MultipleSolutions) {
            Write-Host "##teamcity[buildStatus status='FAILURE' text='Failed to build solution $Solution']"
        }
    }

    Write-Host "##teamcity[progressFinish '$progressMessage']"
    Write-Host "##teamcity[blockClosed name='$blockMessage']"

    return $wasBuildSuccessfull
}

function BuildSolutions($BuildConfiguration) {
    Write-Host "##teamcity[blockOpened name='Building solutions']"

    $solutionsToBuild = GetSolutionsToBuild

    $failedSolutions = @()

    if ($solutionsToBuild -ne $null) {
        foreach ($solution in $solutionsToBuild) {

            $wasBuildSuccessfull = BuildSolution `
                -Solution $solution `
                -BuildConfiguration $BuildConfiguration `
                -MultipleSolutions `
                -IndexOfSolution ([array]::IndexOf($solutionsToBuild, $solution)) `
                -CountOfSolutions $solutionsToBuild.Count `

            if (-not $wasBuildSuccessfull) {
                $failedSolutions += $solution
            }
        }
    }

    $wereAllSolutionsBuiltSuccessfully = $failedSolutions.Count -eq 0
    if (-not $wereAllSolutionsBuiltSuccessfully) {
        Write-Host "##teamcity[buildStatus status='FAILURE' text='Failed to build $($failedSolutions.Count) solution(s): $($failedSolutions -join ', ')']"
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

        .\packages\Machine.Specifications.0.5.12\tools\mspec-clr4.exe --teamcity $assembly | Write-Host

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
