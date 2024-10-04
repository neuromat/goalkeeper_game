# Test Documentation Scripts

A collection of scripts that help maintaining the test documents up-to-date. It applies the strategy of "Code-as-Documentation". We suggest looking into the following testing concepts to understand the role each artifact plays in the documentation:

- Transitions Tree Testing

>If you make changes to any script, make sure you re-generate and commit the resulting diagrams

## Scripts available

- Menus UI Testing:
    - `state_machine_generator.py`: generates the State Machine from which the derives later artifacts
    - `transitions_tree_generator.py`: generates the Transitions Tree

## Scripts usage

1. [optional] create and activate the Virtual Environment
    ```shell
    python3 -m venv .venv
    source .venv/bin/activate
    ```
1. install dependencies
    ```shell
    pip install -r requirements.txt
    ```
1. Run script
    >Replace `<script>` with the script you want to run
    ```shell
    python <script>
    ```
