using System;

using Robot;


namespace Backend {
	public class ButtonKey : Button {
		public Key Key { get; set; } = Key.None;

		public ButtonKey(Key key = Key.None) : base(false) => this.Key = key;

		protected override void PressImpl() => robot.Press(this.Key);

		protected override void ReleaseImpl() => robot.Release(this.Key);
	}
}