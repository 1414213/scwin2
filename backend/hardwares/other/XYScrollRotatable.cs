using System;
using api = SteamControllerApi;

namespace Backend {
	public class XYScrollRotatable : Trackpad {
		public bool IsWheelElseSwipe { get; set; } = true;
		public int Sensitivity { get; set; } = 50;
		public bool Reversed { get; set; }
		public double SwipeRotation { get => this.rotation; set => this.rotation = value; }

		private double rotation = 0;
		private (short x, short y)? previous = null;

		protected override void DoEventImpl(api.ITrackpadData input) {
			var coord = input.Position;

			// if e is an initial press
			if (!this.previous.HasValue) {
				this.previous = coord;
				return;
			}
			var previous = this.previous.Value;

			if (IsWheelElseSwipe) {
				// convert e and previous into polar coordinates
				var (r, theta) = base.CartesianToPolar(coord.x, coord.y);
				var (previousR, previousTheta) = base.CartesianToPolar(previous.x, previous.y);

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
				var (_, theta) = base.CartesianToPolar(delta.x, delta.y);

				if (!Double.IsNaN(theta)) {
					double deltaAngle = Reversed ? theta - rotation : rotation - theta;
					// transform r by angle of rotation and multiply by sensitivity
					robot.ScrollMouseWheel((int)((r / Math.Cos(deltaAngle)) * Sensitivity));
				}
			}
			previous = coord;

			// if e is the final press
			if (input.IsRelease) {
				this.previous = null;
			}
		}

		protected override void ReleaseAllImpl() {}
	}
}