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
  - **IsLongPressHeld** activates the Hardware object assigned to LongPress exactly when the temporal threshold is passed when set to true.  When false input is decided when the button is released

A hardware object must include the class field.  All other fields are optional and will be given a default value if omitted.  The value shown to the right of the field is its default value.

### Button
Any type beginning with the name "Button" is part of the button type.  Any button type can be assigned to a field with "Button" listed as the default value, and any such field will default to a button which does nothing.  Any button can press, release, and tap.  Press presses, release releases, and tap presses and then releases.

### ButtonAction
```
"$type": "Backend.ButtonAction, scwin",
"IsLayered": true,
"Name": ""
```
Adds an action map when it is pressed and removes the action map from the action layering when it is released.  **Name** specifies the action map to choose.  **IsLayered** adds layering like Steam Input's action layer; if an input in the action map is unbound the program will then try the input in the layer below.  It is recommended to keep this enabled.

### ButtonDoubler
```
"$type": "Backend.ButtonDoubler, scwin",
"Button": Button
```
Taps **Button** when pressed and taps again when released.

### ButtonDualStage
```
"$type": "Backend.ButtonDualStage, scwin",
"Button": Button,
```
Presses **Button** when pressed and then released **Button** when pressed again.

### ButtonKey
```
"$type": "Backend.ButtonKey, scwin",
"Key": 0
```
Basic button input simulator.  Presses the given key corresponding to the keycode when presses and releases it when released.

### ButtonScroll
```
"$type": "Backend.ButtonScroll, scwin",
"AsClicks": true,
"IsContinuous": false
```
Scrolls the mouse wheel when pressed.  **Amount** specifies what magnitude to scroll by.  **AsClicks** specifies is the magnitude represents clicks of the mouse wheel.  **IsContinuous**, when false, will scroll by the magnitude once for every press of the button.  When true it will continuously scroll the mouse wheel while the button is being held.  Input is send 100 times per second.

### ButtonMany
```
"$type": "Backend.ButtonMany, scwin",
"Buttons": []
```
Presses every button in **Buttons**, a list of type Button, when pressed and releases every listed button when released.

### PadButtonCross
```
"$type": "Backend.PadButtonCross, scwin",
"East": Button,
"North": Button,
"West": Button,
"South": Button,
"Inner": Button,
"Outer": Button,
"HasOverlap": true,
"Deadzone": 0.2,
"InnerRadius": 0.35,
"OuterRadius": 0.1,
"OverlapIgnoranceRadius": 0.5
```

### StickButtonCross


