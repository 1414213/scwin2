using System;
using Newtonsoft.Json;
using api = SteamControllerApi;
using Robot;


namespace Backend {
	public class StickButtonCross : Hardware {
		public Button East { get; set; } = new ButtonKey();
		public Button North { get; set; } = new ButtonKey();
		public Button West { get; set; } = new ButtonKey();
		public Button South { get; set; } = new ButtonKey();
		public Button Inner { get; set; } = new ButtonKey();
		public Button Outer { get; set; } = new ButtonKey();
		public bool HasOverlap { get; set; } = true;
		public double Deadzone { get => deadzone; set {
			if (value > 1.0 || value < 0) throw new SettingNotProportionException(
				"Deadzone must be proportion of the thumbstick's radius ([0, 1]).");
			else this.deadzone = value;
		} }
		public double InnerRadius { get => innerRadius;	set {
			if (value > 1.0 || value < 0) throw	new SettingNotProportionException(
				"InnerRadius must be proportion of the thumbstick's radius ([0, 1]).");
			else this.innerRadius = value;
		} }
		public double OuterRadius { get => outerRadius;	set {
			if (value > 1.0 || value < 0) throw	new SettingNotProportionException(
				"OuterRadius must be proportion of the thumbstick's radius ([0, 1]).");
			else this.outerRadius = value;
		} }

		private double deadzone = 0.2, innerRadius = 0.35, outerRadius = 0;

		public StickButtonCross() {}

		public StickButtonCross(Key east, Key north, Key west, Key south, double deadzone, bool hasOverlap = false) {
			this.East = new ButtonKey(east);
			this.North = new ButtonKey(north);
			this.West = new ButtonKey(west);
			this.South = new ButtonKey(south);
			this.HasOverlap = hasOverlap;
			this.Deadzone = deadzone;
		}

		public StickButtonCross(
			Button east,
			Button north,
			Button west,
			Button south,
			double deadzone,
			bool hasOverlap = false
		) {
			this.East = east;
			this.North = north;
			this.West = west;
			this.South = south;
			this.HasOverlap = hasOverlap;
			this.Deadzone = deadzone;
		}

		public override void DoEvent(api.InputData e) {
			var coord = e.Coordinates ?? throw new ArgumentException(e + " is not coordinal.");

			// convert event's cartesian coordinates into polar
			double r = Math.Sqrt(coord.x * coord.x + coord.y * coord.y);
			double theta = 0;
			if (coord.y >= 0 && r != 0) theta = Math.Acos(coord.x / r);
			else if (coord.y < 0)       theta = -Math.Acos(coord.x / r);
			else if (r == 0)            theta = Double.NaN;
			if (Double.IsNaN(theta) || (r < Deadzone * Int16.MaxValue)) {
				// If r is 0, then stick was reset to position 0,0.  No input occurs so all buttons are released.
				// Also checks if the input event occured inside of the deadzone.
				East.Release();
				North.Release();
				West.Release();
				South.Release();
				Inner.Release();
				Outer.Release();
				return;
			}

			// transform theta into units of PI (range of [0,2)).
			theta = theta / Math.PI;
			if (theta < 0) theta += 2;

			// check if event is within the trigger range for the inner button
			if (r < innerRadius * Int16.MaxValue) Inner.Press();
			else Inner.Release();

			// check if event is within the trigger range for the outer button
			if (r > (1d - outerRadius) * Int16.MaxValue) Outer.Press();
			else Outer.Release();

			// determine the angle of the event and activate its respective button
			//Console.WriteLine("radius: " + r + " theta: " + theta);
			if (HasOverlap) {
				if ((theta >= 13d/8 && theta < 2.0) || (theta >= 0 && theta < 3d/8)) {
					East.Press();
				}
				else East.Release();
				if (theta >= 1d/8 && theta < 7d/8) {
					North.Press();
				}
				else North.Release();
				if (theta >= 5d/8 && theta < 11d/8) {
					West.Press();
				}
				else West.Release();
				if (theta >= 9d/8 && theta < 15d/8) {
					South.Press();
				}
				else South.Release();
			}
			else {
				if ((theta >= 1.75 && theta  < 2.0) || (theta < 0.25 && theta >= 0)) {
					East.Press();
				}
				else East.Release();
				if (theta >= 0.25 && theta < 0.75) {
					North.Press();
				}
				else North.Release();
				if (theta >= 0.75 && theta < 1.25) {
					West.Press();
				}
				else West.Release();
				if (theta >= 1.25 && theta < 1.75) {
					South.Press();
				}
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
	}
}