using System;
using System.IO;


namespace Robot {
	public interface IRobot {
		int ScrollWheelClickSize { get; }
		(short x, short y) LStickPosition { get; }
		(short x, short y) RStickPosition { get; }

		void MoveMouse(long x, long y, bool relative = false);
		void MoveMouse(int x, int y, bool relative = false);
		void ScrollMouseWheel(int amount, bool asClicks = false);
		void Press(params Key[] keys);
		void Release(params Key[] keys);
		void MoveLStick(short x, short y, bool relative = false) {
			if (relative) {
				MoveLStickX((short)(x + LStickPosition.x));
				MoveLStickY((short)(y + LStickPosition.y));
			} else {
				MoveLStickX(x);
				MoveLStickY(y);
			}
		}

		/// <summary>Must set the field implementing LStickPosition to its given position</summary>
		void MoveLStickX(short x);
		/// <summary>Must set the field implementing LStickPosition to its given position</summary>
		void MoveLStickY(short y);
		void MoveRStick(short x, short y, bool relative = false) {
			if (relative) {
				MoveRStickX((short)(x + RStickPosition.x));
				MoveRStickY((short)(y + RStickPosition.y));
			} else {
				MoveRStickX(x);
				MoveRStickY(y);
			}
		}

		/// <summary>Must set the field implementing LStickPosition to its given position</summary>		
		void MoveRStickX(short x);
		/// <summary>Must set the field implementing LStickPosition to its given position</summary>
		void MoveRStickY(short y);
		void PullLTrigger(byte amount);
		void PullRTrigger(byte amount);

		void DoMacro(Macro macro) {
			//Console.WriteLine(macro.ToString(brief: true));
			this.Press(macro.PressButtons);
			this.Release(macro.ReleaseButtons);
			if (macro.MoveMouse is not {x: 0, y: 0, relatively: true}) {
				this.MoveMouse(macro.MoveMouse.x, macro.MoveMouse.y, macro.MoveMouse.relatively);
			} else if (macro.ScrollMouse is not {amount: 0}) {
				this.ScrollMouseWheel(macro.ScrollMouse.amount, macro.ScrollMouse.asClicks);
			} else if (macro.PullLeftTrigger != 0) {
				this.PullLTrigger(macro.pullLeftTrigger);
			} else if (macro.PullRightTrigger != 0) {
				this.PullRTrigger(macro.pullRightTrigger);
			} else if (macro.moveLeftStick is not {x: 0, y: 0, relatively: true}) {
				this.MoveLStick(macro.moveLeftStick.x, macro.moveLeftStick.y, macro.moveLeftStick.relatively);
			} else if (macro.moveRightStick is not {x: 0, y: 0, relatively: true}) {
				this.MoveRStick(macro.moveRightStick.x, macro.moveRightStick.y, macro.moveRightStick.relatively);
			}
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
		GamepadHome,
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
	}
}