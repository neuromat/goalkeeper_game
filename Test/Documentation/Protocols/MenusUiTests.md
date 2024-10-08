# Menu UI Tests

Copy the CustomTrees from `Test/Samples/CustomTrees` into `<GameData>`. `CustomTrees` contain the opponent folders (`Easy`, `Medium` and `Hard`) and the `index.info`

## Basic Flow

Successfully execute paths derived from the Transitions Tree diagram. Aims to validate the UI navigation.

- [ ] Language Selection -> ToS/EULA and Nickname -> Opponent Selection -> Game Mode Selection
- [ ] Game Mode Selection -> About -> Game Mode Selection
- [ ] Game Mode Selection -> Tutorial -> Game Mode Selection
- [ ] Game Mode Selection -> Opponent Selection
- [ ] Game Mode Selection -> Game
- [ ] Game -> Post-Game -> Game Mode Selection
- [ ] Game -> Post-Game -> Next Phase (only for "Medium" and "Hard" opponents)
- [ ] Next Phase -> Game
- [ ] Game -> Quit Game Confirmation -> Game
- [ ] Game -> Quit Game Confirmation -> Mode Selection

## Textual Content

After selecting one of the languages, verify if the textual components match:

- content: expected content in the view;
- language: the language selected in the first view.

1. ToS/EULA and Nickname:
    - Terms of Service and End-User License Agreement text box
        - [ ] content
        - [ ] language
    - Nickname placeholder
        - [ ] content
        - [ ] language
    - Agree/Do Not Agree checkboxes
        - [ ] content
        - [ ] language
    - Next button
        - [ ] content
        - [ ] language

1. Opponent Selection
    - Instructional text (above the opponents list)
        - [ ] content
        - [ ] language
    - Opponents: expected to see a list of opponents/teams defined in `<GameData>/CustomTrees/index.info`, unaffected by the language selection
        - [ ] content
    - Next button
        - [ ] content
        - [ ] language

1. Game Mode Selection:
    - Title: `<Opponent> : Game Menu`
        - [ ] content
        - [ ] language
    - Instruction text: on the right side
        - [ ] content
        - [ ] language
    - Tutorial, About, Change opponent and Exit buttons
        - [ ] content
        - [ ] language
    - Start game checkboxes: if enabled in the opponent's `<GameData>/CustomTrees/<Opponent>/tree1.txt`
        - [ ] content
        - [ ] language
    - Game Modes: listed in the `<GameData>/CustomTrees/<Opponent>/tree1.txt`, affected by the selected language

1. About
    - Submenus buttons on the left side: "What is", "Software", "Docs", "Credits" and "Return"
        - [ ] content
        - [ ] language
    - "What is" descriptive text on the right side
        - [ ] content
        - [ ] language
    - "Software" descriptive text on the right side
        - [ ] content
        - [ ] language
    - "Docs" descriptive text on the right side
        - [ ] content
        - [ ] language
    - "Credits" descriptive text on the right side
        - [ ] content
        - [ ] language

## Opponents and Game Modes

> Note: the gamemodes and gameplay presented in this document might not be relevant from a research perspective and, instead, they aim to exercise relevant test paths

1. Opponents: "Easy", "Medium" and "Hard"

1. Game Modes
    - Easy
        - [ ] "Warm-Up" and "Goalkeeper"
        - [ ] "Start game" checkboxes **enabled** 
    - Medium
        - [ ] "Timed Warm-Up", "Goalkeeper"
        - [ ] "Start game" checkboxes **enabled**
    - Hard
        - [ ] "Memory Game", "Declarative Memory"
        - [ ] "Start game" checkboxes **disabled**

1. *optional*: remove one of the opponents in `Test/Samples/CustomTrees/index.info`. The opponent should no longer be available in the "Opponent Selection" view
