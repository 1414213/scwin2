using System;
using Newtonsoft.Json;

namespace Robot {
	public class Macro {

		public struct Move2<T> {
			public T x, y; public bool relatively;
			public override string ToString() => "(" + x + ", " + y + ", " + relatively + ")";
		}

		public struct Scroll {
			public int amount; public bool asClicks;
			public override string ToString() => "(" + amount + ", " + asClicks + ")";
		}

		private struct SteamController {
			public Key[] PressButtons, ReleaseButtons;
			public double PullLeftTrigger {
				get => pullLeftTrigger;
				init => pullLeftTrigger = value switch {
					< 0 or > 1 => throw new Backend.SettingInvalidException("PullLeftTrigger not within range [0, 1]."),
					_          => value
				};
			}
			public double PullRightTrigger {
				get => pullRightTrigger;
				init => pullRightTrigger = value switch {
					< 0 or > 1 => throw new Backend.SettingInvalidException("PullRightTrigger not within range [0, 1]."),
					_          => value
				};
			}
			public Move2<double> MoveStick {
				get => new Move2<double>{
					x = this.ShortToRatio(moveStick.x),
					y = this.ShortToRatio(moveStick.y),
					relatively = moveStick.relatively
				};
				init => moveStick = value switch {
					Move2<double> {x: < -1 or > 1, y: < -1 or > 1}
						=> throw new Backend.SettingInvalidException("Stick axes not within range [-1, 1]."),
					_	=> new Move2<short>{ x = this.RatioToShort(value.x),
					                         y = this.RatioToShort(value.y),
					                         relatively = value.relatively }
				};
			}
			public Move2<double> MoveLeftPad {
				get => new Move2<double>{
					x = this.ShortToRatio(moveLeftPad.x),
					y = this.ShortToRatio(moveLeftPad.y),
					relatively = moveLeftPad.relatively
				};
				init => moveLeftPad = value switch {
					Move2<double> {x: < -1 or > 1, y: < -1 or > 1}
						=> throw new Backend.SettingInvalidException("LeftPad not within range [-1, 1]."),
					_	=> new Move2<short>{ x = this.RatioToShort(value.x),
					                         y = this.RatioToShort(value.y),
					                         relatively = value.relatively }
				};
			}
			public Move2<double> MoveRightPad {
				get => new Move2<double>{
					x = this.ShortToRatio(moveRightPad.x),
					y = this.ShortToRatio(moveRightPad.y),
					relatively = moveRightPad.relatively
				};
				init => moveRightPad = value switch {
					Move2<double> {x: < -1 or > 1, y: < -1 or > 1}
						=> throw new Backend.SettingInvalidException("RightPad not within range [-1, 1]."),
					_	=> new Move2<short>{ x = this.RatioToShort(value.x),
					                         y = this.RatioToShort(value.y),
					                         relatively = value.relatively }
				};
			}

			public readonly Move2<short> moveStick, moveLeftPad, moveRightPad;

			private double pullLeftTrigger, pullRightTrigger;

			/// <summary>Ratio [-1d, 1d] to range of short.</summary>
			private short RatioToShort(double ratio) => (short)Math.Clamp(
				ratio * (ratio > 0 ? Int16.MaxValue : Int16.MinValue),
				Int16.MinValue,
				Int16.MaxValue);

			/// <summary>Range of short to [-1d, 1d].</summary>
			private double ShortToRatio(short n) => n switch {
				0 => 0,
				_ => Math.Clamp(n / (double)(n > 0 ? Int16.MaxValue : Int16.MinValue), -1, 1)
			};
		}

		public Key[] PressButtons = {};
		public Key[] ReleaseButtons = {};
		public Move2<int> MoveMouse = new() { x = 0, y = 0, relatively = true };
		public Scroll ScrollMouse = new() { amount = 0 };
		public double PullLeftTrigger {
			get => pullLeftTrigger / 255d;
			init => pullLeftTrigger = value switch {
				< 0 or > 1 => throw new Backend.SettingInvalidException("PullLeftTrigger not within range [0, 1]."),
				_          => (byte)(value * 255)
			};
		}
		public double PullRightTrigger {
			get => pullRightTrigger / 255d;
			init => pullRightTrigger = value switch {
				< 0 or > 1 => throw new Backend.SettingInvalidException("PullRightTrigger not within range [0, 1]."),
				_          => (byte)(value * 255)
			};
		}
		public Move2<double> MoveLeftStick {
			get => new Move2<double>{
				x = this.ShortToRatio(moveLeftStick.x),
				y = this.ShortToRatio(moveLeftStick.y),
				relatively = moveLeftStick.relatively
			};
			init => moveLeftStick = value switch {
				Move2<double> {x: < -1 or > 1, y: < -1 or > 1} => throw new Backend.SettingInvalidException(
					"Axes of MoveLeftStick not witin range [-1, 1]."),
				_ => new Move2<short>{
					x = this.RatioToShort(value.x),
					y = this.RatioToShort(value.y),
					relatively = value.relatively
				}
			};
		}
		public Move2<double> MoveRightStick {
			get => new Move2<double>{
				x = this.ShortToRatio(moveRightStick.x),
				y = this.ShortToRatio(moveRightStick.y),
				relatively = moveRightStick.relatively
			};
			init => moveLeftStick = value switch {
				Move2<double> {x: < -1 or > 1, y: < -1 or > 1} => throw new Backend.SettingInvalidException(
					"Axes of MoveRightStick not within range [-1, 1]."),
				_ => new Move2<short>{
					x = this.RatioToShort(value.x),
					y = this.RatioToShort(value.y),
					relatively = value.relatively }
			};
		}
		public string AddActionLayer = "", RemoveActionLayer = "";
		public bool AddActionLayerAsTransparent = true;
		public int Wait {
			get => wait;
			init => wait = value switch {
				< 0 => throw new Backend.SettingInvalidException("Wait must be a waitable time (> 0)."),
				_   => value
			};
		}

