from pandera import DataFrameSchema, Column, Int, Float
from pandera.errors import SchemaErrors
from pandas import read_csv

moves_data_schema = DataFrameSchema({
    "move": Column(int),
    "waitedResult": Column(int),
    "ehRandom": Column(str),
    "optionChosen": Column(int),
    "correct": Column(bool),
    "movementTime": Column(float),
    "pauseTime": Column(float, coerce=True),
    "timeRunning": Column(float)
})

def get_moves_starting_line(results_file_path: str):
    starting_line = -1
    with open(results_file_path) as file:
        for i, line in enumerate(file.readlines()):
            if "move" in line:
                starting_line = i
    if starting_line > -1:
        return starting_line
    else:
        message = f"{results_file_path} does not have 'move' column"
        raise Exception(message)

def validate_moves_data(csv_file_path: str):
    starting_line = get_moves_starting_line(csv_file_path)
    skip_rows = list(range(0, starting_line))
    data_frame = read_csv(
        csv_file_path, skiprows=skip_rows,
        keep_default_na=False)
    try:
        moves_data_schema.validate(data_frame, lazy=True)
    except SchemaErrors as e:
        fail_message = f"\u274C [moves] {csv_file_path}"
        print(fail_message)
        print(e.failure_cases)
    else:
        success_message = f"\u2705 [moves] {csv_file_path}"
        print(success_message)

def main():
    build_version = "v2024-10-03_17h58"
    relative_game_data_root_path = f"../../../Build/{build_version}/GK-EEG_Data"
    relative_file_path = f"{relative_game_data_root_path}/" + \
        "Plays_AQ_Test-Easy-1_cybersys-Inspiron-5458_241008_170334_027.csv"
    validate_moves_data(relative_file_path)


if __name__ == "__main__":
    main()