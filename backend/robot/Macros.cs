using System;
using Newtonsoft.Json;

namespace Robot {
	public class Macro {
		public Key[] PressButtons = {};
		public Key[] ReleaseButtons = {};
		public (int x, int y, bool relatively)? MoveMouse;
		public (int amount, bool asClicks)? ScrollMouse;
		public byte? PullLeftTrigger, PullRightTrigger;
		public (double x, double y, bool relatively)? MoveLeftStickTo {
			get => moveLeftStickTo switch {
				(short, short, bool) m => (m.x / shortRange, m.y / shortRange, m.relatively),
				null                   => null
			};
			init => moveLeftStickTo = value switch {
				(double, double, bool) m when m.x < 1d && m.x > -1d && m.y < 1d && m.y > -1d
				     => ((short)(m.x * shortRange), (short)(m.y * shortRange), m.relatively),
				null => null,
				_ => throw new Backend.SettingInvalidException("The axes of MoveLeftStickTo must be between -1 and 1.")
			};
		}
		public (double x, double y, bool relatively)? MoveRightStickTo {
			get => moveRightStickTo switch {
				(short, short, bool) m => (m.x / shortRange, m.y / shortRange, m.relatively),
				null                   => null
			};
			init => moveRightStickTo = value switch {
				(double, double, bool) m when m.x < 1d && m.x > -1d && m.y < 1d && m.y > -1d
				     => ((short)(m.x * shortRange), (short)(m.y * shortRange), m.relatively),
				null => null,
				_ => throw new Backend.SettingInvalidException("The axes of MoveRightStickTo must be between -1 and 1.")
			};
		}
		[JsonIgnore]
		public readonly (short x, short y, bool relatively)? moveLeftStickTo, moveRightStickTo;
		public string? AddActionLayer, RemoveActionLayer;
		public bool AddActionLayerAsTransparent = true;
		public int Wait { get => wait; init {
			if (value < 0) throw new Backend.SettingInvalidException("Wait must be a waitable time (> 0).");
			this.wait = value;
		} }

		int wait = 0;
		
		const int shortRange = Int16.MaxValue - Int16.MinValue;
	}
}