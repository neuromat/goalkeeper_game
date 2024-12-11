# EEG Equipment Integration (Windows only)

This project was designed to operate along with the EEG equipments ([TODO] add the equipment spec)

The communication relies on the system's serial and parallel ports.

## Serial Communication Testing

The serial communication feature is active for CustomTrees with the following lines:
```json
{
    ...
    "sendMarkersToEEG":"serial",
    "portEEGserial":"COM4",
    ...
}
```

### Setup

To test the correct integration using the serial port without connecting to the actual EEG equipment, you need to:

1. Install a virtual serial port emulator (VSPE). We used [Eterlogic's Virtual Serial Port Emulator](https://eterlogic.com/Products.VSPE.html), which is not a free tool, but it has a trial period.
1. Create the VSP to which the game will connect with the following configuration:
    ```yaml
    device type: "Connector"
    virtual serial port: "COM4"
    baud rate: 9600
    stop bits: 1
    parity: none
    handshake: none
    byte size: 8
    ```
1. In VSPE window, open the terminal (Tools > Terminal) to inspect the bytes sent from the game. Configure as follows:
    ```yaml
    data source: "Serial port"
    port: "COM4"
    speed: "9600"
    ```
1. Execute the test case with the expected outputs:
    - Note: `0x00` marks the beginning of the match. `0x01` marks the beggining of the player's turn, and preceeds each action.

    | Action | Byte | Action | Byte | Action | Byte |
    |--------|------|--------|------|--------|------|
    | LEFT   | 0x0A | LEFT   | 0x10 | LEFT   | 0x16 |
    | CENTER | 0x0B | CENTER | 0x11 | CENTER | 0x17 |
    | RIGHT  | 0x0C | RIGHT  | 0x12 | RIGHT  | 0x18 |
    | LEFT   | 0x0D | LEFT   | 0x13 | LEFT   | 0x19 |
    | CENTER | 0x0E | CENTER | 0x14 | CENTER | 0x1A |
    | RIGHT  | 0x0F | RIGHT  | 0x15 | RIGHT  | 0x1B |

# Notes

"sendMarkersToEEG":"parallel",
"portEEGserial":"LPT3",
"portSendData":"0xEEFC",

## Parallel communication
