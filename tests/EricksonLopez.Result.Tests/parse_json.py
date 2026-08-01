import json
import sys

json_file = r"D:\DevData\ericksonlopez.dev\dotnet-result\tests\EricksonLopez.Result.Tests\StrykerOutput\2026-07-31.23-41-07\reports\mutation-report.json"

with open(json_file, 'r', encoding='utf-8') as f:
    data = json.load(f)

for filename, file_data in data['files'].items():
    for mutant in file_data.get('mutants', []):
        if mutant['status'] in ['Survived', 'NoCoverage']:
            print(f"{filename} | Line {mutant['location']['start']['line']} | {mutant['mutatorName']} | {mutant['status']}")
            print(f"   Replacement: {mutant['replacement']}")
