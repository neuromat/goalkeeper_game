from pandera import DataFrameSchema, Column
from pandera.errors import SchemaErrors
from pandas import read_csv, DataFrame

schema = DataFrameSchema({
    "currentLanguage": Column(str),
    "operatingSystem": Column(str),
    "ipAddress": Column(str),
    "ipCountry": Column(str),
    "gameVersion": Column(str),
    "gameLanguage": Column(str),
    "institution": Column(str),
    "soccerTeam": Column(str),
    "game": Column(str),
    "playID": Column(str),
    "phase": Column(int),
    "choices": Column(int),
    "showPlayPauseButton": Column(str), # True/False
    "pausePlayInputKey": Column(str),   # None if empty
    "sessionTime": Column(float),
    "relaxTime": Column(float),
    "initialPauseTime": Column(float),
    "numOtherPauses": Column(int),
    "otherPausesTime": Column(float),
    "attentionPoint": Column(str),
    "attentionDiameter": Column(float),
    "attentionColorStart": Column(str),
    "attentionColorCorrect": Column(str),
    "attentionColorWrong": Column(str),
    "playerMachine": Column(str),
    "gameDate": Column(int),
    "gameTime": Column(int),
    "gameRandom": Column(int),
    "playerAlias": Column(str),
    "limitPlays": Column(int),
    "totalCorrect": Column(int),
    "successRate": Column(float),
    "gameMode": Column(str),
    "status": Column(str),
    "playsToRelax": Column(int),
    "scoreboard": Column(str), # True/False
    "finalScoreboard": Column(str),
    "animationType": Column(str),
    "showHistory": Column(str), # True/False
    "sendMarkersToEEG": Column(str),
    "portEEGserial": Column(str),
    "groupCode": Column(str),
    "leftInputKey": Column(str),
    "centerInputKey": Column(str),
    "rightInputKey": Column(str),
    "speedGKAnim": Column(float),
    "portSendData": Column(str),
    "timeFaixa0": Column(float),
    "timeFaixa1": Column(float),
    "timeFaixa2": Column(float),
    "timeFaixa3": Column(float),
    "timeFaixa4": Column(float),
    "keyboardMarker1": Column(float), # [TODO] check the datatype in the source
    "keyboardMarker2": Column(float),
    "keyboardMarker3": Column(float),
    "keyboardMarker4": Column(float),
    "keyboardMarker5": Column(float),
    "keyboardMarker6": Column(float),
    "keyboardMarker7": Column(float),
    "keyboardMarker8": Column(float),
    "keyboardMarker9": Column(float),
    "keyboardMarker0": Column(float),
    "minHits": Column(int),
    "minHitsInSequence": Column(int),
    "maxPlays": Column(int),
    "sequExecuted": Column(int)
})

def parse_data_in_column(data, i):
    # since the DataFrame has to be transposed,
    #   all data is type of "<class 'str'>"

    # when opening the CSV with a spreadsheet
    #   software, lines start in '1', making
    #   it easier to manually verify
    i += 1
    int_fields_indexes = [
        11, 12, 18, 26, 27, 28, 30, 31,
        35, 63, 64, 65, 66
    ]
    float_fields_indexes = [
        15, 16, 17, 19, 21, 32, 46, 48,
        49, 50, 51, 52, 53, 54, 55, 56,
        57, 58, 59, 60, 61, 62
    ]
    try:
        if i in int_fields_indexes:
            return int(data)
        if i in float_fields_indexes:
            return float(data)
    except ValueError:
        # keep as str if unable to parse.
        #   the schema validator will take
        #   care of the validation
        pass
    return str(data)

def validate_csv(csv_file_path: str):
    """
    Raises:
        SchemaError: if the file's content doesn't match the
            schema defined
    """
    raw_data = read_csv(
        csv_file_path, nrows=66, header=None, keep_default_na=False).transpose()
    parsed_data = [
        parse_data_in_column(data, i)
        for i, data in enumerate(raw_data.to_numpy()[1])
    ]
    data_frame = DataFrame([parsed_data], columns=raw_data.to_numpy()[0])
    try:
        schema.validate(data_frame, lazy=True)
    except SchemaErrors as e:
        fail_message = f"\u274C {csv_file_path}"
        print(fail_message)
        print(e.failure_cases)
    else:
        success_message = f"\u2705 {csv_file_path}"
        print(success_message)

if __name__ == "__main__":
    build_version = "v2024-10-03_17h58"
    relative_game_data_root_path = f"../../../Build/{build_version}/GK-EEG_Data"
    relative_file_path = f"{relative_game_data_root_path}/" + \
        "Plays_AQ_Test-Easy-1_cybersys-Inspiron-5458_241008_170334_027.csv"
    validate_csv(relative_file_path)
