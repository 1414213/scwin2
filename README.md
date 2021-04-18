# scwin2
User-level driver for steam controllers built in .NET.&ensp;Allows for functionality to be defined using key-mappings written in JSON and inputted using a command line interface.

The program aims to suppliment Steam Input by implementing features in a more simple and naieve way to allow for more flexibility when configuring the steam controller with the detriment of a rougher experience.&ensp;For example when mouse scrolling is bound to a button it can be set to click the scroll wheel or scroll it while held, and touching and clicking the trackpad are seperate inputs that can be configured seperately.

### Dependencies
- .NET 5
- ViGEm Bus Driver: Will run without this but will fail if gamepad inputs are included in the keymap
- Recommended to be used alongside the Steam client


## How To Use
The program accepts the name of a keymap.&ensp;If the given keymap cannot be found then it creates a blank keymap of the given name.
### Flags
- -n: start the program without creating a virtual gamepad


## Creating Keymaps
The JSON file defines an *Map* (keymap) object:
- Field *Name* specifies the inputmap's name.&ensp;This is optional
- Field *ActionMaps* contains Maps to be applied, similar to Steam Input's action sets.&ensp;Each Map *must* contain a name
- Field *InputMap* defines functionality.&ensp;Each field is some hardware on the controller and contains fields to define how that hardware creates functionality:
  - *Regular* is a straight binding of a *Hardware* object to the specified hardware
  - *TemporalThreshold* defines the timing for when a short press becomes a long press.&ensp;Only applies to buttons
  - *ShortPress* is the functionality used for a pressing time length below the threshhold
  - *LongPress* is the functionality used for a pressing time length above the threshold
  - *IsLongPressHeld* activates the Hardware object assigned to LongPress exactly when the temporal threshold is passed when set to true.&ensp;When false input is decided when the button is released

A hardware object must include the class field.&ensp;All other fields are optional and will be given a default value if omitted.&ensp;The value shown to the right of the field is its default value.

### Button
Any type beginning with the name "Button" is part of the button type.&ensp;Any button type can be assigned to a field with "Button" listed as the default value, and any such field will default to a button which does nothing.&ensp;Any button can press, release, and tap.&ensp;Press presses, release releases, and tap presses and then releases.

### ButtonAction
```
"$type": "Backend.ButtonAction, scwin",
"IsLayered": true,
"Name": ""
```
Adds an action map when it is pressed and removes the action map from the action layering when it is released.&ensp;*Name* specifies the action map to choose.&ensp;*IsLayered* adds layering like Steam Input's action layer; if an input in the action map is unbound the program will then try the input in the layer below.&ensp;It is recommended to keep this enabled.

### ButtonDoubler
```
"$type": "Backend.ButtonDoubler, scwin",
"Button": Button
```
Taps *Button* when pressed and taps again when released.

### ButtonDualStage
```
"$type": "Backend.ButtonDualStage, scwin",
"Button": Button,
```
Presses *Button* when pressed and then released *Button* when pressed again.

### ButtonKey
```
"$type": "Backend.ButtonKey, scwin",
"Key": 0
```
Basic button input simulator.&ensp;Presses the given key corresponding to the keycode when presses and releases it when released.

### ButtonScroll
```
"$type": "Backend.ButtonScroll, scwin",
"AsClicks": true,
"IsContinuous": false
```
Scrolls the mouse wheel when pressed.&ensp;*Amount* specifies what magnitude to scroll by.&ensp;*AsClicks* specifies is the magnitude represents clicks of the mouse wheel.&ensp;*IsContinuous*, when false, will scroll by the magnitude once for every press of the button.&ensp;When true it will continuously scroll the mouse wheel while the button is being held.&ensp;Input is send 100 times per second.

### ButtonMany
```
"$type": "Backend.ButtonMany, scwin",
"Buttons": []
```
Presses every button in *Buttons*, a list of type Button, when pressed and releases every listed button when released.

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
Works the same as *StickButtonCross*.&ensp;*OverlapIgnoranceRadius* specifies a distance, measured as a proportion of the trackpad's radius extending from the center to its edge, to not allow directional input to overlap to make swiping motions feel more precise.

### PadRadial
```
"$type": "Backend.PadRadial, scwin",
"Buttons": [],
"Deadzone": 0,
"AngleOffset": 0,
"IncrementsLeftElseRight": true
"TapsElseHolds": false
```
Works the same as *StickRadial*.&ensp;All Button types are released when touch input is stopped so *Deadzone* defaults to 0.

