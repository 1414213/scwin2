using System.Collections;
using Robot;

namespace Input {
	class ButtonMany : Button, IEnumerable {
		public Button[] Buttons { get; set; } = {};

		public ButtonMany() {}

		public ButtonMany(Button[] buttons) {
			this.Buttons = buttons;
		}

		public ButtonMany(params Key[] keys) {
			this.Buttons = new Button[keys.Length];
			for (int i = 0; i < keys.Length; i++) this.Buttons[i] = new ButtonKey(keys[i]);
		}

		public IEnumerator GetEnumerator() => Buttons.GetEnumerator();

		protected override void PressImpl() {
			foreach (Button b in Buttons) b.Press();
		}

		protected override void ReleaseImpl() {
			foreach (Button b in Buttons) b.Release();
		}
	}
}