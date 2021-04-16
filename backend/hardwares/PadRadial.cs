using System;
using System.Collections.Generic;
using api = SteamControllerApi;

namespace Backend {
	public class PadRadial : Trackpad {
		public IList<Button> Buttons { get => this.radial.Buttons; set => this.radial.Buttons = value; }
		public double Deadzone { get => this.radial.Deadzone; set => this.radial.Deadzone = value; }
		public double AngleOffset { get => this.radial.AngleOffset; set => this.radial.AngleOffset = value; }
		public bool IncrementsLeftElseRight {
			get => this.radial.IncrementsLeftElseRight;
			set => this.radial.IncrementsLeftElseRight = value;
		}
		public bool TapsElseHolds { get => this.radial.TapsElseHolds; set => this.radial.TapsElseHolds = value; }
		// I don't know why I wrote this and it has 0 references ???
		//public bool StartsFromPress { get; set; } = true;

		private StickRadial radial = new StickRadial();

		// sets default deadzone to 0 instead of the default of StickRadial
		public PadRadial() => this.radial.Deadzone = 0;

		protected override void DoEventImpl(api.InputData e) {
			radial.DoEvent(e);
		}

		protected override void ReleaseAllImpl() {
			foreach (var b in radial.Buttons) b.Release();
		}
	}
}
