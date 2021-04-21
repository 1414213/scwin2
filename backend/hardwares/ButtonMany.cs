using System;
using System.Collections;
using System.Collections.Generic;

using api = SteamControllerApi;
using Robot;


namespace Backend {
	class ButtonMany : Button, IEnumerable {
		public IList<Button> Buttons { get; set; } = new List<Button>();

		public ButtonMany() {}

		public ButtonMany(IList<Button> buttons) {
			this.Buttons = buttons;
		}

		public ButtonMany(params Key[] keys) {
			foreach (Key k in keys) {
				Buttons.Add(new ButtonKey{ Key = k });
			}
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