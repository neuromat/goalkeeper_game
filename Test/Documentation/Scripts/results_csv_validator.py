from pathlib import Path
from general_data_csv_schema_validator import validate_general_data
from moves_data_csv_schema_validator import validate_moves_data
from memory_game_data_csv_schema_validator import validate_memory_game_data
from pandas.errors import ParserError


def get_results_file_paths(relative_game_data_root_path_str: str):
    game_root_directory = Path(relative_game_data_root_path_str)
    return [
        str(i)
        for i in game_root_directory.iterdir() 
        if i.is_file() and i.suffix == '.csv'
    ]

def validate(file_paths_str: str):
    fail_emoji = "\u274C"
    success_emoji = "\u2705"
    invalid_file_paths = []
    for f in file_paths_str:
        try:
            print(f)
            is_valid, message = validate_general_data(f)
            if is_valid:
                print(f"{success_emoji} [general]")
            else:
                print(f"{fail_emoji} [general]")
                invalid_file_paths.append((f, "[general]", message))

            is_valid, message = validate_moves_data(f)
            if is_valid:
                print(f"{success_emoji} [moves]")
            else:
                print(f"{fail_emoji} [moves]")
                invalid_file_paths.append((f, "[moves]", message))
            
            if "_JM_" in f:
                is_valid, message = validate_memory_game_data(f)
                if is_valid:
                    print(f"{success_emoji} [memory]")
                else:
                    print(f"{fail_emoji} [memory]")
                    invalid_file_paths.append((f, "[memory]", message))
        except ParserError as e:
            message = f"{fail_emoji}" * 3 + f"Parser error in file {f}"
            print(message)
            raise e
    
    if len(invalid_file_paths) > 0:
        print("\n--- Summary of invalid files ---\n")
        for _0, _1, message in invalid_file_paths:
            print(message)

def main():
    build_version = "v2024-10-03_17h58"
    relative_game_data_root_path_str = f"../../../Build/{build_version}/GK-EEG_Data"
    
    file_paths_str = get_results_file_paths(relative_game_data_root_path_str)
    validate(file_paths_str)


if __name__ == "__main__":
    main()
