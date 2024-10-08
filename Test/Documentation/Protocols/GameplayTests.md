# Gameplay Tests

Copy the CustomTrees from `Test/Samples/CustomTrees` into `<GameData>`. `CustomTrees` contain the opponent folders (`Easy`, `Medium` and `Hard`) and the `index.info`

1. Easy - Warm-Up
    - [ ] 2 actions: left and right
    - [ ] 1 match with 11 rounds
    - [ ] History **disabled**
    - [ ] Final score **enabled**: X hits in Y plays (X/Y * 100 %)
    - [ ] Post-Game screen should have only the "Game Menu" button
    - [ ] Return to Game Modes menu
1. Easy - Goalkeeper
    - [ ] 2 actions: left and right
    - [ ] 1 match with 3 rounds and it should resolve in: center, right and left
    - [ ] History **enabled**
    - [ ] Final scode **disabled**
    - [ ] Post-Game screen should have only the "Game Menu" button
    - [ ] Return to Game Modes menu
1. Medium Goalkeeper
    - [ ] 3 actions: left, center and right
    - [ ] 2 matches:
        - 1st with 5 rounds
        - 2nd with 3 rounds
    - [ ] History enabled
    - [ ] Scoreboards enabled

1. Hard - Memory Game
    - [ ] 3 actions: left, center and right
    - [ ] 2 matches:
        - 1st: has at least 3 rounds, up to 21, and expects 3 succesful rounds in a row
        - 2nd: has 3 rounds should resolve in the sequence: right, left and center
    - [ ] History disabled
    - [ ] Scoreboards enabled

1. Hard - Declarative Memory:
    - [ ] Same as "Hard - Memory Game"
