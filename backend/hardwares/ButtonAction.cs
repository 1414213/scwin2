using System;
using api = SteamControllerApi;

namespace Backend {
	public class ButtonAction : Button {
		public bool IsTransparent { get; set; } = true;
		public string Name { get; set; } = "";

		protected override void PressImpl()
		{
			SideEffectsPipe.Enqueue(new ActionMapAddition{ name = Name, isTransparent = IsTransparent });
		}

		protected override void ReleaseImpl()
		{
			SideEffectsPipe.Enqueue(new ActionMapRemoval{ name = Name });
		}
	}
}