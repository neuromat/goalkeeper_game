from statemachine import StateMachine, State

class MenusUiTransitionsTree(StateMachine):
    """Menus UI transitions tree"""
    language_selection_0 = State("language_selection_0", initial=True)
    tos_eula_nickname_0 = State("tos_eula_nickname_0")
    
    opponent_selection_0 = State("opponent_selection_0")
    opponent_selection_1 = State("opponent_selection_1")
    
    game_mode_selection_0 = State("game_mode_selection_0")
    game_mode_selection_1 = State("game_mode_selection_1")
    game_mode_selection_2 = State("game_mode_selection_2")
    game_mode_selection_3 = State("game_mode_selection_3")
    game_mode_selection_4 = State("game_mode_selection_4")

    about_0 = State("about_0")
    tutorial_0 = State("tutorial_0")

    game_0 = State("game_0")
    game_1 = State("game_1")

    post_game_0 = State("post_game_0")
    quit_game_confirmation_0 = State("quit_game_confirmation_0")

    next = language_selection_0.to(tos_eula_nickname_0) | \
        tos_eula_nickname_0.to(opponent_selection_0) | \
        opponent_selection_0.to(game_mode_selection_0) | \
        game_mode_selection_0.to(about_0) | \
        about_0.to(game_mode_selection_1) | \
        game_mode_selection_0.to(tutorial_0) | \
        game_mode_selection_0.to(opponent_selection_1) | \
        tutorial_0.to(game_mode_selection_2) | \
        game_mode_selection_0.to(game_0) | \
        game_0.to(post_game_0) | \
        post_game_0.to(game_mode_selection_3) | \
        game_0.to(quit_game_confirmation_0) | \
        quit_game_confirmation_0.to(game_1) | \
        quit_game_confirmation_0.to(game_mode_selection_4)


if __name__ == "__main__":
    state_machine = MenusUiTransitionsTree()
    state_machine._graph().write_png("../Diagrams/menu_ui_transitions_tree.png")
