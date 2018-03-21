$files = Get-ChildItem -Path E:\surveysolutions\src\Tests\WB.Tests.Unit\BoundedContexts -Recurse -Include *.cs -Force
$itRegex = 'It\s+([^\s]+)\s+=\s*\(\s*\)\s*=>'
$shouldEqualRegex = '\.ShouldEqual\('
$establishRegex = 'Establish\s+([^\s]+)\s+=\s*\(\s*\)\s*=>\s+{'
$becauseRegex = 'Because\s+of\s+=\s*\(\s*\)\s*=>'

$files| ForEach-Object {
    $fileName =  $_.FullName
    
    $text = [system.io.file]::ReadAllText($fileName) -replace $shouldEqualRegex, ".Should().Be(" -replace "using Machine\.Specifications;", "using FluentAssertions;" -replace "Cleanup stuff = \(\) =>", "[NUnit.Framework.OneTimeTearDown] public void CleanUp()"
    $text = $text -replace "\.ShouldBeFalse\(", ".Should().BeFalse(" -replace "\.ShouldBeTrue\(", ".Should().BeTrue(" -replace "Because of = \(\) =>", "public void BecauseOf() =>" -replace "\)\.ShouldContain\(" ").Should().Contain("
    $text = $text -replace "using It = Machine.Specifications\.It;" ""

    
    if ($text) {
        [regex]::Matches($text, $itRegex) | %{ 
            $value = $_.Groups[0].Value
            if ($value) {
                $methodName = $_.Groups[1].Value
                $text = $text -replace [regex]::escape($value), "[NUnit.Framework.Test] public void $methodName () =>" 
                
                [system.io.file]::WriteAllText($fileName, $text)
            }
        }

        $establishMatch = [regex]::Match($text, $establishRegex)

        if(-not [string]::IsNullOrEmpty($establishMatch.Groups[0])) {
            $fullString = $establishMatch.Groups[0].Value
            $methodName = $establishMatch.Groups[1].Value
            $text = $text -replace [regex]::escape($fullString), "[NUnit.Framework.OneTimeSetUp] public void $methodName () {" 
            $text = $text.Replace("    };", "    }")
            
            $text = $text -replace $becauseRegex, "private void BecauseOf() =>" 

            [system.io.file]::WriteAllText($fileName, $text)
        }
    }
    #$text -replace "XXX",$value | Set-Content -path $_
}