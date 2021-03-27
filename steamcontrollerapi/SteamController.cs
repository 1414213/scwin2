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
		public Dictionary<Key, InputData> State => this.state;
		public Dictionary<Key, Stopwatch> StateTimings => this.stateTimings;

		private Dictionary<Key, InputData> state = new Dictionary<Key, InputData>();
		private Dictionary<Key, Stopwatch> stateTimings = new Dictionary<Key, Stopwatch>();
		private int amountOfKeys = 0;

		// state stores
		private bool hasLPadTouch = false;
		private bool hasStick = false;
		private bool isOscillating = false;

		public Controller() {
			// initialize continuous states to their neutral position
			this.state.Add(Key.LTriggerPull, new InputData(Key.LTriggerPull, 0, Flags.None));
			this.state.Add(Key.RTriggerPull, new InputData(Key.RTriggerPull, 0, Flags.None));
			this.state.Add(Key.GyroMove, new InputData(Key.GyroMove, (0, 0, 0, 0), (0, 0, 0), Flags.None));
			this.state.Add(Key.StickPush, new InputData(Key.StickPush, 0, 0, Flags.None));
			foreach (Key k in Enum.GetValues(typeof(Key))) {
				this.amountOfKeys++;
				if (k == Key.LTriggerPull || k == Key.RTriggerPull || k == Key.GyroMove || k == Key.StickPush)
					continue;
				this.stateTimings.Add(k, new Stopwatch());
			}
		}

		public IList<InputData> GenerateEvents(ReadResult input) => this.ParseState(this.ParseData(input.Data));

		public IList<InputData> GenerateEvents(byte[] input) => this.ParseState(this.ParseData(input));

		private Dictionary<Key, InputData> ParseData(byte[] data) {
			// preparation
			double angle = 15d * (Math.PI / 180d);
			Dictionary<Key, InputData> events = new Dictionary<Key, InputData>(amountOfKeys);
			int LPadTouchAndStickPush = 0x00_00_80;

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
					// pass over keys that exist only in the api, not the firmware, to prevent unexpected behavior
					case Key.LTriggerPull:
					case Key.RTriggerPull:
					case Key.StickPush:
					case Key.GyroMove:
						break;
					case Key.LPadTouch:
					case Key.LPadClick: {
						// rotate touchpad coordinates by 15 degrees
						short x = BitConverter.ToInt16(data.SliceNew(16, 17), 0);
						short y = BitConverter.ToInt16(data.SliceNew(18, 19), 0);
						(x, y) = this.RotatePointAroundOrigin(x, y, -angle);
						hasLPadTouch = true;
						events.Add(key, new InputData(key, x, y, Flags.Pressed | Flags.AbsoluteMove));
						break;
					}
					case Key.RPadTouch:
					case Key.RPadClick: {
						// rotate touchpad coordinates by -15 degrees
						short x = BitConverter.ToInt16(data.SliceNew(20, 21), 0);
						short y = BitConverter.ToInt16(data.SliceNew(22, 23), 0);
						(x, y) = this.RotatePointAroundOrigin(x, y, angle);
						events.Add(key, new InputData(key, x, y, Flags.Pressed | Flags.AbsoluteMove));
						break;
					}
					default:
						events.Add(key, new InputData(key, Flags.Pressed));
						break;
				}
			}

			// add continuous inputs
			events.Add(Key.LTriggerPull, new InputData(Key.LTriggerPull, data[11], Flags.AbsoluteMove));
			events.Add(Key.RTriggerPull, new InputData(Key.RTriggerPull, data[12], Flags.AbsoluteMove));
			events.Add(Key.GyroMove, new InputData(
				Key.GyroMove, (
					BitConverter.ToInt16(data.SliceNew(46, 47), 0), // orientation
					BitConverter.ToInt16(data.SliceNew(42, 43), 0),
					BitConverter.ToInt16(data.SliceNew(44, 45), 0),
					BitConverter.ToInt16(data.SliceNew(40, 41), 0)
				), (
					BitConverter.ToInt16(data.SliceNew(38, 39), 0), // acceleration
					BitConverter.ToInt16(data.SliceNew(36, 37), 0),
					BitConverter.ToInt16(data.SliceNew(34, 35), 0)
				),
				Flags.AbsoluteMove
			));

			// thumbstick coordinates are send in the position for left pad coordinates and thus the thumbstick's
			// position may only be sent when the left pad is not being touched.  When both the thumbstick and left pad
			// need to send data the controller oscillates between sending data for the left pad and thumbstick.

			if ((buttons & LPadTouchAndStickPush) == LPadTouchAndStickPush) isOscillating = true;
			if (!hasLPadTouch) {
				hasStick = true;
				events.Add(Key.StickPush, new InputData(Key.StickPush,
				                                        BitConverter.ToInt16(data, 16),
				                                        BitConverter.ToInt16(data, 18),
				                                        Flags.AbsoluteMove));
			} else if (hasLPadTouch && !isOscillating) {
				hasStick = true;
				events.Add(Key.StickPush, new InputData(Key.StickPush, 0, 0, Flags.AbsoluteMove));
			}

			return events;
		}

		/// <summary>
		/// Takes the pressed key events generated from reading the device input and compares it to
		/// the previously read press events stored in the steam controller instance to decide which keys were
		/// pressed, held, and released, then generates a list of the press and release of events to return.
		/// </summary>
		private IList<InputData> ParseState(Dictionary<Key, InputData> newEvents) {
			IList<InputData> generatedEvents = new List<InputData>();

			// add potential change to the continuous states (left and right trigger and joystick)
			// events of continuous states are only added if there is a potiential change in their state.
			// This means that an event isn't generated if both the new and previous states are 0.
			if (
				!(this.state[Key.LTriggerPull].TriggerPull!.Value == 0
				&& newEvents[Key.LTriggerPull].TriggerPull!.Value == 0)
			) {
				this.state[Key.LTriggerPull] = newEvents[Key.LTriggerPull];
				generatedEvents.Add(newEvents[Key.LTriggerPull]);
			}
			if (
				!(this.state[Key.RTriggerPull].TriggerPull!.Value == 0
				&& newEvents[Key.RTriggerPull].TriggerPull!.Value == 0)
			) {
				this.state[Key.RTriggerPull] = newEvents[Key.RTriggerPull];
				generatedEvents.Add(newEvents[Key.RTriggerPull]);
			}
			this.state[Key.GyroMove] = newEvents[Key.GyroMove];
			generatedEvents.Add(newEvents[Key.GyroMove]);

			if (hasStick) {
				if (
					!(this.state[Key.StickPush].Coordinates!.Value.x == 0
					&& newEvents[Key.StickPush].Coordinates!.Value.x == 0 
					&& this.state[Key.StickPush].Coordinates!.Value.y == 0
					&& newEvents[Key.StickPush].Coordinates!.Value.y == 0)
				) {
					this.state[Key.StickPush] = newEvents[Key.StickPush];
					generatedEvents.Add(newEvents[Key.StickPush]);
				}
			}
			
			// compare the new and previous states to check for any changes in the controller's state
			// Detected changes will generate press and release events for the buttons.
			foreach (Key k in Enum.GetValues(typeof(Key))) {
				// if previous events has:
				if (this.state.ContainsKey(k)) {
					// a corresponding new event, the button was held
					// the new state must be given held key presses to remind it that some keys are still being pressed
					// this will make adding timing easier later
					if (newEvents.ContainsKey(k)) {
						// track pads carry a continuous state so their held press cannot be ignored
						if (k == Key.LPadClick || k == Key.RPadClick || k == Key.LPadTouch || k == Key.RPadTouch) {
							generatedEvents.Add(newEvents[k]);
							this.state[k] = newEvents[k];
						} else {
							this.state[k] = newEvents[k];
						}
					}
					// no corresponding new event, the key was released
					// the state of the release event would be the released key's last known state
					else {
						if (k == Key.LPadTouch && isOscillating) continue;
						else if (!state[k].IsButton) continue;
						else {
							stateTimings[k].Stop();
							if (state[k].IsGyroscopic) generatedEvents.Add(new InputData(
								key: k,
								coordinates: state[k].Coordinates!.Value,
								parallelCoordinates: state[k].ParallelCoordinates!.Value,
								flags: (state[k].Flags & (~Flags.Pressed)) | Flags.Released,
								timeHeld: stateTimings[k].ElapsedMilliseconds
							));
							else if (state[k].IsDuallyCoordinal) generatedEvents.Add(new InputData(
								k,
								state[k].Coordinates!.Value.x,
								state[k].Coordinates!.Value.y,
								(state[k].Flags & (~Flags.Pressed)) | Flags.Released,
								stateTimings[k].ElapsedMilliseconds
							));
							else {
								generatedEvents.Add(new InputData(
									key: k,
									flags: (state[k].Flags & (~Flags.Pressed)) | Flags.Released,
									timeHeld: stateTimings[k].ElapsedMilliseconds
								));
							}
							state.Remove(k);
							stateTimings[k].Reset();
						}
					}
				} else { // If previous event doesn't have -
					// - a corresponding new event then there was a key press -
					if (newEvents.ContainsKey(k)) {
						this.stateTimings[k].Start();
						this.state.Add(k, newEvents[k]);
						generatedEvents.Add(newEvents[k]);
					}
					// - else no previous event and no corresponding new event; nothing happens here.
				}
			}
			// reset state flags
			isOscillating = false;

			return generatedEvents;
		}

		private (short x, short y) RotatePointAroundOrigin(short x, short y, double angle, bool usePolar = false) {
			if (usePolar) {
				// convert to polar coordinates
				double r = Math.Sqrt((x * x) + (y * y));
				double theta = 0;
				if (y >= 0 && r != 0) theta = Math.Acos(x / r);
				else if (y < 0) theta = -Math.Acos(x / r);
				else if (r == 0) theta = Double.NaN;

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
				var xp = (int)(x * Math.Cos(angle) - y * Math.Sin(angle));
				var yp = (int)(x * Math.Sin(angle) + y * Math.Cos(angle));

				// for some reason if the value is at the maximum range of a short int it becomes
				// slightly greater than the range and causes an overflow.  This hack prevents that.
				// It only seems to happen at the most extreme values so hopefully this won't lessen
				// accuracy too much.
				// The hack seems to create 15 degrees of deadzone at the trackpads' most farthest out
				// point on the x and y cardinals.
				// On further testing, the deadzone seems to be a problem with the controller and
				// not the algorithm.
				Math.Clamp(xp, Int16.MinValue, Int16.MaxValue);
				Math.Clamp(yp, Int16.MinValue, Int16.MaxValue);

				return ((short)xp, (short)yp);

				//short xp = Convert.ToInt16(x * Math.Cos(angle) - y * Math.Sin(angle));
				//short yp = Convert.ToInt16(x * Math.Sin(angle) + y * Math.Cos(angle));

				//return (xp, yp);
			}
		}
	}
}
