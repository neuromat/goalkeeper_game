from pathlib import Path
from general_data_csv_schema_validator import validate_general_data
from moves_data_csv_schema_validator import validate_moves_data


def get_results_file_paths(relative_game_data_root_path_str: str):
    game_root_directory = Path(relative_game_data_root_path_str)
    return [
        str(i)
        for i in game_root_directory.iterdir() 
        if i.is_file() and i.suffix == '.csv'
    ]

def validate(file_paths_str: str):
    for f in file_paths_str:
        validate_general_data(f)
        validate_moves_data(f)

def main():
    build_version = "v2024-10-03_17h58"
    relative_game_data_root_path_str = f"../../../Build/{build_version}/GK-EEG_Data"
    
    file_paths_str = get_results_file_paths(relative_game_data_root_path_str)
    validate(file_paths_str)


if __name__ == "__main__":
    main()
