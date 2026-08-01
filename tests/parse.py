import re
import json

html = open('tests/EricksonLopez.Result.Tests/StrykerOutput/2026-07-31.23-02-00/reports/mutation-report.html', 'r', encoding='utf-8').read()
m = re.search(r'window\.report\s*=\s*(\{.*?\});', html, re.DOTALL)
if m:
    data = json.loads(m.group(1))
    for f in data['files']:
        for mut in data['files'][f]['mutants']:
            if mut['status'] == 'Survived':
                print(f"{f}:{mut['location']['start']['line']} ({mut['mutatorName']}) => {mut['replacement']}")
else:
    print("Could not parse json data")
