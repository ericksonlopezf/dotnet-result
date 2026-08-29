// Copyright © Erickson Lopez. MIT License.
const assert = require('assert');
const {
  loadThresholds,
  parseScoreFromDescription,
  evaluateScore,
  verifyMutationGate,
  MAX_REPORT_AGE_DAYS
} = require('./verify-mutation-gate');

console.log('Running tests for verify-mutation-gate.js...\n');

// Test 1: loadThresholds from stryker-config.json
{
  const thresholds = loadThresholds();
  assert.strictEqual(thresholds.high, 100, 'Threshold high should be 100');
  assert.strictEqual(thresholds.low, 98, 'Threshold low should be 98');
  assert.strictEqual(thresholds.break, 95, 'Threshold break should be 95');
  console.log('✅ Test 1 Passed: loadThresholds loads correct values from stryker-config.json');
}

// Test 2: parseScoreFromDescription
{
  assert.strictEqual(parseScoreFromDescription('Stryker: 100% (240/240 killed) - ✅ HIGH'), 100);
  assert.strictEqual(parseScoreFromDescription('Stryker: 98.5% (200/203 killed) - 🟡 LOW'), 98.5);
  assert.strictEqual(parseScoreFromDescription('Stryker: 95.0% - 🟠 WARNING'), 95.0);
  assert.strictEqual(parseScoreFromDescription('Stryker: 94.2% - ❌ FAILED'), 94.2);
  assert.strictEqual(parseScoreFromDescription(null), null);
  assert.strictEqual(parseScoreFromDescription('No percentage here'), null);
  console.log('✅ Test 2 Passed: parseScoreFromDescription correctly extracts numeric percentage');
}

// Test 3: evaluateScore
{
  const thresholds = { high: 100, low: 98, break: 95 };

  const resHigh = evaluateScore(100, thresholds);
  assert.strictEqual(resHigh.status, '✅ HIGH');
  assert.strictEqual(resHigh.passedBreak, true);

  const resLow = evaluateScore(98.5, thresholds);
  assert.strictEqual(resLow.status, '🟡 LOW');
  assert.strictEqual(resLow.passedBreak, true);

  const resWarn = evaluateScore(96.0, thresholds);
  assert.strictEqual(resWarn.status, '🟠 WARNING');
  assert.strictEqual(resWarn.passedBreak, true);

  const resBreakExact = evaluateScore(95.0, thresholds);
  assert.strictEqual(resBreakExact.status, '🟠 WARNING');
  assert.strictEqual(resBreakExact.passedBreak, true);

  const resFail = evaluateScore(94.9, thresholds);
  assert.strictEqual(resFail.status, '❌ FAILED');
  assert.strictEqual(resFail.passedBreak, false);

  console.log('✅ Test 3 Passed: evaluateScore correctly categorizes scores and break gate');
}

// Test 4: verifyMutationGate with mock direct target SHA (Reusing valid evidence)
(async () => {
  const mockContext = {
    repo: { owner: 'ericksonlopezf', repo: 'dotnet-result' },
    sha: 'abc1234567890'
  };

  const freshDate = new Date().toISOString();

  const mockGithub = {
    rest: {
      repos: {
        listCommits: async () => ({
          data: [{ sha: 'abc1234567890' }]
        }),
        getCombinedStatusForRef: async ({ ref }) => {
          if (ref === 'abc1234567890') {
            return {
              data: {
                statuses: [
                  {
                    context: 'mutation-testing/stryker',
                    state: 'success',
                    description: 'Score: 100.0% (14/14 packages >= 95%) - ✅ HIGH',
                    updated_at: freshDate,
                    target_url: 'https://github.com/ericksonlopezf/dotnet-result/actions/runs/12345'
                  }
                ]
              }
            };
          }
          return { data: { statuses: [] } };
        }
      }
    }
  };

  const outputs = {};
  const mockCore = {
    setOutput: (k, v) => { outputs[k] = v; }
  };

  const res = await verifyMutationGate({ github: mockGithub, context: mockContext, core: mockCore });
  assert.strictEqual(res.needsStryker, false, 'Should not need Stryker execution when valid 100% evidence exists');
  assert.strictEqual(res.canProceed, true, 'Should allow publication without re-running Stryker');
  assert.strictEqual(outputs.needs_stryker, 'false');
  assert.strictEqual(outputs.can_proceed, 'true');
  console.log('✅ Test 4 Passed: verifyMutationGate reuses valid fresh Stryker evidence on main');
})();

// Test 5: verifyMutationGate with score below break threshold (triggers conditional execution)
(async () => {
  const mockContext = {
    repo: { owner: 'ericksonlopezf', repo: 'dotnet-result' },
    sha: 'fail1234567890'
  };

  const freshDate = new Date().toISOString();

  const mockGithub = {
    rest: {
      repos: {
        listCommits: async () => ({
          data: [{ sha: 'fail1234567890' }]
        }),
        getCombinedStatusForRef: async () => {
          return {
            data: {
              statuses: [
                {
                  context: 'mutation-testing/stryker',
                  state: 'failure',
                  description: 'Score: 80.0% (10/14 packages >= 95%) - ❌ FAILED',
                  updated_at: freshDate,
                  target_url: 'https://github.com/ericksonlopezf/dotnet-result/actions/runs/12346'
                }
              ]
            }
          };
        }
      }
    }
  };

  const outputs = {};
  const mockCore = {
    setOutput: (k, v) => { outputs[k] = v; }
  };

  const res = await verifyMutationGate({ github: mockGithub, context: mockContext, core: mockCore });
  assert.strictEqual(res.needsStryker, true, 'Should trigger Stryker when previous run failed');
  assert.strictEqual(res.canProceed, false);
  assert.strictEqual(outputs.needs_stryker, 'true');
  assert.strictEqual(outputs.can_proceed, 'false');
  console.log('✅ Test 5 Passed: verifyMutationGate triggers conditional run when previous score was sub-break');
})();

