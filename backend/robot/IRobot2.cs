using System;
using System.IO;


namespace Robot {
	public interface IRobot {
		int ScrollWheelClickSize { get; }

		void MoveMouse(long x, long y, bool relative = false);
		void MoveMouse(int x, int y, bool relative = false);
		void ScrollMouseWheel(int amount, bool asClicks = false);
		void Press(params Key[] keys);
		void Release(params Key[] keys);
		void MoveLStick(short x, short y) {
			MoveLStickX(x);
			MoveLStickY(y);
		}

		void MoveLStickX(short x);
		void MoveLStickY(short y);
		void MoveRStick(short x, short y) {
			MoveRStickX(x);
			MoveRStickY(y);
		}
		
		void MoveRStickX(short x);
		void MoveRStickY(short y);
		void PullLTrigger(byte amount);
		void PullRTrigger(byte amount);
		void DoMacro(params InputEvent[] events) {
			foreach (InputEvent e in events) {
				if ((e.types & InputEvent.Types.None) == InputEvent.Types.None) continue;
				else if ((e.types & InputEvent.Types.Key) == InputEvent.Types.Key) {
					if (e.isPressElseRelease) this.Press(e.keys);
					else this.Release(e.keys);
				} else if ((e.types & InputEvent.Types.Key) == InputEvent.Types.MouseMove) {
					this.MoveMouse(e.coordinates.x, e.coordinates.y, relative: e.isRelative);
				} else if ((e.types & InputEvent.Types.MouseScroll) == InputEvent.Types.MouseScroll) {
					this.ScrollMouseWheel(e.scrollAmount, asClicks: e.scrollAsClicks);
				} else if ((e.types & InputEvent.Types.TriggerPull) == InputEvent.Types.TriggerPull) {
					if (e.isLeftElseRight) this.PullLTrigger(e.pullDistance);
					else this.PullRTrigger(e.pullDistance);
				} else if ((e.types & InputEvent.Types.ThumbstickMove) == InputEvent.Types.ThumbstickMove) {
					if (e.isLeftElseRight)
						this.MoveLStick((short)e.coordinates.x, (short)e.coordinates.y);
					else this.MoveRStick((short)e.coordinates.x, (short)e.coordinates.y);
				}
			}
		}

		void DoMacro(Macro[] macros) {
			foreach (var m in macros) this.DoMacro(m);
		}

		void DoMacro(Macro macro) {
			this.Press(macro.PressButtons);
			this.Release(macro.ReleaseButtons);
			this.MoveMouse(macro.MoveMouse.x, macro.MoveMouse.y, macro.MoveMouse.relative);
			this.ScrollMouseWheel(macro.ScrollMouse.amount, macro.ScrollMouse.asClicks);
			this.PullLTrigger(macro.PullLeftTrigger);
			this.PullRTrigger(macro.PullRightTrigger);
			this.MoveLStick(macro.MoveLeftStickTo.x, macro.MoveLeftStickTo.y);
			this.MoveRStick(macro.MoveRightStickTo.x, macro.MoveRightStickTo.y);
		}

		public static void PrintKeycodes(string directory) {
			StreamWriter file = new StreamWriter(directory + "RobotKeycodes.txt");
			file.WriteLine("Mouse Keycodes:");

			foreach (Key key in Enum.GetValues(typeof(Key))) {
				file.WriteLine(key.ToString() + "\t\t\t\t\t" + (int)key);
				// if (key == Key.MouseFive) {
				// 	file.WriteLine();
				// 	file.WriteLine("Keyboard Keycodes:");
				// }
				// else if (key == Key.Pad_Period) {
				// 	file.WriteLine();
				// 	file.WriteLine("Gamepad Keycodes:");
				// }
			}
		}
	}

	public enum Key {
		None = 0,
		MouseLeft = 1,
		MouseRight,
		MouseMiddle,
		MouseFour,
		MouseFive,
		Tab = '\t', // int value of 9
		Space = ' ',
		Quotation = '\'',
		Comma = ',',
		Dash = '-',
		Dot = '.',
		ForwardSlash = '/',
		Row_0 = '0',
		Row_1 = '1',
		Row_2 = '2',
		Row_3 = '3',
		Row_4 = '4',
		Row_5 = '5',
		Row_6 = '6',
		Row_7 = '7',
		Row_8 = '8',
		Row_9 = '9',
		Semicolon = ';',
		Equal = '=',
		A = 'A',
		B = 'B',
		C = 'C',
		D = 'D',
		E = 'E',
		F = 'F',
		G = 'G',
		H = 'H',
		I = 'I',
		J = 'J',
		K = 'K',
		L = 'L',
		M = 'M',
		N = 'N',
		O = 'O',
		P = 'P',
		Q = 'Q',
		R = 'R',
		S = 'S',
		T = 'T',
		U = 'U',
		V = 'V',
		W = 'W',
		X = 'X',
		Y = 'Y',
		Z = 'Z',
		OpenBracket = '[',
		Backslash = '\\',
		CloseBracket = ']',
		Grave = '`',
		Escape,
		F1,
		F2,
		F3,
		F4,
		F5,
		F6,
		F7,
		F8,
		F9,
		F10,
		F11,
		F12,
		Backspace,
		CapsLock,
		Enter,
		LeftControl,
		RightControl,
		LeftSystem, //note: no corresponding right key
		LeftAlternate,
		RightAlternate,
		LeftShift,
		RightShift,
		Insert,
		Home,
		PageUp,
		PageDown,
		Delete,
		End,
		LeftArrow,
		UpArrow,
		RightArrow,
		DownArrow,
		NumberLock,
		Pad_Backslash,
		Pad_Star,
		Pad_Dash,
		Pad_Add,
		Pad_Enter,
		Pad_0,
		Pad_1,
		Pad_2,
		Pad_3,
		Pad_4,
		Pad_5,
		Pad_6,
		Pad_7,
		Pad_8,
		Pad_9,
		Pad_Period, // int value of 146
		Face_East,
		Face_North,
		Face_West,
		Face_South,
		Dpad_Left,
		Dpad_Up,
		Dpad_Right,
		Dpad_Down,
		LStickClick,
		RStickClick,
		LBumper,
		RBumper,
		Start,
		Back,
		GamepadHome,
	}
}