### PadScroll
```
"$type": "Backend.PadScroll, scwin",
"IsWheelElseSwipe": true,
"Sensitivity": 5,
"Reversed": false,
"SwipeAlongXElseY": true
```
Scrolls the mouse wheel using a trackpad.&ensp;*IsWheelElseSwipe* sets whether the trackpad functions like a scroll wheel.&ensp;*Sensitivity* sets how much a swipe scrolls the mouse.&ensp;*Reversed* reverses input from swiping and increments the scroll wheel counterclockwise when set to true.&ensp;*SwipeAlongXElseY* specifies whether swipes should travel horizontally or vertically.

### PadSlideButtonCross
```
"$type": "Backend.PadSlideButtonCross, scwin",
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
"RelativeSize": 0.5,
"Anchored": false
```
Like *PadButtonCross* but uses the initial contact point as point 0.&ensp;*RelativeSize* sets how big the range of input from the initial point is relative to the diameter of the trackpad.&ensp;*Anchored* specifies whether point 0 should be dragged along when swiping distance exceeds the range of the *RelativeSize*.

### PadSlideStick
```
"$type": "Backend.PadSlideStick, scwin",
"RelativeSize": 0.5,
"Deadzone": 0.1,
"IsLeftElseRight": false,
"Anchored": false
```
Translates sliding input on the trackpad into thumbstick input.&ensp;*RelativeSize* sets how big the range of input from the initial point is relative to the diameter of the trackpad.&ensp;*Deadzone* specifies the deadzone of the thumbstick area.&ensp;*IsLeftElseRight* sets input to simulate the left or right thumbstick.&ensp;*Anchored*, when set to false, causes the thumbstick area to be dragged when input goes beyond the bounds of the area.&ensp;For example travelling up beyond the area's bounds will push the stick towards a northward position while repositoning point 0 of the area to respect that orientation.

### PadStick
```
"$type": "Backend.PadStick, scwin",
"Deadzone": 0.2,
"OuterLimit": 0.8,
"IsLeftElseRight": false
```
Sets a trackpad to act as a thumbstick of a gamepad.&ensp;*Deadzone* sets a radius form the centre where input is always 0 and is measured as a proportion of the thumbstick's radius from its center.&ensp;*OuterLimit* sets a radius of the trackpad to be used, measuring from its center.&ensp;Any input beyond the outer edge will be translated as the thumbstick being pushed to its limit.&ensp;*IsLeftElseRight* specifies whether to simulate the left or right thumbstick.

### PadSwipe
```
"$type": "Backend.PadSwipe, scwin",
"MinimumDistance": 0.25,
"AngleOffset": 0,
"LongSwipeThreshold": 1.5,
"Buttons": [],
"LongSwipeButtons": [],
"IsContinuous": false,
"MinimumSpeed": 80
```
Allows lines to be drawn on a trackpad and activates a given Button based on the angle of the swipe.&ensp;For example a list of two buttons and an offset of 0.5 pi's would tap the first Button for a northward swipe and the second for a southward swipe.&ensp;*MinimumDistance* specifies how long a detected swipe needs to be to activate a button.&ensp;It is recommended to keep this above the default value.&ensp;*AngleOffset* specifies at what angle slice 0 starts.&ensp;Measured in amounts of pi (1 would be read as 1*pi).&ensp;*LongSwipeThreshold* set the minimum length a swipe needs to be to be considered a long swipe and is a proportion of the diameter of the trackpad.&ensp;This should be greater than *MinimumDistance* or else all swipes will be considered long swipes.&ensp;The default value is impossible (1.5 times the diameter of the trackpad) so that long swipes are ignored in default function.&ensp;*Buttons* sets a list of type Button to be pressed by swiping.&ensp;The amount of slices is the size of *Buttons*.&ensp;*LongSwipeButtons* sets a list of type Button to be pressed by long swipes.&ensp;Since the amount of slices is set by *Buttons* if this list is longer than *Buttons* the Buttons exceeding the length of *Buttons* will be ignored and if it is shorter the unfilled slices will be considered empty and do nothing.&ensp;*IsContinuous* sets whether multiple swipes can be made from a single gesture.&ensp;By default the end of the swipe is where input stops on the trackpad.&ensp;When enabled this takes where a stroke stops moving to be a swipe's end with *MinimumSpeed* setting how slow the stroke needs to move to be considered stopped.&ensp;*MinimumSpeed* should be kept at the default value for best performance.

