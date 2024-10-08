# Known Issues

Collection of known issues in the Goalkeeper Game. To get started, copy `DefectiveSamples/CustomTrees` into `<GameData>`

## 1. Success Rate in Results

The field `successRate` available in the results file seems to be incorrect when the number of rounds played is less than 10.

### 1. Reaching minimum of hits in less than 10 rounds

Using `SuccessRate1` (snippet):
```json
{
 "limitPlays":"5",
 "readSequ":"true",
 "sequ": "02020",
 "sequR":"nnnnn",
 "minHitsInSequence":"3"
}
```

If we successfully defend the 3 first attempts, thus ending the match, we expect to have a `successRate` of `1.0`. Instead, we observe `0.3`.

### 1. Reaching the attempts in less than 10 rounds

Using `SuccessRate1` (snippet):
```json
{
 "limitPlays":"5",
 "readSequ":"true",
 "sequ": "02020",
 "sequR":"nnnnn",
 "minHitsInSequence":"3"
}
```

If we defend the first and last 2 attempts, we expect a `successRate` of `0.8`. Instead, we observe `0.4`
