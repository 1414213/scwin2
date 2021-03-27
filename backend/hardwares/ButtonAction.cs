using System;
using api = SteamControllerApi;

namespace Backend {
	public class ButtonAction : Button {
		public bool IsLayered { get; set; } = true;
		public string Name { get; set; } = "";

		protected override void PressImpl()
		{
			sideEffectsPipe.Enqueue(new ActionMapAddition{ name = Name, isLayered = IsLayered });
		}

		protected override void ReleaseImpl()
		{
			sideEffectsPipe.Enqueue(new ActionMapRemoval{ name = Name });
		}
	}
}