### PadTrackball
```
"$type": "Backend.PadTrackball, scwin",
"HadInertia": true,
"Sensitivity": 1966.05
"Decceleration": 0.1,
"InvertX": false,
"InvertY": false
```
Sets a trackpad to act as a trackball.&ensp;*HasInertia* sets whether the trackball will roll when input stops.&ensp;*Sensitivity* sets how many pixels an input the length of the diameter of the trackpad will move the mouse cursor.&ensp;For example a value of 500 will fit 500 pixels across the trackpad.&ensp;*Decceleration* sets how fast the trackball stops rolling.&ensp;*InvertX* and *InvertY* invert the x and y axises.

### StickButtonCross
```
"$type": "Backend.StickButtonCross, scwin",
"East": Button,
"North": Button,
"West": Button,
"South": Button,
"Inner": Button,
"Outer": Button,
"HasOverlap": true,
"Deadzone": 0.2,
"InnerRadius": 0.35,
"OuterRadius": 0.1
```
Allows the thumbstick to be used like a directional pad.&ensp;*East*, *North*, *West*, and *South* bind a Button type to each direction.&ensp;*Inner* is a Button type pressed when input is a certain distance from the center; distance is specified by *InnerRadius* as a proportion of the radius of the thumbstick's range.&ensp;*Outer* is a Button type pressed when input is a certain distance from the edge of the thumbstick's range; distance is specified by *OuterRadius* as a proportion of the radius of the range.&ensp;*HasOverlap* specifies whether there is overlap in the transition between buttons.&ensp;*Deadzone* specifies a distance from the center within which to stop creating input as a proportion of the radius of the thumbstick's range.

### StickRadial
```
"$type": "Backend.StickRadial, scwin",
"Buttons": [],
"Deadzone": 0.1,
"AngleOffset": 0,
"IncrementsLeftElseRight": true
"TapsElseHolds": false
```
Seperates the thumbstick into a number of slices of Button types and activates the slice it enters.&ensp;*Buttons* is a list of Button types.&ensp;The amount of slices is the size of *Buttons*.&ensp;"Deadzone" is measured as a proportion of the thumbstick's radius and creates a range in the center where input releases.&ensp;*AngleOffset* specifies at what angle slice 0 starts.&ensp;Measured in amounts of pie (1 would be read as 1*pi).&ensp;*IncrementsLeftElseRight* specifies whether the list progresses clockwise (left) or counterclockwise arount the thumbstick.&ensp;*TapsElseHolds*: if set to tap the Button respective to the entered slice will be tapped.&ensp;If set to hold the slice will be pressed when entered and released when exitted.

### StickScroll
```
"$type": "Backend.StickScroll, scwin",
"Sensitivity": 0.8,
"Deadzone": 0.2,
"Reversed": false,
"ScrollAlongXElseY": true
```
Sets the thumbstick to scroll the mouse wheel.&ensp;*Sensitivity* sets how much the thumbstick scrolls.&ensp;*Deadzone* sets a deadzone for the thumbstick.&ensp;*Reversed* reverses the direction that the thumbstick scrolls in.&ensp;*ScrollAlongXElseY* sets the thumbstick to scroll based on north and south orientation or east and west.

### StickStick
```
"$type": "Backend.StickStick, scwin",
"Deadzone": 0.2,
"IsLeftElseRight": false
```
Takes thumbstick input and translates it into input of a thumbstick from a gamepad.&ensp;*Deadzone* sets a radius from the center where input is always 0 and is measured as a proportion of the thumbstick's radius from its center.&ensp;*IsLeftElseRight* specifies whether to simulate the left or right thumbstick.

### TriggerButton
```
"$type": "Backend.TriggerButton, scwin",
"Button": Button,
"PullThreshold": 0.5,
"IncludeSwitchInRange": false
```
Sets a trigger to press a button when pulled past a threshold and release the button when released.&ensp;*Button* specifies a Button type to press and release.&ensp;*PullThreshold* is the distance the trigger needs to travel to activate the button.&ensp;This is a value between 0 and 1 inclusively.&ensp;*IncludeSwitchInRange* will include the distance traveled to push the switch behind the trigger as part of the range of the trigger.

### TriggerTrigger
```
"$type": "Backend.TriggerTrigger, scwin",
"IsLeftElseRight": false,
"IncludeSwitchInRange": false
```
Sets a trigger to act as a gamepad's trigger.&ensp;*IsLeftElseRight* sets input to translate to either the left or right trigger of a gameped.&ensp;*IncludeSwitchInRange* will include the distance traveled to push the switch behind the trigger as part of the range of the trigger.


