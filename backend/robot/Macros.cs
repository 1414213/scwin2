using System;

namespace Robot {
	public class Macro {
		public Key[] PressButtons = {Key.None};
		public Key[] ReleaseButtons = {Key.None};
		public (int x, int y, bool relative) MoveMouse = (0, 0, false);
		public (int amount, bool asClicks) ScrollMouse = (0, true);
		public byte PullLeftTrigger = 0;
		public byte PullRightTrigger = 0;
		public (short x, short y) MoveLeftStickTo = (0, 0);
		public (short x, short y) MoveRightStickTo = (0, 0);
		public int Wait = 0;

		public Macro() {}
	}
}