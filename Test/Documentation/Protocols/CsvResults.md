# CSV Results

## Validation Types

The validation process breaks down the results files into 3 parts: general data, moves data and memory game data. The scripts confronts the files with the expected schema.

### General data validation

All game modes include the majority of its information as "general" data. There are some small differences among the game modes data, so look into the `general_data_csv_schema_validator.py` for more details.

### Moves data validation

There is a section in the results file dedicated to register each round of the game. It is a n x 8 table that takes the last `n` lines of the file. It's indicated by the `move` column.

### Memory game data validation (only for Memory Game Mode)

There is a section in the results file dedicated to register some data exclusive related to the Memory Game Mode. It is a m x 3 table that takes the `m` lines between the `try` column position to the `move` data.

## Before you start

You need to make changes to the script in order to successfully run the validation scripts.

In the `results_csv_validator.py`, make sure the variable `relative_game_data_root_path_str` points to the `<GameData>` folder. By convention is the game name defined in build process with `_Data` suffix.

Also, make sure that all the results `.csv` files follow the default naming convention `Plays_<GameMode>_<CustomTreeId>_<HostName>_<Date>_<TimeHms>_<TimeMs>.csv`, where:
- `GameMode`: AQ (Warm-up), AR (Warm-up with Timer), JG (Goalkeeper Game) or JM (Memory Game)
- `CustomTreeId`: `id` field assigned to the opponent's Custom Tree
- `HostName`: Host machine/Device name
- `Date`: Date encoded as `yyMMdd`
- `TimeHms`: Local time encoded as `HHmmss` (hours, minutes and seconds)
- `TimeMs`: Local time's milliseconds encoded as `SSS`

## Running the validator

After executing the Gameplay Tests protocol, run the script that validates the local results files:

```shell
python results_csv_validator.py
```

Each line it outputs has a ✅/❌ emoji, followed by the type of validation `[general|moves|memory]` and file that was validated.

If the validation fails, it will indicate which field has an error and what data type is expected. Refer to [Pandera](https://pandera.readthedocs.io/en/stable/index.html) for more information.

Example of output:
```shell
❌ [general] ../../../Build/v2024-10-03_17h58/GK-EEG_Data/Plays_JM_Test-Hard-1_cybersys-Inspiron-5458_241008_170650_115.csv
  schema_context    column           check check_number failure_case index
0         Column  maxPlays  dtype('int64')         None       object  None
✅ [moves] ../../../Build/v2024-10-03_17h58/GK-EEG_Data/Plays_JM_Test-Hard-1_cybersys-Inspiron-5458_241008_170650_115.csv
❌ [memory] ../../../Build/v2024-10-03_17h58/GK-EEG_Data/Plays_JM_Test-Hard-1_cybersys-Inspiron-5458_241008_170650_115.csv
  schema_context column           check check_number failure_case index
0         Column    try  dtype('int64')         None       object  None
```

## Running the validator against samples

If you wish to run the validator against provided samples, copy the files from `Samples/Results` into the `<GameData>` folder.
