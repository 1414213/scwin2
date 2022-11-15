# scwin2
Input mapper for steam controllers.  Allows for functionality to be defined using key-mappings written in JSON and inputted using a command line interface.

This is a hobby project that aims to suppliment Steam Input by implementing features in a more simple and naïve way to allow for more flexibility when configuring the steam controller.  For example when a button assigned as a mouse scroll can be set to click the scroll wheel or scroll it while held, and touches and clicks of the trackpads can be configured seperately.

### Dependencies
- [.NET 5](https://dotnet.microsoft.com/)
- [ViGEm Bus Driver](https://github.com/ViGEm/ViGEmBus/releases/): Will run without this but inputs set to simulate a gamepad will do nothing


## How To Use
### Building and Running
To run install dependencies and enter `dotnet run [NameOfInputmapHere]` into cmd.  It is recommended to use this alongside the Steam client and Steam Input.  The first argument should be the name of an inputmap.  If the given inputmap cannot be found then a blank inputmap of the given name is created.  Flags can be typed anywhere.
### Flags
- -n, --no-gamepad: start the program without creating a virtual gamepad
- -dir [string], --directory [string]: specify a directory to search for input maps from as the next argument
- --debug [int]: show debug information
    - 0: nothing
    - 1: raw input
    - 2: generated input events
    - 3: generated input events minus motion data
    - \> 9: custom output


## Creating Keymaps
The JSON file defines an `Map` object:
- Field `Name` specifies the inputmap's name.  This is optional
- Field `ActionMaps` contains `Map`s to be applied, similar to Steam Input's action sets.  Each `Map` *must* contain a name
- Field `InputMap` defines the keymap.  Each contained field is some input on the controller which is assigned a simulated hardware.

Each simulated input must include the `$type` field.  All other fields are optional and will be given a default value if omitted.  The value shown to the right of the field is its default value.

### Button
Any type beginning with the name "Button" is a type of a button type.  Any button type can be assigned to a field with "Button" listed as its default value and any such field will default to a button which simulates no input.  Any button can simulate a press, release, and tap (press and then release).

### ButtonAction
```
"$type": "Input.ButtonAction, scwin",
"IsLayered": true,
"Name": ""
```
Functions like action layers in Steam Input.  Layers an action map when pressed and removes it when released.  `Name` specifies the action map to choose.  `IsLayered` sets transparency; when true if an input in the layer is unbound the program will then try to use the equivalent input from the layer below.  When false an input with no simulated input bound to it will do nothing.  It is recommended to keep this true.

### ButtonDoubler
```
"$type": "Input.ButtonDoubler, scwin",
"Button": Button
```
Taps `Button` when pressed and taps it again when it is released.

### ButtonDualStage
```
"$type": "Input.ButtonDualStage, scwin",
"Button": Button,
```
Presses `Button` when pressed and released and then released `Button` when pressed and released again.

### ButtonMacro
```
"$type": "Input.ButtonMacro, scwin",
"Pressed": [Macro],
"Held": [Macro],
"Released": [Macro],
"RepetitionsPerSecond": 100
```
Defines simulated input for a button to perform.  `Pressed` and `Released` are lists of `Macro`s which are performed when the button is pressed or released, respectively.  `Held` will repeat its given macros while the button is held.  `RepetitionsPerSecond` defines how many times per second `Held`'s `Macro`s are performed.

The order of which each list of `Macro`s is performed is first element to last.  The order of which input fields in a `Macro` are simulated is not guaranteed, save that button presses will always occur before releases and waiting will always occur last.  If order is important then define the simulated inputs as seperate `Macro` objects.

### Macro
```
"PressButtons": [Key]
"ReleaseButtons" [Key]
"MoveMouse": {"x": 0, "y": 0, "relatively": false},
"ScrollMouse": {"amount": 0, "asClicks": false"},
"PullLeftTrigger": 0,
"PullRightTrigger": 0,
"MoveLeftStick": {"x": 0, "y": 0, "relatively": false},
"MoveRightStick": {"x": 0, "y": 0, "relatively": false},
"AddActionLayer": "",
"RemoveActionLayer": "",
"AddActionLayerAsTransparent": true,
"Wait": 0
```
Defines input to simulate.
- `PressButtons` and `ReleaseButtons` are lists containing type `Key` that will press and release those `Key`s, respectively.  Each `Key` can be defined as either it's keycode or it's name.
- `MoveMouse` simulates mouse movement.  `x` and `y` define how many pixels to move the mouse cursur and `relatively` defines whether to move from the current position when true or to move to that coordinate on the display when false.
- `ScrollMouse` simulates scrolling the mouse.  `amount` defines how far to scroll and `asClicks` defines whether the amount is measured as clicks of the scroll wheel.
- `PullLeftTrigger` and `PullRightTrigger` define an amount to pull the left and right triggers of the virtual gamepad, respectively.  Their values must be in the range of \[0, 1].
- `MoveLeftStick` and `MoveRightStick` define an amount by which to move the left and right thumbsticks of the virtual gamepad, respectively.  `x` and `y` define the x and y coordinates of the thumbsticks and must be in the range of \[-1, 1].  `relatively` defines whether to move a thumbstick from it's current position when true or to that coordinate when false.
- `AddActionLayer` adds a given action layer.
- `RemoveActionLayer` removes a given action layer.
- `AddActionLayerAsTransparent` defines whether blank inputs in the action layer activate the input beneath themselves in the layering when true or do nothing when false.
- `Wait` defines an amount of time, measured in milliseconds, to wait before performing more `Macro`s.

### ButtonKey
```
"$type": "Input.ButtonKey, scwin",
"Key": "None"
```
Simulates input from a key.  `Key` is the key to simulate and can be defined as its name or key code.

### ButtonScroll
```
"$type": "Input.ButtonScroll, scwin",
"Amount": 1.0,
"AsClicks": true,
"IsContinuous": false
```
Scrolls the mouse wheel when pressed.  `Amount` specifies the amount to scroll.  `AsClicks` specifies whether or not `Amount` represents clicks of the mouse wheel.  `IsContinuous` will continuously scroll the mouse wheel while held when true and scroll a distance of one `Amount` when pressed when false.  While held and `IsContinuous` is true input is sent 100 times per second.

### ButtonTemporal
```
"$type": "Input.ButtonTemporal, scwin",
"Short": Button,
"Long": Button,
"TemporalThreshold": 500,
"IsLongPressHeld": false
```
Allows a single button to simulate two buttons, differentating between the two by how long the button is pressed.  `TemporalThreshold` defines the point in time when to differentiate between a short or long press and is measured in milliseconds.  `Short` is a `Button` activated by a short press.  `Long` is a `Button` activated by a long press.  When `IslongPressHeld` is true `Long` is pressed when the time held surpasses the `TemporalThreshold` and is then released on release.  If the duration of the hold is shorter than the `TemporalThreshold` then `Short` is tapped on release.  When `IsLongPressHeld` is false `TemporalThreshold` is measured as the duration of the hold.  On release if the duration of the hold is shorter than the `TemporalThreshold` then `Short` is tapped else `Long` is tapped.

### ButtonMany
```
"$type": "Input.ButtonMany, scwin",
"Buttons": [Button]
```
Presses and releases every `Button` in `Buttons` when pressed and released, respectively.  `Buttons` is a list of `Button` types.

### Trackpad
```
"DoubleTapButton": Button,
"IsDoubleTapHeld": false
```
Defines functionality shared by all simulation using a trackpad; these JSON fields can be added to any input type whose name is prefixed with "Pad".  `DoubleTapButton` defines a `Button` type to be activated when the trackpad is touched twice in a quick succession.  `IsDoubleTapHeld` sets whether `DoubleTapButton` is held during the second touch and released when that input stops when true or is immediately tapped when the second touch is received when false.

### PadButtonCross
```
"$type": "Input.PadButtonCross, scwin",
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
Works the same as `StickButtonCross`.

### PadRadial
```
"$type": "Input.PadRadial, scwin",
"Buttons": [Button],
"Deadzone": 0.0,
"AngleOffset": 0.0,
"IncrementsLeftElseRight": true
"TapsElseHolds": false
```
Works the same as `StickRadial`.  All `Button`s in `Buttons` are released when input to the trackpad stops.

### PadScroll
```
"$type": "Input.PadScroll, scwin",
"IsWheelElseSwipe": true,
"Sensitivity": 5.0,
"Reversed": false,
"SwipeAlongXElseY": true
```
Sets a trackpad to simulate scrolling the mouse wheel.  `IsWheelElseSwipe` sets whether input is given by swiveling the trackpad or by swiping it.  `Sensitivity` sets how much a swivel or swipe scrolls the mouse.  `Reversed` reverses the direction swivels and swipes scroll.  `SwipeAlongXElseY` specifies whether swipes should travel horizontally or vertically when true and false, respectively.

### PadSlideButtonCross
```
"$type": "Input.PadSlideButtonCross, scwin",
"East": Button,
"North": Button,
"West": Button,
"South": Button,
"Inner": Button,
"Outer": Button,
"HasOverlap": true,
"OverlapIgnoranceRadius": 0,
"Deadzone": 0.2,
"InnerRadius": 0.35,
"OuterRadius": 0.1,
"RelativeSize": 0.5,
"Anchored": false
```
Works the same as `PadSlideStick` but simuates a sliding `PadButtonCross` instead of a thumbstick.  Implements all the properties of a `PadButtonCross`.

### PadSlideStick
```
"$type": "Input.PadSlideStick, scwin",
"RelativeSize": 0.5,
"Deadzone": 0.1,
"IsLeftElseRight": false,
"Anchored": false
```
Translates sliding input on the trackpad into thumbstick input.  For example sliding leftwards will push the simulated thumbstick left and then sliding northwards will push it into a more north-facing position.  `RelativeSize` sets how big the simulated thumbstick is relative to the diameter of the trackpad, with a range of \[0, 1].  `Deadzone` sets the deadzone of the simulated thumbstick, with a range of \[0, 1].  `IsLeftElseRight` specifies whether to simulate a left or right thumbstick when true and false, respectively.  `Anchored` defines what happens when input leaves the area of the simulated thumbstick.  When set to false the center of the simulated thumbstick is dragged along the surface of the trackpad.  When true leaving its area simulates pushing the thumbstick to its maximum degree of tilt.

### PadStick
```
"$type": "Input.PadStick, scwin",
"Deadzone": 0.2,
"Gradingzone": 0.8,
"IsLeftElseRight": false
```
Sets a trackpad to simulate a thumbstick of the virtual gamepad; works the same as `StickStick`.  `Deadzone` sets a radius extending from the trackpad's center where input is always 0 and is measured as a proportion of the thumbstick's radius, with a range of \[0, 1].  `Gradingzone` specifies an area measuring from the center of the thumbstick to simulate the degree of tilt of a controller's thumbstick; any input beyond this radius simulates maximum tilt of a thumbstick.  Value has a range of \[0, 1].  `IsLeftElseRight` specifies whether to simulate a left or right thumbstick when true and false, respectively.

### PadSwipe
```
"$type": "Input.PadSwipe, scwin",
"MinimumDistance": 0.25,
"AngleOffset": 0.0,
"LongSwipeThreshold": 1.5,
"Buttons": [Button],
"LongSwipeButtons": [Button],
"IsContinuous": false,
"MinimumSpeed": 80.0
```
Translates lines drawn as swipes on a trackpad into button input based on the direction of the line.  For example, with a list of two buttons and an offset of 0.5 pi's a northward swipe would tap the first button and a southward swipe would tap the second.  `MinimumDistance` sets the minimum length a detected swipe needs to be to activate a `Button`.  It is recommended to keep this above the default value.  `AngleOffset` specifies at what angle slice listing begins.  Measured in amounts of pi (1 would be read as 1\*pi).  `LongSwipeThreshold` set the minimum length a swipe needs to be to be considered a long swipe.  Measured as a proportion of the diameter of the trackpad, with a range of \[0, 1].  This should be greater than `MinimumDistance`; when less all swipes will be considered long swipes.  The default value is impossible so that long swipes are ignored in default function.  `Buttons` specifies a list of `Button` types to be tapped by swiping.  The size of this sets the amount of directions to draw lines with, extending from 0 pi to 2 pi.  `LongSwipeButtons` specifies a list of `Button` types to be pressed by long swipes.  Since the amount of directions is set by `Buttons` if this list is shorter than `Buttons` the unfilled directions will do nothing.  `IsContinuous` sets whether multiple swipes can be made from a single gesture.  By default the end of the swipe is where input stops on the trackpad.  When true this considers where a stroke stops moving to be a swipe's end point with `MinimumSpeed` setting how slow the stroke needs to move to be considered stopped.  `MinimumSpeed` should be kept near the default value for best performance.

### PadTrackball
```
"$type": "Input.PadTrackball, scwin",
"HadInertia": true,
"Sensitivity": 1966.05
"Smoothing": 500,
"Decceleration": 0.1,
"Acceleration": {
    "Amount": 2,
    "LowerBoundary": 0,
    "UpperBoundary": 2147483647,
    "Kind": "Wide"
},
"InvertX": false,
"InvertY": false
```
Sets a trackpad to act as a trackball.  `HasInertia` sets whether the trackball will roll when input stops when true.  `Sensitivity` sets the resolution of sensitivity measured as pixels per diameter.  For example when set to 500 an input the length of half the trackpad will move the mouse cursor 250 pixels.  `Decceleration` sets how fast the trackball deccelerates when rolling.  `InvertX` and `InvertY` invert the x and y axises.

Accelerator controls how the trackball accelerates input.
- `Amount` sets the factor to multiply the sensitivity by.  A value of 1 will create no acceleration.
- `LowerBoundary` sets the movement speed at which acceleration begins.  Giving a negative value will set this to 0 so that acceleration starts at all speeds.
- `UpperBoundary` sets the movement speed at which acceleration stops.  Giving this a negative value will set it to it's maximum value so that acceleration never stops.
- `Kind` sets the kind of acceleration curve the trackball will use.
  - `Linear` - Speed is multiplied by a constant value.
  - `Wide` - Wide acceleration.  Acceleration increases expodentially, giving it a more gradual start.
  - `None` - No acceleration.

### StickButtonCross
```
"$type": "Input.StickButtonCross, scwin",
"East": Button,
"North": Button,
"West": Button,
"South": Button,
"Inner": Button,
"Outer": Button,
"HasOverlap": true,
"OverlapIgnoranceRadius": 0,
"Deadzone": 0.2,
"InnerRadius": 0.35,
"OuterRadius": 0.0
```
Allows the thumbstick to be used like a directional pad.  `East`, `North`, `West`, and `South` bind a `Button` type to their respective direction.  `Inner` is a `Button` type that is pressed when input is within a certain distance from the center.  `InnerRadius` defines this distance as a proportion of the radius of the thumbstick's range measuring from the center, with a range of \[0, 1].  `Outer` is a `Button` type pressed when input is a certain distance from the edge of the thumbstick's range; this distance is specified by `OuterRadius` as a proportion of the radius of the thumbstick's range measured from its edge, with a range of \[0, 1].  `HasOverlap` specifies whether there is overlap in the transition between buttons when true.  `OverlapIgnoranceRadius` specifies a distance, measured as a proportion of the trackpad's radius from its center to its edge with a range of \[0, 1], within which to not simulate diagonal input.  Its purpose is to hopefully make swiping feel more precise.  `Deadzone` specifies a distance from the center within which to stop creating input, measured as a proportion of the radius of the thumbstick's range from its center, with a range of \[0, 1].

### StickRadial
```
"$type": "Input.StickRadial, scwin",
"Buttons": [Button],
"Deadzone": 0.1,
"AngleOffset": 0.0,
"IncrementsLeftElseRight": true
"TapsElseHolds": false
```
Seperates the thumbstick into a number of slices and activates a slice's respective `Button` when input enters it.  `Buttons` is a list of `Button` types.  `Deadzone` is measured as a proportion of the thumbstick's radius with a range of \[0, 1].  Entering the `Deadzone` is the same as releasing the trackpad.  `AngleOffset` specifies at what angle slice 0 starts.  Measured in amounts of pie (1 would be read as 1\*pi).  `IncrementsLeftElseRight` specifies whether rotating clockwise (left) or counterclockwise traverses `Buttons` forwards when true or backwards when false.  `TapsElseHolds`: if true when a slice is entered it's respective `Button` will be tapped; if false the slice's respective `Button` will be pressed when entered and released when exitted.

### StickScroll
```
"$type": "Input.StickScroll, scwin",
"Sensitivity": 0.8,
"Deadzone": 0.2,
"Reversed": false,
"ScrollAlongXElseY": true
```
Sets the thumbstick to scroll the mouse wheel.  `Sensitivity` sets how quickly the thumbstick scrolls.  `Deadzone` sets a deadzone for the thumbstick measured from its center as a proportion of its radius with a range of \[0, 1].  `Reversed` reverses the direction that the thumbstick scrolls in.  `ScrollAlongXElseY` sets the thumbstick to scroll based on east and west orientation when true or north and south when false.

### StickStick
```
"$type": "Input.StickStick, scwin",
"Deadzone": 0.2,
"Gradingzone": 1.0
"IsLeftElseRight": false
```
Translates thumbstick input into thumbstick input of a gamepad.  `Deadzone` sets a radius extending from the thumbstick's center where input is always 0.  Measured as a proportion of the thumbstick's radius with a range of \[0, 1].  `Gradingzone` specifies an area measuring from the center of the thumbstick to simulate the degree of tilt of a controller's thumbstick; any input beyond this radius simulates maximum tilt of a thumbstick.  `IsLeftElseRight` specifies whether to simulate a left or right thumbstick.

### TriggerButton
```
"$type": "Input.TriggerButton, scwin",
"Button": Button,
"PullThreshold": 0.5,
"IncludeSwitchInRange": false
```
Sets a trigger to press a button when pulled past a certain distance and release the button when released out of the range of the specified distance.  `Button` specifies a `Button` type to press and release.  `PullThreshold` is the distance the trigger needs to travel to activate the button; has range of \[0, 1].  `IncludeSwitchInRange` will include the distance traveled to push the switch behind the trigger as part of the trigger's range when true.

### TriggerTrigger
```
"$type": "Input.TriggerTrigger, scwin",
"IsLeftElseRight": false,
"IncludeSwitchInRange": false
```
Sets a trigger to act as a gamepad's trigger.  `IsLeftElseRight` sets input to translate to either a left or right trigger.  `IncludeSwitchInRange` will include the distance traveled to push the switch behind the trigger as part of the trigger's range when true.

## Key Codes
Each simulated button has a name and an associated number.  Either can be used within an input map.
```
None           0
MouseLeft      1
MouseRight     2
MouseMiddle    3
MouseFour      4
MouseFive      5
Tab            9
Space          32
Quotation      39
Comma          44
Dash           45
Dot            46
ForwardSlash   47
Row_0          48
Row_1          49
Row_2          50
Row_3          51
Row_4          52
Row_5          53
Row_6          54
Row_7          55
Row_8          56
Row_9          57
Semicolon      59
Equal          61
A              65
B              66
C              67
D              68
E              69
F              70
G              71
H              72
I              73
J              74
K              75
L              76
M              77
N              78
O              79
P              80
Q              81
R              82
S              83
T              84
U              85
V              86
W              87
X              88
Y              89
Z              90
OpenBracket    91
Backslash      92
CloseBracket   93
Grave          96
Escape         97
F1             98
F2             99
F3             100
F4             101
F5             102
F6             103
F7             104
F8             105
F9             106
F10            107
F11            108
F12            109
Backspace      110
CapsLock       111
Enter          112
LeftControl    113
RightControl   114
LeftSystem     115
LeftAlternate  116
RightAlternate 117
LeftShift      118
RightShift     119
Insert         120
Home           121
PageUp         122
PageDown       123
Delete         124
End            125
LeftArrow      126
UpArrow        127
RightArrow     128
DownArrow      129
NumberLock     130
Pad_Backslash  131
Pad_Star       132
Pad_Dash       133
Pad_Add        134
Pad_Enter      135
Pad_0          136
Pad_1          137
Pad_2          138
Pad_3          139
Pad_4          140
Pad_5          141
Pad_6          142
Pad_7          143
Pad_8          144
Pad_9          145
GamepadHome    146
Face_East      147
Face_North     148
Face_West      149
Face_South     150
Dpad_Left      151
Dpad_Up        152
Dpad_Right     153
Dpad_Down      154
LStickClick    155
RStickClick    156
LBumper        157
RBumper        158
Start          159
Back           160
```
