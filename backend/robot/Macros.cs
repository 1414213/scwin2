using System;
using Newtonsoft.Json;

namespace Robot {
	public class Macro {
		public struct Move2<T> {
			public T x, y; public bool relatively;
			public override string ToString() =>
				"(" + x + ", " + y + ", " + relatively + ")";
		}
		public struct Scroll {
			public int amount; public bool asClicks;
			public override string ToString() => "(" + amount + ", " + asClicks + ")";
		}

		public Key[] PressButtons = {};
		public Key[] ReleaseButtons = {};
		public Move2<int>? MoveMouse;
		public Scroll? ScrollMouse;
		public byte? PullLeftTrigger { get; init; }
		public byte? PullRightTrigger { get; init; }
		public Move2<double>? MoveLeftStick {
			get => moveLeftStick switch {
				Move2<short> m => new Move2<double>{ x = this.ShortToRatio(m.x),
				                                     y = this.ShortToRatio(m.y),
				                                     relatively = m.relatively },
				null           => null
			};
			init => moveLeftStick = value switch {
				Move2<double> m when (m.x is < 1d and > -1d) && (m.y is < 1d and > -1d)
				     => new Move2<short>{ x = this.RatioToShort(m.x),
				                          y = this.RatioToShort(m.y),
				                          relatively = m.relatively },
				null => null,
				_ => throw new Backend.SettingInvalidException("The axes of MoveLeftStick must be between -1 and 1.")
			};
		}
		public Move2<double>? MoveRightStick {
			get => moveRightStick switch {
				Move2<short> m => new Move2<double>{ x = this.ShortToRatio(m.x),
				                                     y = this.ShortToRatio(m.y),
				                                     relatively = m.relatively },
				null           => null
			};
			init => moveRightStick = value switch {
				Move2<double> m when m.x < 1d && m.x > -1d && m.y < 1d && m.y > -1d
				     => new Move2<short>{ x = this.RatioToShort(m.x),
				                          y = this.RatioToShort(m.y),
				                          relatively = m.relatively },
				null => null,
				_ => throw new Backend.SettingInvalidException("The axes of MoveRightStick must be between -1 and 1.")
			};
		}
		public readonly Move2<short>? moveLeftStick, moveRightStick;
		public string? AddActionLayer, RemoveActionLayer;
		public bool AddActionLayerAsTransparent = true;
		public int Wait { get => wait; init {
			if (value < 0) throw new Backend.SettingInvalidException("Wait must be a waitable time (> 0).");
			this.wait = value;
		} }

		readonly int wait = 0;
		
		const int shortRange = Int16.MaxValue - Int16.MinValue;

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
				if (PressButtons.Length != 0)      str += PressButtonsToString + " ";
				if (ReleaseButtons.Length != 0)    str += ReleaseButtonsToString + " ";
				if (MoveMouse is not null)         str += MoveMouseToString + " ";
				if (ScrollMouse is not null)       str += ScrollMouseToString + " ";
				if (PullLeftTrigger is not null)   str += PullLeftTriggerToString + " ";
				if (PullRightTrigger is not null)  str += PullRightTriggerToString + " ";
				if (MoveLeftStick is not null)     str += MoveLeftStickToString + " ";
				if (MoveRightStick is not null)    str += MoveRightStickToString + " ";
				if (AddActionLayer is not null)    str += AddActionLayerToString + " ";
				if (RemoveActionLayer is not null) str += RemoveActionLayerToString + " ";
				if (AddActionLayer is not null)    str += AddActionLayerAsTransparentToString + " ";
				if (Wait > 0)                      str += WaitToString + " ";
				str.TrimEnd();

				return str;
			}
		}

		string PressButtonsToString { get {
			var str = "PressButtons: [";
			for (int i = 0; i < PressButtons.Length - 1; i++) str += PressButtons[i].ToString() + ", ";
			str += PressButtons[PressButtons.Length - 1].ToString() + "]";
			return str;
		} }
		string ReleaseButtonsToString { get {
			var str = "ReleaseButtons: [";
			for (int i = 0; i < ReleaseButtons.Length - 1; i++) str += ReleaseButtons[i].ToString() + ", ";
			str += ReleaseButtons[ReleaseButtons.Length - 1].ToString() + "]";
			return str;
		} }
		string MoveMouseToString => "MoveMouse: " + (MoveMouse?.ToString() ?? "null");
		string ScrollMouseToString => "ScrollMouse: " + (ScrollMouse?.ToString() ?? "null");
		string PullLeftTriggerToString => "PullLeftTrigger: " + (PullLeftTrigger?.ToString() ?? "null");
		string PullRightTriggerToString => "PullRightTrigger: " + (PullRightTrigger?.ToString() ?? "null");
		string MoveLeftStickToString => "MoveLeftStick: " + (MoveLeftStick?.ToString() ?? "null");
		string MoveRightStickToString => "MoveRightStick: " + (moveRightStick?.ToString() ?? "null");
		string AddActionLayerToString => "AddActionLayer: " + (AddActionLayer ?? "null");
		string RemoveActionLayerToString => "RemoveActionLayer: " + (RemoveActionLayer ?? "null");
		string AddActionLayerAsTransparentToString => "AddActionLayerAsTransparent: " + AddActionLayerAsTransparent;
		string WaitToString => "Wait: " + Wait;

		short RatioToShort(double ratio) {
			var value = ratio > 0 ? ratio * Int16.MaxValue : ratio * Int16.MinValue;
			return (short)Math.Clamp(value, Int16.MinValue, Int16.MaxValue);
		}

		double ShortToRatio(short n) {
			if (n == 0) return 0;
			var value = n > 0 ? n / (double)Int16.MaxValue : n / (double)Int16.MinValue;
			return Math.Clamp(value, -1d, 1d);
		}
	}
}