using System;
using api = SteamControllerApi;

namespace Input {
	public class ButtonAction : Button {
		public bool IsTransparent { get; set; } = true;
		public string Name { get; set; } = "";

		protected override void PressImpl() {
			EventDoer.AddActionLayer(Name, IsTransparent);
		}

		protected override void ReleaseImpl() {
			EventDoer.RemoveActionLayer(Name);
		}
	}
}