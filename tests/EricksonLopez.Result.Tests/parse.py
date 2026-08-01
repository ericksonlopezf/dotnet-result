import json
import sys

html_file = r"D:\DevData\ericksonlopez.dev\dotnet-result\tests\EricksonLopez.Result.Tests\StrykerOutput\2026-07-31.23-07-15\reports\mutation-report.html"
with open(html_file, 'r', encoding='utf-8') as f:
    html = f.read()

start_str = "report = "
start_idx = html.find(start_str)
if start_idx == -1:
    print("Not found")
    sys.exit(1)

start_idx += len(start_str)
# find the ending semicolon but it's nested JSON. Actually, Stryker HTML has `window.mutationTestReport = { ... };` at the end of the script block.
# the script block is usually `<script> window.mutationTestReport = {...}; </script>`
# Let's find the closing `</script>` and go back a bit.
start_idx = html.find("{", start_idx)
end_idx = html.rfind("}", start_idx, html.rfind("</script>")) + 1
json_str = html[start_idx:end_idx]

try:
    data = json.loads(json_str)
except Exception as e:
    print(f"JSON Parse Error: {e}")
    sys.exit(1)

for filename, file_data in data['files'].items():
    for mutant in file_data.get('mutants', []):
        if mutant['status'] in ['Survived', 'NoCoverage']:
            print(f"{filename} | Line {mutant['location']['start']['line']} | {mutant['mutatorName']} | {mutant['status']}")
            print(f"   Replacement: {mutant['replacement']}")
