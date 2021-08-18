using System;
using System.Collections.Generic;
using System.Diagnostics;

using Device.Net;
using Hid.Net.Windows;
//using SerialPort.Net.Windows;

using MethodExtensions;


namespace SteamControllerApi {
	public class Controller {
		public ushort VendorId => 0x28de;
		public ushort ProductId => 0x1102;
		public Dictionary<KeyInternal, IInputData> State { get; } = new ();
		public Dictionary<KeyInternal, Stopwatch> StateTimings { get; } = new ();

		//private Dictionary<Key, IInputData> state = new Dictionary<Key, IInputData>();
		//private Dictionary<Key, Stopwatch> stateTimings = new Dictionary<Key, Stopwatch>();
		private readonly int amountOfKeys = 0;

		// state stores

		// hasLPadTouch and hasStick track if the left touchpad data slot contains either the stick
		// or the pad.
		private bool hasLPadTouch = false;
		private bool hasStick = false;
		
		// Tracks if the controller is oscillating stick and left trackpad data in slot [16, 19].  This must 
		// be done so that, when oscillating data, stick data is not confused with trackpad data and so that 
		// the program doesn't mistake the absence of new stick data as the stick being returned into a 
		// neutral position.
		private bool isOscillating = false;

		public Controller() {
			// initialize continuous states to their neutral position
			this.State.Add(KeyInternal.LTrigger, new TriggerData(0, true, Flags.None));
			this.State.Add(KeyInternal.RTrigger, new TriggerData(0, false, Flags.None));
			this.State.Add(KeyInternal.Motion, new MotionData((0, 0, 0, 0), (0, 0, 0), Flags.None));
			this.State.Add(KeyInternal.Stick, new StickData((0, 0), Flags.None));
			foreach (KeyInternal k in Enum.GetValues(typeof(KeyInternal))) {
				this.amountOfKeys++;
				this.StateTimings.Add(k, new Stopwatch());
			}
		}

		public IList<IInputData> GenerateEvents(ReadResult input) => this.ParseState(this.ParseData(input.Data));

		public IList<IInputData> GenerateEvents(byte[] input) => this.ParseState(this.ParseData(input));

