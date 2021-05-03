using System;

namespace Robot {
	public class Macro {
		public Key[] PressButtons = {};
		public Key[] ReleaseButtons = {};
		public (int x, int y, bool relative)? MoveMouse;
		public (int amount, bool asClicks)? ScrollMouse;
		public byte? PullLeftTrigger, PullRightTrigger;
		public (short x, short y)? MoveLeftStickTo, MoveRightStickTo;
		public string? AddActionLayer, RemoveActionLayer;
		public bool AddActionLayerAsTransparent = true;
		public int Wait { get => wait; set {
			if (value < 0) throw new Backend.SettingInvalidException("Wait must be a waitable time (> 0).");
			this.wait = value;
		} }

		private int wait = 0;
	}
}