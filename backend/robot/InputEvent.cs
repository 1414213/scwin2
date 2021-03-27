using System;

namespace Robot {
	public class InputEvent {
		[Flags]
		public enum Types {
			None = 0,
			Key = 1,
			MouseMove = 2,
			MouseScroll = 4,
			TriggerPull = 8,
			ThumbstickMove = 16
		}

		public Types types = Types.None;
		public Key[] keys = {Key.None};
		public bool isPressElseRelease = true;
		public byte pullDistance;
		public int scrollAmount;
		public bool scrollAsClicks;
		public (int x, int y) coordinates;
		public bool isRelative;
		public bool isLeftElseRight = true;

		public InputEvent(Types key, params Key[] keys) {
			this.types = key;
			this.keys = keys;
		}

		public InputEvent(Types trigger, byte pullDistance) {
			this.types = trigger;
			this.pullDistance = pullDistance;
		}

		public InputEvent(Types scroll, int scrollAmount, bool scrollAsClicks = false) {
			this.types = scroll;
			this.scrollAmount = scrollAmount;
			this.scrollAsClicks = scrollAsClicks;
		}

		public InputEvent(Types coordinal, (short x, short y) coordinates) {
			this.types = coordinal;
			this.coordinates = coordinates;
		}

		public InputEvent(Types coordinal, (int x, int y) coordinates) {
			this.types = coordinal;
			this.coordinates = coordinates;
		}
	}
}