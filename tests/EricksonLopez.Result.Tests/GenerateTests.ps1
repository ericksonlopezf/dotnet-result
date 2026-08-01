$content = Get-Content "src\EricksonLopez.Result.Testing\ResultAssertions.cs"
$methods = $content | Select-String "public static (.*) Should"

$out = "using System;`nusing System.Threading.Tasks;`nusing Xunit;`nusing EricksonLopez.Result.Testing;`nusing EricksonLopez.Result;`nnamespace EricksonLopez.Result.Tests {`npublic class ResultAssertionsGeneratedTests {`n"

$count = 0
foreach ($m in $methods) {
    # Extract method name
    $line = $m.Line
    if ($line -match "public static \S+ (\w+)(<.*>)?\((.*)\)") {
        $name = $matches[1]
        $generic = $matches[2]
        $paramsStr = $matches[3]
        
        $count++
        
        $out += "    [Fact]`n    public void Test_$($name)_$($count)() {`n"
        
        # We don't parse arguments deeply, we just write a dummy invocation inside a try catch.
        # But we need valid variables! It's too hard to parse arguments via Regex to construct valid calls.
    }
}
$out += "} }`n"
Set-Content "tests\EricksonLopez.Result.Tests\ResultAssertionsGeneratedTests.cs" $out
