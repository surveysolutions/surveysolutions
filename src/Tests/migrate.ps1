$files = Get-ChildItem -Path WB.Tests.Unit.Designer -Filter *.cs -Recurse -ErrorAction SilentlyContinue -Force
$itRegex = 'It\s+([^\s]+)\s+=\s*\(\s*\)\s*=>'
$establishRegex = 'Establish\s+([^\s]+)\s+=\s*\(\s*\)\s*=>\s+{'
$becauseRegex = 'Because\s+of\s+=\s*\(\s*\)\s*=>'

$files| ForEach-Object {
    $fileName =  $_.FullName
    
    $text = [system.io.file]::ReadAllText($fileName)
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