// Test 6: verifyMutationGate with score 95.0% (WARNING threshold - release allowed)
(async () => {
  const mockContext = {
    repo: { owner: 'ericksonlopezf', repo: 'dotnet-result' },
    sha: 'warn1234567890'
  };

  const freshDate = new Date().toISOString();

  const mockGithub = {
    rest: {
      repos: {
        listCommits: async () => ({
          data: [{ sha: 'warn1234567890' }]
        }),
        getCombinedStatusForRef: async ({ ref }) => {
          if (ref === 'warn1234567890') {
            return {
              data: {
                statuses: [
                  {
                    context: 'mutation-testing/stryker',
                    state: 'success',
                    description: 'Score: 95.0% (14/14 packages >= 95%) - 🟠 WARNING',
                    updated_at: freshDate,
                    target_url: 'https://github.com/ericksonlopezf/dotnet-result/actions/runs/12347'
                  }
                ]
              }
            };
          }
          return { data: { statuses: [] } };
        }
      }
    }
  };

  const outputs = {};
  const mockCore = {
    setOutput: (k, v) => { outputs[k] = v; }
  };

  const res = await verifyMutationGate({ github: mockGithub, context: mockContext, core: mockCore });
  assert.strictEqual(res.needsStryker, false, 'Should allow release for 95.0% WARNING score without re-running');
  assert.strictEqual(res.canProceed, true);
  console.log('✅ Test 6 Passed: verifyMutationGate allows release for 95.0% WARNING score');
})();

// Test 7: verifyMutationGate with expired report (> 7 days - triggers conditional execution)
(async () => {
  const mockContext = {
    repo: { owner: 'ericksonlopezf', repo: 'dotnet-result' },
    sha: 'exp1234567890'
  };

  // 10 days ago
  const oldDate = new Date(Date.now() - 10 * 24 * 60 * 60 * 1000).toISOString();

  const mockGithub = {
    rest: {
      repos: {
        listCommits: async () => ({
          data: [{ sha: 'exp1234567890' }]
        }),
        getCombinedStatusForRef: async () => {
          return {
            data: {
              statuses: [
                {
                  context: 'mutation-testing/stryker',
                  state: 'success',
                  description: 'Score: 100% - ✅ HIGH',
                  updated_at: oldDate,
                  target_url: 'https://github.com/ericksonlopezf/dotnet-result/actions/runs/12349'
                }
              ]
            }
          };
        }
      }
    }
  };

  const outputs = {};
  const mockCore = {
    setOutput: (k, v) => { outputs[k] = v; }
  };

  const res = await verifyMutationGate({ github: mockGithub, context: mockContext, core: mockCore });
  assert.strictEqual(res.needsStryker, true, 'Should trigger Stryker when report is expired (> 7 days)');
  assert.strictEqual(res.canProceed, false);
  console.log('✅ Test 7 Passed: verifyMutationGate triggers conditional run for expired report (> 7 days)');
})();

// Test 8: verifyMutationGate with code drift in src/ (triggers conditional execution)
(async () => {
  const mockContext = {
    repo: { owner: 'ericksonlopezf', repo: 'dotnet-result' },
    sha: 'newSha123456789'
  };

  const freshDate = new Date().toISOString();

  const mockGithub = {
    rest: {
      repos: {
        listCommits: async () => {
          return {
            data: [
              { sha: 'newSha123456789' },
              { sha: 'oldEvaluatedCommit123' }
            ]
          };
        },
        getCombinedStatusForRef: async ({ ref }) => {
          if (ref === 'oldEvaluatedCommit123') {
            return {
              data: {
                statuses: [
                  {
                    context: 'mutation-testing/stryker',
                    state: 'success',
                    description: 'Score: 100% - ✅ HIGH',
                    updated_at: freshDate,
                    target_url: 'https://github.com/ericksonlopezf/dotnet-result/actions/runs/12350'
                  }
                ]
              }
            };
          }
          return { data: { statuses: [] } };
        },
        compareCommits: async () => {
          return {
            data: {
              files: [
                { filename: 'src/EricksonLopez.Result/Result.cs' },
                { filename: 'README.md' }
              ]
            }
          };
        }
      }
    }
  };

  const outputs = {};
  const mockCore = {
    setOutput: (k, v) => { outputs[k] = v; }
  };

  const res = await verifyMutationGate({ github: mockGithub, context: mockContext, core: mockCore });
  assert.strictEqual(res.needsStryker, true, 'Should trigger Stryker when src/ code drift is detected');
  assert.strictEqual(res.canProceed, false);
  console.log('✅ Test 8 Passed: verifyMutationGate triggers conditional run when src/ was modified');
})();
