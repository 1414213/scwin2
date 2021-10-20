using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Backend {
	public class ButtonMacro : Button {
		public enum ButtonState { None = 0, Press = 1, Releaase = 2 }

		public record Macro : Robot.Macro {
			public struct Movement2 {
				public double X => this.ShortToRatio(vector.x);
				public double Y => this.ShortToRatio(vector.y);
				public readonly (short x, short y) vector;
				public readonly ButtonState state;

				public Movement2(short x, short y, ButtonState state) { this.vector = (x, y); this.state = state; }

				public Movement2(double x, double y, ButtonState state, string errorFieldName) : this(0, 0, 0) {
					var error = errorFieldName + " must have an (x, y) within the range [-1, 1].";
					if (x is < -1 or > 1 || y is < -1 or > 1) throw new SettingInvalidException(error);
					this = new Movement2(RatioToShort(x), RatioToShort(y), state);
				}

				public static implicit operator (short x, short y)(Movement2 movement2) => movement2.vector;

				/// <summary>Ratio [-1d, 1d] to range of short.</summary>
				private short RatioToShort(double ratio) {
					var value = ratio * (ratio > 0 ? Int16.MaxValue : Int16.MinValue);
					return (short)Math.Clamp(value, Int16.MinValue, Int16.MaxValue);
				}

				/// <summary>Range of short to [-1d, 1d].</summary>
				private double ShortToRatio(short n) {
					if (n == 0) return 0;
					var value = n / (double)(n > 0 ? Int16.MaxValue : Int16.MinValue);
					return Math.Clamp(value, -1d, 1d);
				}
			}

			public new Key[] PressButtons {
				get => pressButtons;
				init {
					pressButtons = value;
					var robotButtons = new List<Robot.Key>();
					foreach (var b in pressButtons) if (b.IsRobotKey()) robotButtons.Add((Robot.Key)(int)b);
					base.PressButtons = robotButtons.ToArray();
				}
			}
			public new Key[] ReleaseButtons {
				get => releaseButtons;
				init {
					releaseButtons = value;
					var robotButtons = new List<Robot.Key>();
					foreach (var b in releaseButtons) if (b.IsRobotKey()) robotButtons.Add((Robot.Key)(int)b);
					base.ReleaseButtons = robotButtons.ToArray();
				}
			}
			public string AddActionLayer = "", RemoveActionLayer = "";
			public bool AddActionLayerAsTransparent = true;
			public (double x, double y, ButtonState state)? LeftPadTouch {
				get => leftPadTouch is null
					? null
					: (leftPadTouch.Value.position.X, leftPadTouch.Value.position.Y, leftPadTouch.Value.state);
				init => leftPadTouch = value is null
					? null
					: (new Movement2(value.Value.x, value.Value.y, "LeftPadTouch"), value.Value.state);
			}
			public (double x, double y, ButtonState state)? RightPadTouch;
			public (double x, double y, ButtonState state)? LeftPadClick;
			public (double x, double y, ButtonState state)? RightPadClick;
			public Movement2? MoveStick;

			private (Movement2 position, ButtonState state)? leftPadTouch, rightPadTouch, leftPadClick, rightPadClick;

			private Key[] pressButtons = {}, releaseButtons = {};

			public override string ToString() => PressButtonsToString() + " "
				+ ReleaseButtonsToString() + " "
				+ "MoveMouse: " + MoveMouse.ToString() + " "
				+ "ScrollMouse: " + ScrollMouse.ToString() + " "
				+ "PullLeftTrigger: " + PullLeftTrigger.ToString() + " "
				+ "PullRightTrigger: " + PullRightTrigger.ToString() + " "
				+ "MoveLeftStick: " + MoveLeftStick.ToString() + " "
				+ "MoveRightStick: " + MoveRightStick.ToString() + " "
				+ "AddActionLayer: " + AddActionLayer
				+ "RemoveActionLayer: " + RemoveActionLayer
				+ "AddActionLayerAsTransparent: " + AddActionLayerAsTransparent
				+ "Wait: " + Wait;

			public new string ToString(bool brief) {
				if (!brief) return this.ToString();
				else {
					var str = "";
					if (PressButtons.Length != 0) {
						str += PressButtonsToString() + " ";
					} else if (ReleaseButtons.Length != 0) {
						str += ReleaseButtonsToString() + " ";
					} else if (MoveMouse is not {x: 0, y: 0, relatively: true}) {
						str += "MoveMouse: " + MoveMouse.ToString() + " ";
					} else if (ScrollMouse is not {amount: 0}) {
						str += "ScrollMouse: " + ScrollMouse.ToString() + " ";
					} else if (PullLeftTrigger != 0) {
						str += "PullLeftTrigger: " + PullLeftTrigger.ToString() + " ";
					} else if (PullRightTrigger != 0) {
						str += "PullRightTrigger: " + PullRightTrigger.ToString() + " ";
					} else if (MoveLeftStick is not {x: 0, y: 0, relatively: true}) {
						str += "MoveLeftStick: " + MoveLeftStick.ToString() + " ";
					} else if (MoveRightStick is not {x: 0, y: 0, relatively: true}) {
						str += "MoveRightStick: " + MoveRightStick.ToString() + " ";
					} else if (AddActionLayer != "") {
						str += "AddActionLayer: " + AddActionLayer + " "
							+ "AddActionLayerAsTransparent: " + AddActionLayerAsTransparent;
					} else if (RemoveActionLayer != "") {
						str += "RemoveActionLayer: " + RemoveActionLayer + " ";
					} else if (Wait > 0) {
						str += "Wait: " + Wait + " ";
					}
					str.TrimEnd();
					return str;
				}
			}
		}

		public Macro[] Pressed { get; set; } = {};
		public Macro[] Held { get; set; } = {};
		public Macro[] Released { get; set; } = {};
		public int RepetitionsPerSecond { get => 1000 / waitTime; set {
			int waitTime = 1000 / value;
			if (waitTime < 0) throw new SettingInvalidException("RepetitionsPerSecond must be > 0.");
			this.waitTime = waitTime;
		} }

		private int waitTime = 10;
		private Task onPress = null!, onHold = null!;
		private CancellationTokenSource cancelOnHold = null!;

		protected override void PressImpl() {
			onPress = Task.Run(() => this.DoMacros(Pressed));
			this.cancelOnHold = new CancellationTokenSource();
			onHold = Task.Run(() => {
				while (true) {
					this.DoMacros(Held);
					Thread.Sleep(waitTime);
					cancelOnHold.Token.ThrowIfCancellationRequested();
				}
			}, cancelOnHold.Token);
		}

		protected override void ReleaseImpl() {
			cancelOnHold.Cancel();
			Task.Run(() => {
				onPress.Wait();
				this.DoMacros(Released);
			});
		}

		private void DoMacros(Macro[] macros) {
			foreach (var macro in macros) {
				robot.DoMacro(macro as Robot.Macro);
				if (macro.AddActionLayer is string aal) {
					EventDoer.AddActionLayer(aal, macro.AddActionLayerAsTransparent);
				}
				if (macro.RemoveActionLayer is string ral) EventDoer.RemoveActionLayer(ral);
				if (macro.LeftPadTouch is not null) {
					EventDoer.DoEvents({new SteamControllerApi.TrackpadData(
						SteamControllerApi.Key.LPadTouch,
						(macro.LeftPadTouch.Value.x, macro.LeftPadTouch.Value.y),
						SteamControllerApi.Flags.Pressed,
						null)});
				}

				Thread.Sleep(macro.Wait);
			}
		}

		public enum Key {
			None = Robot.Key.None,
			MouseLeft = Robot.Key.MouseLeft,
			MouseRight = Robot.Key.MouseRight,
			MouseMiddle = Robot.Key.MouseMiddle,
			MouseFour = Robot.Key.MouseFour,
			MouseFive = Robot.Key.MouseFive,
			Tab = Robot.Key.Tab, // int value of 9
			Space = Robot.Key.Space,
			Quotation = Robot.Key.Quotation,
			Comma = Robot.Key.Comma,
			Dash = Robot.Key.Dash,
			Dot = Robot.Key.Dot,
			ForwardSlash = Robot.Key.ForwardSlash,
			Row_0 = Robot.Key.Row_0,
			Row_1 = Robot.Key.Row_1,
			Row_2 = Robot.Key.Row_2,
			Row_3 = Robot.Key.Row_3,
			Row_4 = Robot.Key.Row_4,
			Row_5 = Robot.Key.Row_5,
			Row_6 = Robot.Key.Row_6,
			Row_7 = Robot.Key.Row_7,
			Row_8 = Robot.Key.Row_8,
			Row_9 = Robot.Key.Row_9,
			Semicolon = Robot.Key.Semicolon,
			Equal = Robot.Key.Equal,
			A = Robot.Key.A,
			B = Robot.Key.B,
			C = Robot.Key.C,
			D = Robot.Key.D,
			E = Robot.Key.D,
			F = Robot.Key.F,
			G = Robot.Key.G,
			H = Robot.Key.H,
			I = Robot.Key.I,
			J = Robot.Key.J,
			K = Robot.Key.K,
			L = Robot.Key.L,
			M = Robot.Key.M,
			N = Robot.Key.N,
			O = Robot.Key.O,
			P = Robot.Key.P,
			Q = Robot.Key.Q,
			R = Robot.Key.R,
			S = Robot.Key.S,
			T = Robot.Key.T,
			U = Robot.Key.U,
			V = Robot.Key.V,
			W = Robot.Key.W,
			X = Robot.Key.X,
			Y = Robot.Key.Y,
			Z = Robot.Key.Z,
			OpenBracket = Robot.Key.OpenBracket,
			Backslash = Robot.Key.Backslash,
			CloseBracket = Robot.Key.CloseBracket,
			Grave = Robot.Key.Grave,
			Escape = Robot.Key.Escape,
			F1 = Robot.Key.F1,
			F2 = Robot.Key.F2,
			F3 = Robot.Key.F3,
			F4 = Robot.Key.F4,
			F5 = Robot.Key.F5,
			F6 = Robot.Key.F6,
			F7 = Robot.Key.F7,
			F8 = Robot.Key.F8,
			F9 = Robot.Key.F9,
			F10 = Robot.Key.F10,
			F11 = Robot.Key.F11,
			F12 = Robot.Key.F12,
			Backspace = Robot.Key.Backspace,
			CapsLock = Robot.Key.CapsLock,
			Enter = Robot.Key.Enter,
			LeftControl = Robot.Key.LeftControl,
			RightControl = Robot.Key.RightControl,
			LeftSystem = Robot.Key.LeftSystem, //note: no corresponding right key
			LeftAlternate = Robot.Key.LeftAlternate,
			RightAlternate = Robot.Key.RightAlternate,
			LeftShift = Robot.Key.LeftShift,
			RightShift = Robot.Key.RightShift,
			Insert = Robot.Key.Insert,
			Home = Robot.Key.Home,
			PageUp = Robot.Key.PageUp,
			PageDown = Robot.Key.PageDown,
			Delete = Robot.Key.Delete,
			End = Robot.Key.End,
			LeftArrow = Robot.Key.LeftArrow,
			UpArrow = Robot.Key.UpArrow,
			RightArrow = Robot.Key.RightArrow,
			DownArrow = Robot.Key.DownArrow,
			NumberLock = Robot.Key.NumberLock,
			Pad_Backslash = Robot.Key.Pad_Backslash,
			Pad_Star = Robot.Key.Pad_Star,
			Pad_Dash = Robot.Key.Pad_Dash,
			Pad_Add = Robot.Key.Pad_Add,
			Pad_Enter = Robot.Key.Pad_Enter,
			Pad_0 = Robot.Key.Pad_0,
			Pad_1 = Robot.Key.Pad_1,
			Pad_2 = Robot.Key.Pad_2,
			Pad_3 = Robot.Key.Pad_3,
			Pad_4 = Robot.Key.Pad_4,
			Pad_5 = Robot.Key.Pad_5,
			Pad_6 = Robot.Key.Pad_6,
			Pad_7 = Robot.Key.Pad_7,
			Pad_8 = Robot.Key.Pad_8,
			Pad_9 = Robot.Key.Pad_9,
			Pad_Period = Robot.Key.Pad_Period, // int value of 146
			GamepadHome = Robot.Key.GamepadHome,
			Face_East = Robot.Key.Face_East,
			Face_North = Robot.Key.Face_North,
			Face_West = Robot.Key.Face_West,
			Face_South = Robot.Key.Face_South,
			Dpad_Left = Robot.Key.Dpad_Left,
			Dpad_Up = Robot.Key.Dpad_Up,
			Dpad_Right = Robot.Key.Dpad_Right,
			Dpad_Down = Robot.Key.Dpad_Down,
			LStickClick = Robot.Key.LStickClick,
			RStickClick = Robot.Key.RStickClick,
			LBumper = Robot.Key.LBumper,
			RBumper = Robot.Key.RBumper,
			Start = Robot.Key.Start,
			Back = Robot.Key.Back,
			Steam_Home,
			Steam_East,
			Steam_North,
			Steam_West,
			Steam_South,
			Steam_StickClick,
			Steam_LBumper,
			Steam_RBumper,
			Steam_LTriggerClick,
			Steam_RTriggerClick,
			Steam_LGripClick,
			Steam_RGripClick,
			Steam_Left,
			Steam_Right,
		}
	}

	public static class MacroExtensions {
		public static bool IsRobotKey(this ButtonMacro.Key key) => key switch {
			ButtonMacro.Key.Steam_Home          => false,
			ButtonMacro.Key.Steam_East          => false,
			ButtonMacro.Key.Steam_North         => false,
			ButtonMacro.Key.Steam_West          => false,
			ButtonMacro.Key.Steam_South         => false,
			ButtonMacro.Key.Steam_StickClick    => false,
			ButtonMacro.Key.Steam_LBumper       => false,
			ButtonMacro.Key.Steam_RBumper       => false,
			ButtonMacro.Key.Steam_LTriggerClick => false,
			ButtonMacro.Key.Steam_RTriggerClick => false,
			ButtonMacro.Key.Steam_LGripClick    => false,
			ButtonMacro.Key.Steam_RGripClick    => false,
			ButtonMacro.Key.Steam_Left          => false,
			ButtonMacro.Key.Steam_Right         => false,
			_ => true,
		};
	}
}