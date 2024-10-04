from statemachine import StateMachine, State


class MenusUi(StateMachine):
    """Menus UI state machine"""
    language_selection = State("Language Selection", initial=True)
    tos_eula_nickname = State("ToS/EULA and Nickname")
    opponent_selection = State("Opponent Selection")
    game_mode_selection = State("Game Mode Selection")
    game = State("Game")
    post_game = State("Post-Game")
    quit_game_confirmation = State("Quit Game Confirmation")
    tutorial = State("Tutorial")

    # About UI State Machine
    about = State("About")

    proceed = language_selection.to(tos_eula_nickname) | \
        tos_eula_nickname.to(opponent_selection) | \
        opponent_selection.to(game_mode_selection) | \
        game_mode_selection.to(game) | \
        game_mode_selection.to(tutorial) | \
        game_mode_selection.to(about)
    
    go_back = game_mode_selection.to(opponent_selection) | \
        tutorial.to(game_mode_selection) | \
        about.to(game_mode_selection)
    
    quit_game = game.to(quit_game_confirmation)
    quit_yes = quit_game_confirmation.to(post_game)
    quit_no = quit_game_confirmation.to(game)


if __name__ == '__main__':
    state_machine = MenusUi()
    state_machine._graph().write_png("../Diagrams/menu_ui_state_machine.png")
