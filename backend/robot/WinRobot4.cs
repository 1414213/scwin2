using System;

using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using WindowsInput;
using WindowsInput.Native;


namespace Robot {
	public class WindowsRobot : IRobot {
		public int ScrollWheelClickSize { get; } = 120;

		private InputSimulator sim = new InputSimulator();
		// In this case throwing a null reference error when the event doer attempts to send gamepad input
		// when the gamepad was commanded to not initialize is wanted since if there is no virtual gamepad
		// to send input to then the event doer can't really do much with the keybinding.
		private IXbox360Controller virtualGamepad = null!;

		public WindowsRobot(bool createVirtualGamepad = true) {
			if (createVirtualGamepad) {
				ViGEmClient client = new ViGEmClient();
				this.virtualGamepad = client.CreateXbox360Controller();
				this.virtualGamepad.Connect();
			}
		}
		~WindowsRobot() {
			if (this.virtualGamepad != null) this.virtualGamepad.Disconnect();
		}

		// A bug/oversite prevents InputSimulator from sending mouse movement as long integers despite Windows
		// supporting this.
		// Shoud be easy to fix, just fork the project and change the INPUT struct to store x and y as
		// long instead of the current int.
		public void MoveMouse(long x, long y, bool relative = false) {
			throw new NotImplementedException("Use MoveMouse(int, int, bool) instead.");
		}

		/// <summary>
		/// Absolute movement value must be the size of a unsigned 16 bit integer (ushort).
		/// </summary>
		public void MoveMouse(int x, int y, bool relative = false) {
			if (relative) {
				this.sim.Mouse.MoveMouseBy(x, -y);
			}
			else {
				if (x < 0 || y < 0 || x > 0xFFFF || y > 0xFFFF) {
					throw
					new ArgumentException("Absolute movement values must be between 0 and 0xFFFF (65,535)");
				}
				this.sim.Mouse.MoveMouseTo((double)x, -(double)y);
			}
		}

		public void ScrollMouseWheel(int amount, bool asClicks = false) {
			if (asClicks) this.sim.Mouse.VerticalScroll(amount * this.ScrollWheelClickSize);
			else this.sim.Mouse.VerticalScroll(amount);
		}

		public void Press(params Key[] keys) {
			foreach(Key k in keys) {
				if (k == Key.None) continue;
				if (k == Key.MouseLeft) this.sim.Mouse.LeftButtonDown();
				else if (k == Key.MouseRight) this.sim.Mouse.RightButtonDown();
				else if (k == Key.MouseMiddle) this.sim.Mouse.MiddleButtonDown();
				else if (k == Key.MouseFour) this.sim.Mouse.XButtonDown(1);
				else if (k == Key.MouseFive) this.sim.Mouse.XButtonDown(2);
				else if ((int)k >= (int)Key.Tab && (int)k <= (int)Key.Pad_Period)
					this.sim.Keyboard.KeyDown(k.ToVirtualKeyCode());
				else if ((int)k >= (int)Key.Face_South)
					this.virtualGamepad.SetButtonState(k.ToXbox360Button(), true);
			}
		}

		public void Release(params Key[] keys) {
			foreach(Key k in keys) {
				if (k == Key.None) continue;
				if (k == Key.MouseLeft) this.sim.Mouse.LeftButtonUp();
				else if (k == Key.MouseRight) this.sim.Mouse.RightButtonUp();
				else if (k == Key.MouseMiddle) this.sim.Mouse.MiddleButtonUp();
				else if (k == Key.MouseFour) this.sim.Mouse.XButtonUp(1);
				else if (k == Key.MouseFive) this.sim.Mouse.XButtonUp(2);
				else if ((int)k >= (int)Key.Tab && (int)k <= (int)Key.Pad_Period)
					this.sim.Keyboard.KeyUp(k.ToVirtualKeyCode());
				else if ((int)k >= (int)Key.Face_South)
					this.virtualGamepad.SetButtonState(k.ToXbox360Button(), false);
			}
		}

