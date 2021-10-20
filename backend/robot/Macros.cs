using System;
using Newtonsoft.Json;

namespace Robot {
	public class SettingInvalidException : Exception {
		public SettingInvalidException() {}
		public SettingInvalidException(string message) : base(message) {}
		public SettingInvalidException(string message, Exception inner) : base(message, inner) {}
	}

	public struct Movement2 {
		public double X => this.ShortToRatio(vector.x);
		public double Y => this.ShortToRatio(vector.y);
		public readonly (short x, short y) vector;
		public readonly bool relatively;

		public Movement2(short x, short y, bool relatively) { this.vector = (x, y); this.relatively = relatively; }

		public Movement2(double x, double y, bool relatively, string errorFieldName) : this(0, 0, false) {
			var error = errorFieldName + " must have an (x, y) within the range [-1, 1].";
			if (x is < -1 or > 1 || y is < -1 or > 1) throw new SettingInvalidException(error);
			this = new Movement2(RatioToShort(x), RatioToShort(y), relatively);
		}

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

		public override string ToString() => "Movement2 { vector = " + vector + ", relatively = " + relatively + " }";
	}

	public record Macro {

		public struct MouseMovement {
			public int x, y; public bool relatively;
			public override string ToString() =>
				"MoveMouse { x = " + x + ", y = " + y + ", relatively = " + relatively + " }";
		}

		public struct MouseScroll {
			public int amount; public bool asClicks;
			public override string ToString() => "Scroll { amount = " + amount + ", asClicks = " + asClicks + " }";
		}

		public Key[] PressButtons = {};
		public Key[] ReleaseButtons = {};
		public MouseMovement MoveMouse = new MouseMovement{ x = 0, y = 0, relatively = true };
		public MouseScroll ScrollMouse = new MouseScroll{ amount = 0 };
		public double PullLeftTrigger {
			get => pullLeftTrigger / 255d;
			init => pullLeftTrigger = value switch {
				< 0 or > 1 => throw new SettingInvalidException("PullLeftTrigger not within range [0, 1]."),
				_          => (byte)(value * 255)
			};
		}
		public double PullRightTrigger {
			get => pullRightTrigger / 255d;
			init => pullRightTrigger = value switch {
				< 0 or > 1 => throw new SettingInvalidException("PullRightTrigger not within range [0, 1]."),
				_          => (byte)(value * 255)
			};
		}
		public (double x, double y, bool relatively) MoveLeftStick {
			get => (moveLeftStick.X, moveLeftStick.Y, moveLeftStick.relatively);
			init => moveLeftStick = new Movement2(value.x, value.y, value.relatively, "MoveLeftStick");
		}
		public (double x, double y, bool relatively) MoveRightStick {
			get => (moveRightStick.X, moveRightStick.Y, moveRightStick.relatively);
			init => moveRightStick = new Movement2(value.x, value.y, value.relatively, "MoveRightStick");
		}
		public int Wait {
			get => wait;
			init => wait = value < 0
				? throw new SettingInvalidException("Wait must be a waitable time (> 0).")
				: value;
		}

		public readonly Movement2 moveLeftStick = new Movement2(0, 0, true);
		public readonly Movement2 moveRightStick = new Movement2(0, 0, true);
		public readonly byte pullLeftTrigger, pullRightTrigger;

		private readonly int wait = 0;
		private const int shortRange = Int16.MaxValue - Int16.MinValue;

		public override string ToString() => PressButtonsToString() + " "
			+ ReleaseButtonsToString() + " "
			+ "MoveMouse: " + MoveMouse.ToString() + " "
			+ "ScrollMouse: " + ScrollMouse.ToString() + " "
			+ "PullLeftTrigger: " + PullLeftTrigger.ToString() + " "
			+ "PullRightTrigger: " + PullRightTrigger.ToString() + " "
			+ "MoveLeftStick: " + moveLeftStick.ToString() + " "
			+ "MoveRightStick: " + moveRightStick.ToString() + " "
			+ "Wait: " + Wait;

		public string ToString(bool brief) {
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
				} else if (moveLeftStick is not {vector: (0, 0), relatively: true}) {
					str += "MoveLeftStick: " + moveLeftStick.ToString() + " ";
				} else if (moveRightStick is not {vector: (0, 0), relatively: true}) {
					str += "MoveRightStick: " + moveRightStick.ToString() + " ";
				} else if (Wait > 0) {
					str += "Wait: " + Wait + " ";
				}
				str.TrimEnd();
				return str;
			}
		}

		protected string PressButtonsToString() {
			var str = "PressButtons: [";
			for (int i = 0; i < PressButtons.Length - 1; i++) str += PressButtons[i].ToString() + ", ";
			str += PressButtons[PressButtons.Length - 1].ToString() + "]";
			return str;
		}

		protected string ReleaseButtonsToString() {
			var str = "ReleaseButtons: [";
			for (int i = 0; i < ReleaseButtons.Length - 1; i++) str += ReleaseButtons[i].ToString() + ", ";
			str += ReleaseButtons[ReleaseButtons.Length - 1].ToString() + "]";
			return str;
		}
	}
}