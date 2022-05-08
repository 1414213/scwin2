using System;
using api = SteamControllerApi;
using Robot;

namespace Input {
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
		public double OverlapIgnoranceRadius { 
			get => buttonCross.OverlapIgnoranceRadius;
			set => buttonCross.OverlapIgnoranceRadius = value;
		}

		private StickButtonCross buttonCross = new StickButtonCross();

		protected override void DoEventImpl(api.ITrackpadData input) {
			var coord = input.Position;

			// Handle input.  If input is a release event, reset thumbstick
			buttonCross.DoEvent(input);
			if (input.IsRelease) buttonCross.DoEvent(new api.StickData((0, 0), api.Flags.None));
		}

		protected override void ReleaseAllImpl() {
			East.ReleaseAll();
			North.ReleaseAll();
			West.ReleaseAll();
			South.ReleaseAll();
			Inner.ReleaseAll();
			Outer.ReleaseAll();
		}

		public override void Unfreeze(api.IInputData newInput) => this.DoEvent(newInput);
	}
}