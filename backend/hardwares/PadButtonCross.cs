using System;
using api = SteamControllerApi;
using Robot;

namespace Backend {
	public class PadButtonCross : Trackpad {
		public Button East { get => buttonCross.East; set => buttonCross.East = value; }
		public Button North { get => buttonCross.North; set => buttonCross.North = value; }
		public Button West { get => buttonCross.West; set => buttonCross.West = value; }
		public Button South { get => buttonCross.South; set => buttonCross.South = value; }
		public Button Inner { get => buttonCross.Inner; set => buttonCross.Inner = value; }
		public Button Outer { get => buttonCross.Outer; set => buttonCross.Outer = value; }
		public bool HasOverlap { get => buttonCross.HasOverlap; set => buttonCross.HasOverlap = value; }
		public double Deadzone { get => buttonCross.Deadzone; set => buttonCross.Deadzone = value; }
		public double InnerRadius { get => buttonCross.InnerRadius; set => buttonCross.InnerRadius = value; }
		public double OuterRadius { get => buttonCross.OuterRadius; set => buttonCross.OuterRadius = value; }
		public double OverlapIgnoranceRadius { get => overlapIgnoranceRadius; set {
			if (value < 0 || value > 1.0) throw new SettingNotProportionException(
				"OverlapIgnoranceRadius must be proportion of the thumbstick's radius ([0, 1]).");
			overlapIgnoranceRadius = value;
		} }

		private StickButtonCross buttonCross = new StickButtonCross();
		private double overlapIgnoranceRadius = 0.5;

		public PadButtonCross() {}

		public PadButtonCross(Key east, Key north, Key west, Key south, double deadzone, bool hasOverlap = false) {
			this.buttonCross = new StickButtonCross(east, north, west, south, deadzone, hasOverlap);
		}

		protected override void DoEventImpl(api.ITrackpadData input) {
			var coord = input.Position;

			// Check if the event occurred within the radius where overlap is ignored.
			// If the overlap value needs to be changed, then change it
			double r = Math.Sqrt(coord.x * coord.x + coord.y * coord.y) / Int16.MaxValue;
			if (buttonCross.HasOverlap && r < overlapIgnoranceRadius) {
				buttonCross.HasOverlap = false;
			} else {
				if (!buttonCross.HasOverlap) buttonCross.HasOverlap = true;
			}

			// Handle event.  If e is a release event, reset thumbstick
			buttonCross.DoEvent(input);
			if (input.IsRelease) {
				buttonCross.DoEvent(new api.StickData((0, 0), api.Flags.None));
			}
		}

		protected override void ReleaseAllImpl() {
			East.ReleaseAll();
			North.ReleaseAll();
			West.ReleaseAll();
			South.ReleaseAll();
			Inner.ReleaseAll();
			Outer.ReleaseAll();
		}
	}
}