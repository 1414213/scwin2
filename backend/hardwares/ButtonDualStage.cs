using System;

namespace Input {
	internal class ButtonDualStage : Button {
		public Button Button { get; set; } = new ButtonKey();
		public new bool IsPressed => this.isPressed;

		private bool isPressed;

		protected override void PressImpl() {
			if (isPressed) {
				isPressed = false;
				Button.Release();
			} else {
				isPressed = true;
				Button.Press();
			}
		}

		protected override void ReleaseImpl() {}
	}
}