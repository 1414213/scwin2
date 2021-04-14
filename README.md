# scwin2
User-level driver for steam controllers built in .NET.  Allows for functionality to be defined using key-mappings written in JSON and inputted using a command line interface.

### Dependencies
- .NET 5
- ViGEm Bus Driver: Will run without this but will fail if gamepad inputs are included in the keymap

## How To Use
The program accepts the name of a keymap.  If the given keymap cannot be found then it creates a blank keymap of the given name.
### Flags
- -n: start the program without creating a virtual gamepad

## Creating Keymaps
The JSON file defines an *Map* (keymap) object:
- Field **Name** specifies the inputmap's name.  This is optional
- Field **ActionMaps** contains Maps to be applied, similar to Steam Input's action sets.  Each Map *must* contain a name
- Field **InputMap** defines functionality.  Each field is some hardware on the controller and contains fields to define how that hardware creates functionality:
  - **Regular** is a straight binding of a *Hardware* object to the specified hardware
  - **TemporalThreshold** defines the timing for when a short press becomes a long press.  Only applies to buttons
  - **ShortPress** is the functionality used for a pressing time length below the threshhold
  - **LongPress** is the functionality used for a pressing time length above the threshold
  - **IsLongPressHeld** activates the Hardware object assigned to LongPress exactly when the temporal threshold is passed when set to true.  When false the input is decided when the button is released.

### Hardware Types
