using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using api = SteamControllerApi;


namespace Backend {
	public class XYScrollRotatable : Trackpad {
		public bool IsWheelElseSwipe { get; set; } = true;
		public int Sensitivity { get; set; } = 50;
		public bool Reversed { get; set; }
		public double SwipeRotation { get => this.rotation; set => this.rotation = value; }
		[JsonIgnore]
		public override string HardwareType => "Trackpad";

		private double rotation = 0;
		private (short x, short y)? previous = null;

		protected override void DoEventImpl(api.InputData e) {
			var coord = e.Coordinates ?? throw new ArgumentException(e + " isn't coordinal.");

			// if e is an initial press
			if (!this.previous.HasValue) {
				this.previous = coord;
				return;
			}
			var previous = this.previous.Value;

			if (IsWheelElseSwipe) {
				// convert e into polar coordinates
				double r = Math.Sqrt(coord.x * coord.x + coord.y * coord.y);
				double theta = 0;
				if (coord.y >= 0 && r != 0) theta = Math.Acos(coord.x / r);
				else if (coord.y < 0) theta = -Math.Acos(coord.x / r);
				else if (r == 0) theta = Double.NaN;

				// convert previous into polar coordinates
				double previousR = Math.Sqrt(previous.x * previous.x + previous.y * previous.y);
				double previousTheta = 0;
				if (previous.y >= 0 && r != 0) previousTheta = Math.Acos(previous.x / previousR);
				else if (previous.y < 0) previousTheta = -Math.Acos(previous.x / previousR);
				else if (previousR == 0) previousTheta = Double.NaN;

				// find difference of rotation of current and previous coordinates and scroll mousewheel by a multiple of that
				if (!(Double.IsNaN(theta) && Double.IsNaN(previousTheta))) {
					double delta = Reversed ? previousTheta - theta : theta - previousTheta;
					robot.ScrollMouseWheel((int)(delta * Sensitivity));
				}
			} else {
				// find amount of movement
				var delta = (x: coord.x - previous.x, y: coord.y - previous.y);

				// find r as a proportion of the diameter of the trackpad
				double r = Math.Sqrt(delta.x * delta.x + delta.y * delta.y) / Int16.MaxValue;
				double theta = 0;
				if (delta.y >= 0 && r != 0) theta = Math.Acos(delta.x / r);
				else if (delta.y < 0) theta = -Math.Acos(delta.x / r);
				else if (r == 0) theta = Double.NaN;

				if (!Double.IsNaN(theta)) {
					double deltaAngle = Reversed ? theta - rotation : rotation - theta;
					// transform r by angle of rotation and multiply by sensitivity
					robot.ScrollMouseWheel((int)((r / Math.Cos(deltaAngle)) * Sensitivity));
				}
			}
			previous = coord;

			// if e is the final press
			if ((e.Flags & api.Flags.Released) == api.Flags.Released) {
				this.previous = null;
			}
		}

		protected override void ReleaseAllImpl() {}
	}
}