		private Dictionary<KeyInternal, IInputData> ParseData(byte[] data) {
			// preparation
			const double angle = 15d * (Math.PI / 180d);
			const int LPadTouchAndStickPush = 0x00_00_80;
			var events = new Dictionary<KeyInternal, IInputData>(amountOfKeys);

			// reset state flags
			isOscillating = false;
			hasStick = false;
			hasLPadTouch = false;

			// check for pressed buttons

			// buttons are hexes 9-11 (indexes 8-10) in the input.Data byte array
			// convert that portion into a number
			byte[] a_buttonsTemp = new byte[4];
			Array.Copy(data, 8, a_buttonsTemp, 1, 3);
			// copy button bit flags, reversing to correct endianness because I wrote the keycodes backwards.
			// This operation is not CLS compliant, might be bad later
			Array.Reverse(a_buttonsTemp);
			uint buttons = BitConverter.ToUInt32(a_buttonsTemp, 0);
			foreach (Key key in Enum.GetValues(typeof(Key))) {
				if (((uint)key & buttons) == (uint)key) switch (key) {
					case Key.LPadTouch:
					case Key.LPadClick: {
						// rotate touchpad coordinates by 15 degrees
						short x = BitConverter.ToInt16(data.SliceNew(16, 17), 0);
						short y = BitConverter.ToInt16(data.SliceNew(18, 19), 0);
						(x, y) = this.RotatePointAroundOrigin(x, y, -angle);
						hasLPadTouch = true;
						events.Add(key.ToInternal(), new TrackpadData(key, (x, y), Flags.Pressed));
						break;
					}
					case Key.RPadTouch:
					case Key.RPadClick: {
						// rotate touchpad coordinates by -15 degrees
						short x = BitConverter.ToInt16(data.SliceNew(20, 21), 0);
						short y = BitConverter.ToInt16(data.SliceNew(22, 23), 0);
						(x, y) = this.RotatePointAroundOrigin(x, y, angle);
						events.Add(key.ToInternal(), new TrackpadData(key, (x, y), Flags.Pressed));
						break;
					}
					// button press
					default:
						events.Add(key.ToInternal(), new ButtonData(key, Flags.Pressed));
						break;
				}
			}

			// add continuous inputs
			events.Add(KeyInternal.LTrigger, new TriggerData(data[11], true, Flags.None));
			events.Add(KeyInternal.RTrigger, new TriggerData(data[12], false, Flags.None));
			events.Add(KeyInternal.Motion, new MotionData(
				(
					roll:  BitConverter.ToInt16(data.SliceNew(44, 45), 0),
					pitch: BitConverter.ToInt16(data.SliceNew(42, 43), 0),
					yaw:   BitConverter.ToInt16(data.SliceNew(46, 47), 0),
					w:     BitConverter.ToInt16(data.SliceNew(40, 41), 0)
				), (
					x: BitConverter.ToInt16(data.SliceNew(38, 39), 0), // acceleration
					y: BitConverter.ToInt16(data.SliceNew(36, 37), 0),
					z: BitConverter.ToInt16(data.SliceNew(34, 35), 0)
				),
				Flags.None
			));

			// thumbstick coordinates are send in the position for left pad coordinates and thus the thumbstick's
			// position may only be sent when the left pad is not being touched.  When both the thumbstick and left pad
			// need to send data the controller oscillates between sending data for the left pad and thumbstick.

			if ((buttons & LPadTouchAndStickPush) == LPadTouchAndStickPush) isOscillating = true;
			if (!hasLPadTouch) {
				hasStick = true;
				events.Add(KeyInternal.Stick, new StickData(
					Position: (BitConverter.ToInt16(data, 16), BitConverter.ToInt16(data, 18)),
					Flags: Flags.None));
			} else if (hasLPadTouch && !isOscillating) {
				hasStick = true;
				events.Add(KeyInternal.Stick, new StickData((0, 0), Flags.None));
			}

			return events;
		}

