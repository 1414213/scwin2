using System;
using SteamControllerApi;
using api = SteamControllerApi;

namespace Input {
	public class XYScrollRotatable : Trackpad {
		public bool IsWheelElseSwipe { get; set; } = true;
		public int Sensitivity { get; set; } = 50;
		public bool Reversed { get; set; }
		public double SwipeRotation { get => this.rotation; set => this.rotation = value; }

		private double rotation = 0;
		private (short x, short y)? previous = null;

		protected override void DoEventImpl(api.ITrackpadData input) {
			var coord = input.Position;

			// If e is an initial press:
			if (!this.previous.HasValue) {
				this.previous = coord;
				return;
			}
			var previous = this.previous.Value;

			if (IsWheelElseSwipe) {
				// Convert e and previous into polar coordinates.
				var (r, theta) = base.CartesianToPolar(coord.x, coord.y);
				var (previousR, previousTheta) = base.CartesianToPolar(previous.x, previous.y);

				// Find difference of rotation of current and previous coordinates and scroll mousewheel by a multiple of that.
				if (!(Double.IsNaN(theta) && Double.IsNaN(previousTheta))) {
					double delta = Reversed ? previousTheta - theta : theta - previousTheta;
					robot.ScrollMouseWheel((int)(delta * Sensitivity));
				}
			} else {
				// Find amount of movement.
				var delta = (x: coord.x - previous.x, y: coord.y - previous.y);

				// Find r as a proportion of the diameter of the trackpad.
				double r = Math.Sqrt(delta.x * delta.x + delta.y * delta.y) / Int16.MaxValue;
				var (_, theta) = base.CartesianToPolar(delta.x, delta.y);

				if (!Double.IsNaN(theta)) {
					double deltaAngle = Reversed ? theta - rotation : rotation - theta;
					// Transform r by angle of rotation and multiply by sensitivity.
					robot.ScrollMouseWheel((int)((r / Math.Cos(deltaAngle)) * Sensitivity));
				}
			}
			previous = coord;

			// If e is the final press:
			if (input.IsRelease) {
				this.previous = null;
			}
		}

		protected override void ReleaseAllImpl() {}

		public override void Unfreeze(IInputData newInput) {
			previous = null;
			this.DoEvent(newInput);
		}
	}
}