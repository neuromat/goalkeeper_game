# Gameplay Tests

Copy the CustomTrees from `Test/Samples/CustomTrees` into `<GameData>`. `CustomTrees` contain the opponent folders (`Easy`, `Medium` and `Hard`) and the `index.info`

1. Easy - Warm-Up
    - [ ] 2 actions: left and right
    - [ ] History **disabled**
    - [ ] Scoreboard **enabled**
    - [ ] Final scordeboard **enabled**
    - [ ] 1 match with 2 rounds; resolves in "left" and "right" (`bmSequ`)
    - [ ] Maximum of 4 plays (`bmMaxPlays`)

1. Easy - Goalkeeper
    - [ ] 2 actions: left and right
    - [ ] History **enabled**
    - [ ] Scoreboard **enabled**
    - [ ] Final scordeboard **disabled**
    - [ ] 1 match with 3 rounds: first kick is random, and the others are deterministic (`state`)

1. Medium - Goalkeeper
    - [ ] 3 actions: left, center and right
    - [ ] History **enabled**
    - [ ] Scoreboard **enabled**
    - [ ] Final scordeboard **enabled** (short)
    - [ ] 2 matches:
        - 1st with 5 rounds maximum; resolves in right, left, center, and repeat; requires at least 3 hits in a row
        - 2nd with 3 rounds maximum; resolves in left, center and right; requires at least 3 hits in a row

1. Medium - Warm-up with time
    - [ ] 3 actions: left, center and right
    - [ ] Countdown before enabling the action buttons
    - [ ] History **disabled**
    - [ ] Scoreboard **enabled**
    - [ ] Final scordeboard **enabled**
    - [ ] Probabilistic kicks; max of 5 (`limitPlays`)

1. Hard - Memory Game
    - [ ] 3 actions: left, center and right
    - [ ] 2 matches:
        - 1st: at least 3 rounds, up to 21; expects 3 succesful hits in a row
        - 2nd: 3 rounds that should resolve in the sequence: right, left and center
    - [ ] History disabled
    - [ ] Scoreboards enabled

1. Hard - Declarative Memory:
    - [ ] Same as "Hard - Memory Game"