		/// <summary>
		/// Takes the pressed key events generated from reading the device input and compares it to
		/// the previously read press events stored in the steam controller instance to decide which keys were
		/// pressed, held, and released, then generates a list of the press and release of events to return.
		/// </summary>
		private IList<IInputData> ParseState(Dictionary<KeyInternal, IInputData> newEvents) {
			var generatedEvents = new List<IInputData>(amountOfKeys);

			// Add potential changes to the continuous states (left and right trigger and joystick).
			// Events of continuous states are only added if there is a potiential change in their state.
			// This means that an event isn't generated if both the new and previous states are 0 and a 
			// harware with continuous state will generate one 0 event when and only when returned to a neutral 
			// position.  This 0 can be used to track when the hardware has returned to a neutral position.
			if (
				((ITriggerData)State[KeyInternal.LTrigger]).Trigger != 0
				|| ((ITriggerData)newEvents[KeyInternal.LTrigger]).Trigger != 0
			) {
				State[KeyInternal.LTrigger] = newEvents[KeyInternal.LTrigger];
				generatedEvents.Add(newEvents[KeyInternal.LTrigger]);
			}
			if (
				((ITriggerData)State[KeyInternal.RTrigger]).Trigger != 0
				|| ((ITriggerData)newEvents[KeyInternal.RTrigger]).Trigger != 0
			) {
				State[KeyInternal.RTrigger] = newEvents[KeyInternal.RTrigger];
				generatedEvents.Add(newEvents[KeyInternal.RTrigger]);
			}
			State[KeyInternal.Motion] = newEvents[KeyInternal.Motion];
			generatedEvents.Add(newEvents[KeyInternal.Motion]);

			if (hasStick) {
				var position = ((IPositional)State[KeyInternal.Stick]).Position;
				var newPosition = ((IPositional)newEvents[KeyInternal.Stick]).Position;
				if (position is not (0, 0) || newPosition is not (0, 0)) {
					State[KeyInternal.Stick] = newEvents[KeyInternal.Stick];
					generatedEvents.Add(newEvents[KeyInternal.Stick]);
				}
			}
			
			// Compare the new and previous states to check for any changes in the controller's state.
			// Detected changes will generate press and release events for the buttons.
			foreach (KeyInternal k in Enum.GetValues(typeof(KeyInternal))) {
				// If previous events has:
				if (State.ContainsKey(k)) {
					// A corresponding new event, the button was held.
					if (newEvents.ContainsKey(k)) {
						// Track pads carry a continuous state so their held press cannot be ignored.
						if (k is KeyInternal.LPadClick
							or KeyInternal.RPadClick
							or KeyInternal.LPadTouch
							or KeyInternal.RPadTouch
						) {
							generatedEvents.Add(newEvents[k]);
						}
						// The new state must be given held key presses to remind 
						// it that some keys are still being pressed.
						this.State[k] = newEvents[k];
					} else {
					// No corresponding new event, the key was released.
					// The state of the release event would be the released key's last known state.
						if (k == KeyInternal.LPadTouch && isOscillating) continue;
						else if (State[k] is not IButtonData) continue;
						else {
							StateTimings[k].Stop();
							if (State[k] is TrackpadData td) generatedEvents.Add(td with {
								Flags = (td.Flags & (~Flags.Pressed)) | Flags.Released,
								TimeHeld = StateTimings[k].ElapsedMilliseconds,
							});
							else {
								generatedEvents.Add(new ButtonData(
									Key: (Key)((int)k),
									Flags: (State[k].Flags & (~Flags.Pressed)) | Flags.Released,
									TimeHeld: StateTimings[k].ElapsedMilliseconds));
							}
							State.Remove(k);
							StateTimings[k].Reset();
						}
					}
				} else { // If previous event doesn't have -
					// - a corresponding new event then there was a key press -
					if (newEvents.ContainsKey(k)) {
						this.StateTimings[k].Start();
						this.State.Add(k, newEvents[k]);
						generatedEvents.Add(newEvents[k]);
					}
					// - else no previous event and no corresponding new event; nothing happens here.
				}
			}
			// reset state flags
			isOscillating = false;

			return generatedEvents;
		}

		/// <summary>Rotate point given from trackpad to eliminate the built-in 15 degrees of rotation.</summary>
		private (short x, short y) RotatePointAroundOrigin(short x, short y, double angle, bool usePolar = false) {
			if (usePolar) {
				// convert to polar coordinates
				double r = Math.Sqrt(x * x + y * y);
				double theta = 0;
				if (y >= 0 && r != 0) theta = Math.Acos(x / r);
				else if (y < 0)       theta = -Math.Acos(x / r);
				else if (r == 0)      theta = Double.NaN;

				// rotate by given angle
				theta += angle;

				// convert back into cartesian coordinates
				if (!Double.IsNaN(theta)) {
					// coordinate needs to be clamped to the value of a short because of error or something
					x = Convert.ToInt16(Math.Clamp(r * Math.Cos(theta), Int16.MinValue, Int16.MaxValue));
					y = Convert.ToInt16(Math.Clamp(r * Math.Sin(theta), Int16.MinValue, Int16.MaxValue));
				}
				return (x, y);
			} else {
				var x_prime = Math.Clamp(x * Math.Cos(angle) - y * Math.Sin(angle), Int16.MinValue, Int16.MaxValue);
				var y_prime = Math.Clamp(x * Math.Sin(angle) + y * Math.Cos(angle), Int16.MinValue, Int16.MaxValue);

				// for some reason if the value is at the maximum range of a short int it becomes
				// slightly greater than the range and causes an overflow.  This hack prevents that.
				// It only seems to happen at the most extreme values so hopefully this won't lessen
				// accuracy too much.
				// The hack seems to create 15 degrees of deadzone at the trackpads' most farthest out
				// point on the x and y cardinals.
				// On further testing, the deadzone seems to be a problem with the controller and
				// not the algorithm.
				return ((short)x_prime, (short)y_prime);
			}
		}
	}
}
