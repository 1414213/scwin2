using SteamControllerApi;
using api = SteamControllerApi;

namespace Backend {
	public class PadRadial : Trackpad {
		public Button[] Buttons { get => radial.Buttons; set => radial.Buttons = value; }
		public double Deadzone { get => radial.Deadzone; set => radial.Deadzone = value; }
		public double AngleOffset { get => radial.AngleOffset; set => radial.AngleOffset = value; }
		public bool IncrementsLeftElseRight {
			get => radial.IncrementsLeftElseRight;
			set => radial.IncrementsLeftElseRight = value;
		}
		public bool TapsElseHolds { get => radial.TapsElseHolds; set => radial.TapsElseHolds = value; }

		private StickRadial radial = new StickRadial{ Deadzone = 0 };

		protected override void DoEventImpl(api.ITrackpadData input) {
			radial.DoEvent(input);
		}

		protected override void ReleaseAllImpl() { foreach (var b in radial.Buttons) b.Release(); }

		public override void Unfreeze(IInputData newInput) => this.DoEvent(newInput);
	}
}