		public void Press(params int[] keycodes) {
			Key[] keys = new Key[keycodes.Length];
			int i = 0;
			foreach (int k in keycodes) {
				if (k > (int)Key.Face_South)
					throw new ArgumentException("Keycode " + k + " could not be reasoned as a key.");
				else keys[i] = (Key)k;
				i++;
			}
			this.Press(keys);
		}

		public void Release(params int[] keycodes) {
			Key[] keys = new Key[keycodes.Length];
			int i = 0;
			foreach (int k in keycodes) {
				if (k > (int)Key.Face_South)
					throw new ArgumentException("Keycode " + k + " could not be reasoned as a key.");
				else keys[i] = (Key)k;
				i++;
			}
			this.Release(keys);
		}

		public void MoveLStickX(short x) => virtualGamepad.SetAxisValue(Xbox360Axis.LeftThumbX, x);

		public void MoveLStickY(short y) => virtualGamepad.SetAxisValue(Xbox360Axis.LeftThumbY, y);

		public void MoveRStickX(short x) => virtualGamepad.SetAxisValue(Xbox360Axis.RightThumbX, x);

		public void MoveRStickY(short y) => virtualGamepad.SetAxisValue(Xbox360Axis.RightThumbY, y);

		public void PullLTrigger(byte amount) {
			this.virtualGamepad.SetSliderValue(Xbox360Slider.LeftTrigger, amount);
		}

		public void PullRTrigger(byte amount) {
			this.virtualGamepad.SetSliderValue(Xbox360Slider.RightTrigger, amount);
		}
	}

