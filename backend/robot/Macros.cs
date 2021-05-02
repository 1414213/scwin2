using System;

namespace Robot {
	public class Macro {
		public Key[] PressButtons = {};
		public Key[] ReleaseButtons = {};
		public (int x, int y, bool relative)? MoveMouse;
		public (int amount, bool asClicks)? ScrollMouse;
		public byte? PullLeftTrigger;
		public byte? PullRightTrigger;
		public (short x, short y)? MoveLeftStickTo;
		public (short x, short y)? MoveRightStickTo;
		public int Wait = 0;

		public Macro() {}
	}
}