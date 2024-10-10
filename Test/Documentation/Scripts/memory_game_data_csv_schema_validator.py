from pandera import DataFrameSchema, Column, Int, Float
from pandera.errors import SchemaErrors
from pandas import read_csv

memory_game_data_schema = DataFrameSchema({
    "try": Column(int),
    "timeUntilAnyKey": Column(float, coerce=True),
    "timeUntilShowAgain": Column(float, coerce=True)
})

def get_lines_range(results_file_path: str):
    starting_line = -1
    ending_line = -1
    total_lines = 0
    with open(results_file_path) as file:
        for i, line in enumerate(file.readlines()):
            if "try," in line:
                starting_line = i
            elif "move," in line:
                ending_line = i
            total_lines += 1
    
    error_messages = []
    if starting_line == -1:
        error_messages.append(f"{results_file_path} does not have 'try' column")
    if ending_line == -1:
        error_messages.append(f"{results_file_path} does not have 'move' column")
    if len(error_messages) > 0:
        raise Exception(error_messages)
    
    return starting_line, ending_line, total_lines

def validate_memory_game_data(csv_file_path: str):
    starting_line, ending_line, total_lines = \
        get_lines_range(csv_file_path)
    skip_rows = list(range(0, starting_line)) + list(range(ending_line, total_lines))
    data_frame = read_csv(
        csv_file_path, skiprows=skip_rows,
        keep_default_na=False)
    try:
        memory_game_data_schema.validate(data_frame, lazy=True)
    except SchemaErrors as e:
        message = f"\u274C [memory] {csv_file_path}"
        is_valid = False
    else:
        message = f"\u2705 [memory] {csv_file_path}"
        is_valid = True
    return is_valid, message

def main():
    build_version = "v2024-10-03_17h58"
    relative_game_data_root_path = f"../../../Build/{build_version}/GK-EEG_Data"
    relative_file_path = f"{relative_game_data_root_path}/" + \
        "Plays_JM_Test-Hard-1_cybersys-Inspiron-5458_241008_170650_115.csv"
    validate_memory_game_data(relative_file_path)


if __name__ == "__main__":
    main()