	static class KeyExtensions {
		public static VirtualKeyCode ToVirtualKeyCode(this Key k) {
			if ((int)k <= (int)Key.MouseFive || (int)k >= (int)Key.Face_South) {
				throw new ArgumentException($"{k} isn't a keyboard key.");
			}
			else if (((int)k >= (int)Key.Row_0) && ((int)k <= (int)Key.Row_9))
				return (VirtualKeyCode)((int)k);
			else if (((int)k >= (int)Key.A) && ((int)k <= (int)Key.Z))
				return (VirtualKeyCode)((int)k);
			else return k switch {
				Key.Tab            => VirtualKeyCode.TAB,
				Key.Space          => VirtualKeyCode.SPACE,
				Key.Quotation      => VirtualKeyCode.OEM_7,
				Key.Comma          => VirtualKeyCode.OEM_COMMA,
				Key.Dash           => VirtualKeyCode.OEM_MINUS,
				Key.Dot            => VirtualKeyCode.OEM_PERIOD,
				Key.ForwardSlash   => VirtualKeyCode.OEM_2,
				Key.Semicolon      => VirtualKeyCode.OEM_1,
				Key.Equal          => VirtualKeyCode.OEM_PLUS, // might be wrong idk
				Key.OpenBracket    => VirtualKeyCode.OEM_4,
				Key.Backslash      => VirtualKeyCode.OEM_5,
				Key.CloseBracket   => VirtualKeyCode.OEM_6,
				Key.Grave          => VirtualKeyCode.OEM_3,
				Key.Escape         => VirtualKeyCode.ESCAPE,
				Key.F1             => VirtualKeyCode.F1,
				Key.F2             => VirtualKeyCode.F2,
				Key.F3             => VirtualKeyCode.F3,
				Key.F4             => VirtualKeyCode.F4,
				Key.F5             => VirtualKeyCode.F5,
				Key.F6             => VirtualKeyCode.F6,
				Key.F7             => VirtualKeyCode.F7,
				Key.F8             => VirtualKeyCode.F8,
				Key.F9             => VirtualKeyCode.F9,
				Key.F10            => VirtualKeyCode.F10,
				Key.F11            => VirtualKeyCode.F11,
				Key.F12            => VirtualKeyCode.F12,
				Key.Backspace      => VirtualKeyCode.BACK,
				Key.CapsLock       => VirtualKeyCode.CAPITAL,
				Key.Enter          => VirtualKeyCode.RETURN,
				Key.LeftControl    => VirtualKeyCode.LCONTROL,
				Key.LeftSystem     => VirtualKeyCode.LWIN,
				// left and right alt seem to use the same keycode
				Key.LeftAlternate  => VirtualKeyCode.MENU,
				Key.LeftShift      => VirtualKeyCode.LSHIFT,
				Key.RightControl   => VirtualKeyCode.RCONTROL,
				// left and right alt seem to use the same keycode
				Key.RightAlternate => VirtualKeyCode.MENU,
				Key.RightShift     => VirtualKeyCode.RSHIFT,
				Key.Insert         => VirtualKeyCode.INSERT,
				Key.Home           => VirtualKeyCode.HOME,
				Key.PageUp         => VirtualKeyCode.PRIOR,
				Key.PageDown       => VirtualKeyCode.NEXT,
				Key.Delete         => VirtualKeyCode.DELETE,
				Key.End            => VirtualKeyCode.END,
				Key.LeftArrow      => VirtualKeyCode.LEFT,
				Key.UpArrow        => VirtualKeyCode.UP,
				Key.DownArrow      => VirtualKeyCode.DOWN,
				Key.RightArrow     => VirtualKeyCode.RIGHT,
				Key.NumberLock     => VirtualKeyCode.NUMLOCK,
				Key.Pad_Backslash  => VirtualKeyCode.DIVIDE,
				Key.Pad_Star       => VirtualKeyCode.MULTIPLY,
				Key.Pad_Dash       => VirtualKeyCode.SUBTRACT,
				Key.Pad_Add        => VirtualKeyCode.ADD,
				// can't find the keycode for this one
				//case Keyboard::Pad_Enter: return num
				Key.Pad_0          => VirtualKeyCode.NUMPAD0,
				Key.Pad_1          => VirtualKeyCode.NUMPAD1,
				Key.Pad_2          => VirtualKeyCode.NUMPAD2,
				Key.Pad_3          => VirtualKeyCode.NUMPAD3,
				Key.Pad_4          => VirtualKeyCode.NUMPAD4,
				Key.Pad_5          => VirtualKeyCode.NUMPAD5,
				Key.Pad_6          => VirtualKeyCode.NUMPAD6,
				Key.Pad_7          => VirtualKeyCode.NUMPAD7,
				Key.Pad_8          => VirtualKeyCode.NUMPAD8,
				Key.Pad_9          => VirtualKeyCode.NUMPAD9,
				Key.Pad_Period     => VirtualKeyCode.DECIMAL,
				_ => throw new ArgumentException($"Couldn't convert {k} into a keycode")
			};
		}

		public static Xbox360Button ToXbox360Button(this Key k) {
			return k switch {
				Key.LBumper     => Xbox360Button.LeftShoulder,
				Key.RBumper     => Xbox360Button.RightShoulder,
				Key.Dpad_Right  => Xbox360Button.Right,
				Key.Dpad_Up     => Xbox360Button.Up,
				Key.Dpad_Left   => Xbox360Button.Left,
				Key.Dpad_Down   => Xbox360Button.Down,
				Key.Back        => Xbox360Button.Back,
				Key.Start       => Xbox360Button.Start,
				Key.LStickClick => Xbox360Button.LeftThumb,
				Key.RStickClick => Xbox360Button.RightThumb,
				Key.Face_East   => Xbox360Button.B,
				Key.Face_North  => Xbox360Button.Y,
				Key.Face_West   => Xbox360Button.X,
				Key.Face_South  => Xbox360Button.A,
				_  => throw new ArgumentException($"Couldn't convert {k} into a Xbox gamepad button.")
			};
		}
	}
}