using System;
using api = SteamControllerApi;
using Robot;

namespace Input {
	public class StickButtonCross : Hardware {
		public Button East { get; set; } = new ButtonKey();
		public Button North { get; set; } = new ButtonKey();
		public Button West { get; set; } = new ButtonKey();
		public Button South { get; set; } = new ButtonKey();
		public Button Inner { get; set; } = new ButtonKey();
		public Button Outer { get; set; } = new ButtonKey();
		public bool HasOverlap { get; set; } = true;
		public double OverlapIgnoranceRadius { get => overlapIgnoranceRadius; set {
			if (value is < 0 or > 1.0) throw new SettingNotProportionException(
				"OverlapIgnorance radius should be bewteen 0 and 1, inclusively.");
			else this.overlapIgnoranceRadius = value;
		} }
		public double Deadzone { get => deadzone; set {
			if (value > 1.0 || value < 0) throw new SettingNotProportionException(
				"Deadzone must be proportion of the thumbstick's radius ([0, 1]).");
			else this.deadzone = value;
		} }
		public double InnerRadius { get => innerRadius;	set {
			if (value > 1.0 || value < 0) throw new SettingNotProportionException(
				"InnerRadius must be proportion of the thumbstick's radius ([0, 1]).");
			else this.innerRadius = value;
		} }
		public double OuterRadius { get => outerRadius;	set {
			if (value > 1.0 || value < 0) throw new SettingNotProportionException(
				"OuterRadius must be proportion of the thumbstick's radius ([0, 1]).");
			else this.outerRadius = value;
		} }

		private double deadzone = 0.2, innerRadius = 0.35, outerRadius = 0, overlapIgnoranceRadius = 0;

		public StickButtonCross() {}

		public StickButtonCross(Key east, Key north, Key west, Key south, double deadzone, bool hasOverlap = false) {
			this.East = new ButtonKey(east);
			this.North = new ButtonKey(north);
			this.West = new ButtonKey(west);
			this.South = new ButtonKey(south);
			this.HasOverlap = hasOverlap;
			this.Deadzone = deadzone;
		}

		public override void DoEvent(api.IInputData input) {
			var coord = (input as api.IPositional ?? throw new ArgumentException(input + " is not coordinal."))
			            .Position;

			// Convert event's cartesian coordinates into polar.
			var (r, theta) = base.CartesianToPolar(coord.x, coord.y);
			if (Double.IsNaN(theta) || (r < Deadzone * Int16.MaxValue)) {
				// If r is 0, then stick was reset to position 0,0.  No input occurs so all buttons are released.
				// Also checks if the input event occured inside of the deadzone.
				this.ReleaseAll();
				return;
			}

			// Transform theta into units of PI (range of [0,2)).
			theta = theta / Math.PI;
			if (theta < 0) theta += 2;

			// Check if input is within range to trigger the inner button.
			if (r < innerRadius * Int16.MaxValue) Inner.Press();
			else Inner.Release();

			// Check if input is within range to trigger the outer button.
			if (r > (1d - outerRadius) * Int16.MaxValue) Outer.Press();
			else Outer.Release();

			// Check if overlapping should be ignored.
			var hasOverlap = HasOverlap && (r > OverlapIgnoranceRadius * Int16.MaxValue);

			// Determine the angle of the event and activate its respective button.
			if (hasOverlap) {
				if ((theta >= 13d/8 && theta < 2.0) || (theta >= 0 && theta < 3d/8)) East.Press();
				else East.Release();
				if (theta >= 1d/8 && theta < 7d/8)  North.Press();
				else North.Release();
				if (theta >= 5d/8 && theta < 11d/8) West.Press();
				else West.Release();
				if (theta >= 9d/8 && theta < 15d/8) South.Press();
				else South.Release();
			} else {
				if ((theta >= 1.75 && theta  < 2.0) || (theta < 0.25 && theta >= 0)) East.Press();
				else East.Release();
				if (theta >= 0.25 && theta < 0.75) North.Press();
				else North.Release();
				if (theta >= 0.75 && theta < 1.25) West.Press();
				else West.Release();
				if (theta >= 1.25 && theta < 1.75) South.Press();
				else South.Release();
			}
		}

		public override void ReleaseAll() {
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