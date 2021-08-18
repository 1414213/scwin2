using System;
using api = SteamControllerApi;

namespace Backend {
	public class PadScroll : Trackpad {
		public bool IsWheelElseSwipe { get; set; } = true;
		public double Sensitivity { get; set; } = 5;
		public bool Reversed { get; set; }
		public bool SwipeAlongXElseY { get; set; } = true;

		private bool isInitialPress = true;
		private (short x, short y) previous;
		private double previousTheta;
		private double amountStore;

		protected override void DoEventImpl(api.ITrackpadData input) {
			(short x, short y) coord = input.Position;

			// if e is an initial press
			if (isInitialPress) {
				previous = coord;
				isInitialPress = false;
				var (_, theta) = base.CartesianToPolar(coord.x, coord.y);
				this.previousTheta = theta;
				return;
			}

			if (IsWheelElseSwipe) {
				// convert e into polar coordinates
				double r = Math.Sqrt((coord.x * coord.x) + (coord.y * coord.y));
				double theta = 0;
				if (coord.y >= 0 && r != 0) theta = Math.Acos(coord.x / r);
				else if (coord.y < 0) theta = -Math.Acos(coord.x / r);
				else if (r == 0) theta = Double.NaN;

				// convert previous into polar coordinates
				double previousTheta = this.previousTheta;

				// find difference of rotation of current and previous coordinates and scroll mousewheel by a multiple
				// of that
				if (!(Double.IsNaN(theta) && Double.IsNaN(previousTheta))) {
					if (theta < 0) theta += 2 * Math.PI;
					if (previousTheta < 0) previousTheta += 2 * Math.PI;

					double delta;
					// check if swipe moves from fourth quadrant to first and fix math
					if (previousTheta >= Math.PI*1.5 && theta < Math.PI*0.5) {
						delta = Reversed ? previousTheta - (theta + 2 * Math.PI)
						                 : (theta + 2 * Math.PI) - previousTheta;
					}
					// else if swipe moves from first quadrant to fourth
					else if (previousTheta < Math.PI*0.5 && theta >= Math.PI*1.5) {
						delta = Reversed ? (previousTheta + 2 * Math.PI) - theta
						                 : theta - (previousTheta + 2 * Math.PI);
					} else delta = Reversed ? previousTheta - theta : theta - previousTheta;
					delta *= Sensitivity;

					amountStore += delta;
					robot.ScrollMouseWheel((int)amountStore);
					amountStore -= (int)amountStore;
				}
				this.previousTheta = theta;
			} else {
				// find amount of movement
				// (double x, double y) delta = (
				// 	(double)(this.Reversed ? previous.x - coord.x : coord.x - previous.x) / Int16.MaxValue, 
				// 	(double)(this.Reversed ? previous.y - coord.y : coord.y - previous.y) / Int16.MaxValue
				// );
				double delta = SwipeAlongXElseY
					? (double)(Reversed ? previous.x - coord.x : coord.x - previous.x) / Int16.MaxValue
					: (double)(Reversed ? previous.y - coord.y : coord.y - previous.y) / Int16.MaxValue;
				delta *= this.Sensitivity;

				// use event to generate mouse wheel scrolling
				amountStore += delta;
				robot.ScrollMouseWheel((int)amountStore);
				amountStore -= (int)amountStore;
			}
			previous = coord;

			// if e is the final press
			if (input.IsRelease) {
				isInitialPress = true;
				amountStore = 0;
			}
		}

		protected override void ReleaseAllImpl() {}

		public override void Unfreeze(api.IInputData newInput) {
			// Reset input intake so that previous inputs from before freezing the collection 
			// of input don't cause any jumping.
			isInitialPress = true;
			amountStore = 0;
			this.DoEvent(newInput);
		}
	}
}