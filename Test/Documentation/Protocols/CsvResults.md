# CSV Results

After executing the Gameplay Tests protocol, run the script that validates the local results files:

```shell
python results_validator.py
```

## Validation

In the `Scripts` folder, execute: 
```shell
python result_files_validator.py
```

### 1. File names convention

The file names should be in the form of `Plays_<GameMode>_<CustomTreeId>_<HostName>_<Date>_<TimeHms>_<TimeMs>.csv`, where:
- `GameMode`: AQ (Warm-up), AR (Warm-up with Timer), JG (Goalkeeper Game) or JM (Memory Game)
- `CustomTreeId`: `id` field assigned to the opponent's Custom Tree
- `HostName`: Host machine/Device name
- `Date`: Date encoded as `yyMMdd`
- `TimeHms`: Local time encoded as `HHmmss` (hours, minutes and seconds)
- `TimeMs`: Local time's milliseconds encoded as `SSS`

### 1. All fields filled and correct data type (Section 1)

Consult the `general_results_csv_schema.py` to see the expected data types in the first section of the the results file, which excludes the sub-table that registers each individual move.

### 1. All fields filled and correct data type (Section 2)

Consult the `moves_results_csv_schema.py` to see the expected data types in the first section of the the results file, which excludes the sub-table that registers each individual move.