		public readonly Move2<short> moveLeftStick = new() { x = 0, y = 0, relatively = true };
		public readonly Move2<short> moveRightStick = new() { x = 0, y = 0, relatively = true };
		public readonly byte pullLeftTrigger, pullRightTrigger;

		private readonly int wait = 0;
		private const int shortRange = Int16.MaxValue - Int16.MinValue;

		public byte PullLeftTriggerAsByte() => pullLeftTrigger;

		public byte PullRightTriggerAsByte() => pullRightTrigger;

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

		public override string ToString() => PressButtonsToString + " " + ReleaseButtonsToString + " "
			+ MoveMouseToString + " "
			+ ScrollMouseToString + " "
			+ PullLeftTriggerToString + " "
			+ PullRightTriggerToString + " "
			+ MoveLeftStickToString + " "
			+ MoveRightStickToString + " "
			+ AddActionLayerToString + " "
			+ RemoveActionLayerToString + " "
			+ AddActionLayerAsTransparentToString + " "
			+ WaitToString;

		public string ToString(bool brief) {
			if (!brief) return this.ToString();
			else {
				var str = "";
				if (PressButtons.Length != 0)                             str += PressButtonsToString + " ";
				if (ReleaseButtons.Length != 0)                           str += ReleaseButtonsToString + " ";
				if (MoveMouse is not {x: 0, y: 0, relatively: true})      str += MoveMouseToString + " ";
				if (ScrollMouse is not {amount: 0})                       str += ScrollMouseToString + " ";
				if (PullLeftTrigger != 0)                                 str += PullLeftTriggerToString + " ";
				if (PullRightTrigger != 0)                                str += PullRightTriggerToString + " ";
				if (MoveLeftStick is not {x: 0, y: 0, relatively: true})  str += MoveLeftStickToString + " ";
				if (MoveRightStick is not {x: 0, y: 0, relatively: true}) str += MoveRightStickToString + " ";
				if (AddActionLayer != "")                                 str += AddActionLayerToString + " ";
				if (RemoveActionLayer != "")                              str += RemoveActionLayerToString + " ";
				if (AddActionLayer != "")
					str += AddActionLayerAsTransparentToString + " ";
				if (Wait > 0)                                             str += WaitToString + " ";
				str.TrimEnd();

				return str;
			}
		}

		string PressButtonsToString {
			get {
				var str = "PressButtons: [";
				for (int i = 0; i < PressButtons.Length - 1; i++) str += PressButtons[i].ToString() + ", ";
				str += PressButtons[PressButtons.Length - 1].ToString() + "]";
				return str;
			}
		}
		string ReleaseButtonsToString {
			get {
				var str = "ReleaseButtons: [";
				for (int i = 0; i < ReleaseButtons.Length - 1; i++) str += ReleaseButtons[i].ToString() + ", ";
				str += ReleaseButtons[ReleaseButtons.Length - 1].ToString() + "]";
				return str;
			}
		}
		string MoveMouseToString => "MoveMouse: " + MoveMouse.ToString();
		string ScrollMouseToString => "ScrollMouse: " + ScrollMouse.ToString();
		string PullLeftTriggerToString => "PullLeftTrigger: " + PullLeftTrigger.ToString();
		string PullRightTriggerToString => "PullRightTrigger: " + PullRightTrigger.ToString();
		string MoveLeftStickToString => "MoveLeftStick: " + MoveLeftStick.ToString();
		string MoveRightStickToString => "MoveRightStick: " + moveRightStick.ToString();
		string AddActionLayerToString => "AddActionLayer: " + AddActionLayer;
		string RemoveActionLayerToString => "RemoveActionLayer: " + RemoveActionLayer;
		string AddActionLayerAsTransparentToString => "AddActionLayerAsTransparent: " + AddActionLayerAsTransparent;
		string WaitToString => "Wait: " + Wait;
	}
}