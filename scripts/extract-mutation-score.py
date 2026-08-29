#!/usr/bin/env python3
# Copyright © Erickson Lopez. MIT License.
import datetime
import json
import os
import sys
from pathlib import Path

# Ensure UTF-8 output on standard streams (e.g. Windows cp1252)
if sys.stdout and hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
if sys.stderr and hasattr(sys.stderr, "reconfigure"):
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")


def load_thresholds(config_path="stryker-config.json"):
    thresholds = {"high": 100, "low": 98, "break": 95}
    if os.path.exists(config_path):
        try:
            with open(config_path, "r", encoding="utf-8") as f:
                config = json.load(f)
                t = config.get("stryker-config", {}).get("thresholds") or config.get("thresholds", {})
                thresholds = {
                    "high": t.get("high", 100),
                    "low": t.get("low", 98),
                    "break": t.get("break", 95)
                }
        except Exception as e:
            print(f"Warning: Could not parse config {config_path}: {e}", file=sys.stderr)
    return thresholds


def find_json_reports(target_dir):
    reports = []
    path = Path(target_dir)
    if not path.exists():
        return reports
    for p in path.rglob("*.json"):
        name = p.name
        if not name.endswith(".html.json") and not name.endswith("metadata.json") and not name.startswith("summary-"):
            reports.append(p)
    return reports


def main():
    target_dir = sys.argv[1] if len(sys.argv) > 1 else "StrykerOutput/ci"
    if not os.path.exists(target_dir) and os.path.exists("StrykerOutput"):
        target_dir = "StrykerOutput"

    pkg_name = sys.argv[2] if len(sys.argv) > 2 else "Result"
    config_file = sys.argv[3] if len(sys.argv) > 3 else "stryker-config.json"

    thresholds = load_thresholds(config_file)
    score = 0.0
    killed = 0
    total = 0
    found_report = False

    json_files = find_json_reports(target_dir)
    if json_files:
        try:
            with open(json_files[0], "r", encoding="utf-8") as f:
                data = json.load(f)

            if "mutationScore" in data:
                score = float(data["mutationScore"])

            files = data.get("files", {})
            for file_info in files.values():
                for m in file_info.get("mutants", []):
                    st = str(m.get("status", "")).lower()
                    if st in ("killed", "timeout"):
                        killed += 1
                        total += 1
                    elif st in ("survived", "nocoverage"):
                        total += 1

            if total > 0 and "mutationScore" not in data:
                score = round((killed / total) * 100.0, 2)
            found_report = True
        except Exception as e:
            print(f"Warning: Error parsing {json_files[0]}: {e}", file=sys.stderr)

    passed_gate = (score >= thresholds["break"]) and found_report

    if score >= thresholds["high"]:
        status_label = "✅ HIGH"
    elif score >= thresholds["low"]:
        status_label = "🟡 LOW"
    elif score >= thresholds["break"]:
        status_label = "🟠 WARNING"
    else:
        status_label = "❌ FAILED"

    sha = os.environ.get("GITHUB_SHA", "unknown")
    repo = os.environ.get("GITHUB_REPOSITORY", "")
    run_id = os.environ.get("GITHUB_RUN_ID", "")
    server_url = os.environ.get("GITHUB_SERVER_URL", "https://github.com")
    run_url = f"{server_url}/{repo}/actions/runs/{run_id}" if repo and run_id else ""
    execution_date_iso = datetime.datetime.now(datetime.timezone.utc).isoformat()

    metadata = {
        "package": pkg_name,
        "commit_sha": sha,
        "execution_date": execution_date_iso,
        "mutation_score": score,
        "mutants_killed": killed,
        "total_mutants": total,
        "threshold_high": thresholds["high"],
        "threshold_low": thresholds["low"],
        "threshold_break": thresholds["break"],
        "status": status_label,
        "passed_break": passed_gate,
        "run_url": run_url
    }

    os.makedirs("StrykerOutput", exist_ok=True)
    summary_path = os.path.join("StrykerOutput", f"summary-{pkg_name}.json")
    with open(summary_path, "w", encoding="utf-8") as f:
        json.dump(metadata, f, indent=2)

    step_summary_path = os.environ.get("GITHUB_STEP_SUMMARY")
    if step_summary_path and os.path.exists(os.path.dirname(step_summary_path)):
        summary_md = f"""
## 🛡️ Stryker Mutation Testing Results — {pkg_name}

| Metric | Value |
|--------|-------|
| **Mutation Score** | **{score}%** |
| **Mutants Killed** | {killed} |
| **Total Mutants** | ${total} |
| **Threshold High** | ≥{thresholds['high']}% |
| **Threshold Low** | ≥{thresholds['low']}% |
| **Threshold Break** | ≥{thresholds['break']}% |
| **Status** | {status_label} |
| **Commit SHA** | `{sha[:7]}` (`{sha}`) |
| **Execution Date** | {execution_date_iso} |
"""
        with open(step_summary_path, "a", encoding="utf-8") as f:
            f.write(summary_md)

    github_output = os.environ.get("GITHUB_OUTPUT")
    if github_output and os.path.exists(os.path.dirname(github_output)):
        with open(github_output, "a", encoding="utf-8") as f:
            f.write(f"score={score}\n")
            f.write(f"passed_gate={'true' if passed_gate else 'false'}\n")
            f.write(f"status={status_label}\n")
            f.write(f"killed={killed}\n")
            f.write(f"total={total}\n")

    print(f"[{pkg_name}] Stryker Score: {score}% ({killed}/{total}) - {status_label} (Passed: {passed_gate})")


if __name__ == "__main__":
    main()
