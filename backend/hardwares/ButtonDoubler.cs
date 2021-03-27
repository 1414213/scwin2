using System;
using api = SteamControllerApi;

namespace Backend {
	public class ButtonDoubler : Button {
		public Button Button { get; set; } = new ButtonKey();

		protected override void PressImpl() => Button.Tap();

		protected override void ReleaseImpl() => Button.Tap();
	}
}