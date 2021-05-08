# scwin2
User-level driver for steam controllers built in .NET.&ensp;Allows for functionality to be defined using key-mappings written in JSON and inputted using a command line interface.

This is a hobby project that aims to suppliment Steam Input by implementing features in a more simple and naïve way to allow for more flexibility when configuring the steam controller.&ensp;For example when a button assigned as a mouse scroll can be set to click the scroll wheel or scroll it while held, and touches and clicks of the trackpads can be configured seperately.

### Dependencies
- [.NET 5](https://dotnet.microsoft.com/)
- [ViGEm Bus Driver](https://github.com/ViGEm/ViGEmBus/releases/): Will run without this but inputs set to simulate a gamepad will do nothing


## How To Use
It is recommended to use this alongside the Steam client and Steam Input.&ensp;The first argument should be the name of a keymap.&ensp;If the given keymap cannot be found then it creates a blank keymap of the given name.&ensp;Flags can appear anywhere.
### Flags
- -n, --no-gamepad: start the program without creating a virtual gamepad
- -d, --directory: specify a directory to search for input maps from as the next argument


## Creating Keymaps
The JSON file defines an *Map* object:
- Field *Name* specifies the inputmap's name.&ensp;This is optional
- Field *ActionMaps* contains Maps to be applied, similar to Steam Input's action sets.&ensp;Each Map *must* contain a name
- Field *InputMap* defines the keymap.&ensp;Each contained field is some input on the controller which is assigned a simulated hardware.

Each simulated input must include the *$type* field.&ensp;All other fields are optional and will be given a default value if omitted.&ensp;The value shown to the right of the field is its default value.

### Button
Any type beginning with the name "Button" is a type of a button type.&ensp;Any button type can be assigned to a field with "Button" listed as its default value and any such field will default to a button which simulates no input.&ensp;Any button can simulate a press, release, and tap (press and then release).

### ButtonAction
```
"$type": "Backend.ButtonAction, scwin",
"IsLayered": true,
"Name": ""
```
Functions like action layers in Steam Input.&ensp;Layers an action map when it is pressed and removes it when released.&ensp;*Name* specifies the action map to choose.&ensp;*IsLayered* sets transparency; when true if an input in the layer is unbound the program will then try to use the equivalent input from the layer below.$ensp;When false an input with no simulated input bound to it will do nothing.&ensp;It is recommended to keep this true.

### ButtonDoubler
```
"$type": "Backend.ButtonDoubler, scwin",
"Button": Button
```
Taps *Button* when pressed and taps it again when released.

### ButtonDualStage
```
"$type": "Backend.ButtonDualStage, scwin",
"Button": Button,
```
Presses *Button* when pressed and released and then released *Button* when pressed and released again.

### ButtonKey
```
"$type": "Backend.ButtonKey, scwin",
"Key": "None"
```
Simulates input from a button or key.&ensp;*Key* is the button or key to simulate and can be its name or code.

### ButtonScroll
```
"$type": "Backend.ButtonScroll, scwin",
"Amount": 1.0,
"AsClicks": true,
"IsContinuous": false
```
Scrolls the mouse wheel when pressed.&ensp;*Amount* specifies the amount to scroll.&ensp;*AsClicks* specifies whether or not *Amount* represents clicks of the mouse wheel.&ensp;*IsContinuous*, when set to false, will simulate input once for every press of the button.&ensp;When set to true holding the button will continuously scroll the mouse wheel; while held input is sent 100 times per second.

### ButtonTemporal
```
"$type": "Backend.ButtonTemporal, scwin",
"Short": Button,
"Long": Button,
"TemporalThreshold": 500,
"IsLongPressHeld": false
```
Allows a single button to simulate two buttons, differentating between the two by how long the button is pressed.&ensp;*TemporalThreshold* defines the point in time when to differentiate between a short or long press.&ensp;Measured in milliseconds.&ensp;*Short* is a Button type activated by a short press.&ensp;*Long* is a Button type activated by a long press.&ensp;*IslongPressHeld*, when set to true, sets *Long* to activate exactly when the temporal threshold is passed.&ensp;When false simulation is differentiated when the button is released.

### ButtonMany
```
"$type": "Backend.ButtonMany, scwin",
"Buttons": []
```
Presses every button in *Buttons* when pressed and releases every button when released.&ensp;*Buttons* is a list of Button types.

### Trackpad
```
"DoubleTapButton": Button,
"IsDoubleTapHeld": false
```
Defines functionality shared by all simulation using a trackpad; these fields can be given to any input prefixed with "Pad".&ensp;*DoubleTapButton* specifies a Button type to be activated when the applied trackpad is touched twice in a quick succession.&ensp;*IsDoubleTapHeld* sets whether the button is held during the second touch and released when that input stops or is immediately tapped when the second touch is recieved.

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
Works the same as *StickButtonCross*.&ensp;*OverlapIgnoranceRadius* specifies a distance, measured as a proportion of the trackpad's radius from its center to its edge, within which to not simulate diagonal input.&ensp;Its purpose is to hopefully make swiping feel more precise.

### PadRadial
```
"$type": "Backend.PadRadial, scwin",
"Buttons": [],
"Deadzone": 0.0,
"AngleOffset": 0.0,
"IncrementsLeftElseRight": true
"TapsElseHolds": false
```
Works the same as *StickRadial*.&ensp;All buttons in *Buttons* are released when input to the trackpad stops.

### PadScroll
```
"$type": "Backend.PadScroll, scwin",
"IsWheelElseSwipe": true,
"Sensitivity": 5.0,
"Reversed": false,
"SwipeAlongXElseY": true
```
Sets a trackpad to simulate scrolling the mouse wheel.&ensp;*IsWheelElseSwipe* sets whether input is given by swiveling the trackpad or by swiping it.&ensp;*Sensitivity* sets how much a swivel or swipe scrolls the mouse.&ensp;*Reversed* reverses the direction swivels and swipes scroll.&ensp;*SwipeAlongXElseY* specifies whether swipes should travel horizontally or vertically.

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
Works the same as *PadSlideStick* but simuates a button cross instead of a thumbstick.

### PadSlideStick
```
"$type": "Backend.PadSlideStick, scwin",
"RelativeSize": 0.5,
"Deadzone": 0.1,
"IsLeftElseRight": false,
"Anchored": false
```
Translates sliding input on the trackpad into thumbstick input.&ensp;For example sliding leftwards will push the simulated thumbstick left and then sliding northwards will push it into a more north-facing position.&ensp;*RelativeSize* sets how big the simulated thumbstick is relative to the diameter of the trackpad.&ensp;*Deadzone* sets the deadzone of the simulated thumbstick.&ensp;*IsLeftElseRight* specifies whether to simulate a left or right thumbstick.&ensp;*Anchored* sets what happens when input leaves the area of the simulated thumbstick.&ensp;When set to false the center of the simulated thumbstick is dragged along the surface of the trackpad.&ensp;When true leaving its area simulates pushing the thumbstick to its maximum degree of tilt.

### PadStick
```
"$type": "Backend.PadStick, scwin",
"Deadzone": 0.2,
"OuterLimit": 0.8,
"IsLeftElseRight": false
```
Sets a trackpad to simulate a thumbstick of a gamepad.&ensp;*Deadzone* sets an area extending from the center where simulated input is always 0.&ensp;Measured as a proportion of its radius.&ensp;*OuterLimit* specifies an area extending from the edge where entered input will simulate the thumbstick being pushed to its maximum degree of tilt.&ensp;Measured as a proportion of the radius from its center.&ensp;*IsLeftElseRight* specifies whether to simulate a left or right thumbstick.

### PadSwipe
```
"$type": "Backend.PadSwipe, scwin",
"MinimumDistance": 0.25,
"AngleOffset": 0.0,
"LongSwipeThreshold": 1.5,
"Buttons": [],
"LongSwipeButtons": [],
"IsContinuous": false,
"MinimumSpeed": 80.0
```
Translates lines drawn as swipes on a trackpad into button input based on the direction of the line.&ensp;For example, with a list of two buttons and an offset of 0.5 pi's a northward swipe would tap the first button and a southward swipe would tap the second.&ensp;*MinimumDistance* sets the minimum length a detected swipe needs to be to activate a button.&ensp;It is recommended to keep this above the default value.&ensp;*AngleOffset* specifies at what angle slice listing begins.&ensp;Measured in amounts of pi (1 would be read as 1*pi).&ensp;*LongSwipeThreshold* set the minimum length a swipe needs to be to be considered a long swipe.&ensp;Measured as a proportion of the diameter of the trackpad.&ensp;This should be greater than *MinimumDistance*; when less all swipes will be considered long swipes.&ensp;The default value is impossible so that long swipes are ignored in default function.&ensp;*Buttons* specifies a list of Button types to be tapped by swiping.&ensp;The size of this sets the amount of directions to draw lines with, extending from 0 pi to 2 pi.&ensp;*LongSwipeButtons* specifies a list of Button types to be pressed by long swipes.&ensp;Since the amount of directions is set by *Buttons* if this list is shorter than *Buttons* the unfilled directions will do nothing.&ensp;*IsContinuous* sets whether multiple swipes can be made from a single gesture.&ensp;By default the end of the swipe is where input stops on the trackpad.&ensp;When enabled this takes where a stroke stops moving to be a swipe's end with *MinimumSpeed* sets how slow the stroke needs to move to be considered stopped.&ensp;*MinimumSpeed* should be kept near the default value for best performance.

### PadTrackball
```
"$type": "Backend.PadTrackball, scwin",
"HadInertia": true,
"Sensitivity": 1966.05
"Decceleration": 0.1,
"InvertX": false,
"InvertY": false
```
Sets a trackpad to act as a trackball.&ensp;*HasInertia* sets whether the trackball will roll when input stops.&ensp;*Sensitivity* sets the resolution of sensitivity measured as pixels per diameter.&ensp;For example when set to 500 an input the length of half the trackpad will move the mouse cursor 250 pixels.&ensp;*Decceleration* sets how fast the trackball deccelerates when rolling.&ensp;*InvertX* and *InvertY* invert the x and y axises.

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
"OuterRadius": 0.0
```
Allows the thumbstick to be used like a directional pad.&ensp;*East*, *North*, *West*, and *South* bind a Button type to their respective direction.&ensp;*Inner* is a Button type pressed when input is within a certain distance from the center; distance is specified by *InnerRadius* as a proportion of the radius of the thumbstick's range measuring from the center.&ensp;*Outer* is a Button type pressed when input is a certain distance from the edge of the thumbstick's range; distance is specified by *OuterRadius* as a proportion of the radius of the thumbstick's range measured from its edge.&ensp;*HasOverlap* specifies whether there is overlap in the transition between buttons.&ensp;*Deadzone* specifies a distance from the center within which to stop creating input, measured as a proportion of the radius of the thumbstick's range from its center.

### StickRadial
```
"$type": "Backend.StickRadial, scwin",
"Buttons": [],
"Deadzone": 0.1,
"AngleOffset": 0.0,
"IncrementsLeftElseRight": true
"TapsElseHolds": false
```
Seperates the thumbstick into a number of slices and activates a slice's respective button when input enters it.&ensp;*Buttons* is a list of Button types.&ensp;"Deadzone" is measured as a proportion of the thumbstick's radius from its center and creates a range where input releases.&ensp;*AngleOffset* specifies at what angle slice 0 starts.&ensp;Measured in amounts of pie (1 would be read as 1*pi).&ensp;*IncrementsLeftElseRight* specifies whether rotating clockwise (left) or counterclockwise traverses up or down *Buttons*, respectively.&ensp;*TapsElseHolds*: if set to tap the Button respective to the entered slice will be tapped.&ensp;If set to hold the slice will be pressed when entered and released when exitted.

### StickScroll
```
"$type": "Backend.StickScroll, scwin",
"Sensitivity": 0.8,
"Deadzone": 0.2,
"Reversed": false,
"ScrollAlongXElseY": true
```
Sets the thumbstick to scroll the mouse wheel.&ensp;*Sensitivity* sets how quickly the thumbstick scrolls.&ensp;*Deadzone* sets a deadzone for the thumbstick measured from its center as a proportion of its radius.&ensp;*Reversed* reverses the direction that the thumbstick scrolls in.&ensp;*ScrollAlongXElseY* sets the thumbstick to scroll based on east and west orientation or north and south.

### StickStick
```
"$type": "Backend.StickStick, scwin",
"Deadzone": 0.2,
"IsLeftElseRight": false
```
Translates thumbstick input into thumbstick input of a gamepad.&ensp;*Deadzone* sets a radius extending from its center where input is always 0.&ensp;Measured as a proportion of the thumbstick's radius.&ensp;*IsLeftElseRight* specifies whether to simulate a left or right thumbstick.

### TriggerButton
```
"$type": "Backend.TriggerButton, scwin",
"Button": Button,
"PullThreshold": 0.5,
"IncludeSwitchInRange": false
```
Sets a trigger to press a button when pulled past a certain degree and release the button when released.&ensp;*Button* specifies a Button type to press and release.&ensp;*PullThreshold* is the distance the trigger needs to travel to activate the button.&ensp;This is a value between 0 and 1 inclusively.&ensp;*IncludeSwitchInRange* will include the distance traveled to push the switch behind the trigger as part of the range of the trigger.

### TriggerTrigger
```
"$type": "Backend.TriggerTrigger, scwin",
"IsLeftElseRight": false,
"IncludeSwitchInRange": false
```
Sets a trigger to act as a gamepad's trigger.&ensp;*IsLeftElseRight* sets input to translate to either a left or right trigger.&ensp;*IncludeSwitchInRange* will include the distance traveled to push the switch behind the trigger as part of the range of the trigger.

## Key Codes
Each simulated button has a name and an associated number.&ensp;Either can be used within an input map.
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
Pad_Period     146
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
GamepadHome    161
```
