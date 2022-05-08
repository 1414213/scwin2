using System;
using api = SteamControllerApi;

namespace Input {
	public class StickStick : Hardware {
		/// <summary>
		/// Proportion of the thumbstick's radius.
		/// </summary>
		public double Deadzone { get => deadzone; set {
			if (value < 0 || value > 1.0) throw new SettingNotProportionException(
				"Deadzone must be proportion of the thumbstick's radius [0, 1].");
			this.deadzone = value;
		} }
		/// <summary>
		/// Proportion of thumbstick's radius where degrees of input is measured, measuring from its center.
		/// Input beyond this zone is simulated as maximum tilt.
		/// </summary>
		public double Gradingzone { get => gradingzone; set {
			if (value is < 0 or > 1d) throw new SettingNotProportionException(
				"Gradingzone must be proportion of the thumbstick's radius [0, 1].");
			this.gradingzone = value;
		} }
		/// <summary>
		/// Sends inputs to either left or right virtual thumbstick.
		/// <summary>
		public bool IsLeftElseRight { get; set; }

		double deadzone = 0.2, gradingzone = 1;

		public override void DoEvent(api.IInputData input) {
			var stickInput = input as api.IPositional
			                 ?? throw new ArgumentException(input + " must be a coordinal event.");

			var (r, coordCos, coordSin) = base.CartesianToAngle(stickInput.Position.x, stickInput.Position.y);

			// check if coordinate is inside the thumbstick's deadzone
			if (r < Deadzone * Int16.MaxValue || r == 0) {
				if (IsLeftElseRight) robot.MoveLStick(0, 0);
				else robot.MoveRStick(0, 0);
				return;
			}

			// proportion simulated input to be inside of the grading zone
			double r_graded = 0;
			{
				double temp = (r / Int16.MaxValue) / Gradingzone;
				r_graded = (temp >= 1 ? 0.999 : temp) * Int16.MaxValue;
			}

			// var coord_graded = (x: (short)Math.Clamp(
			//                     	(int)(r_graded * coordCos),
			// 	                	Int16.MinValue,
			// 	                	Int16.MaxValue
			//                     ),
			//                     y: (short)Math.Clamp(
			//                     	(int)(r_graded * coordSin),
			// 	                	Int16.MinValue,
			// 	                	Int16.MaxValue
			//                     ));
			var coord_graded = (
				x: (short)Math.Clamp((int)(r_graded * coordCos), Int16.MinValue, Int16.MaxValue),
				y: (short)Math.Clamp((int)(r_graded * coordSin), Int16.MinValue, Int16.MaxValue)
			);

			// send the steam controller's stick data to the virtual gamepad
			if (IsLeftElseRight) robot.MoveLStick(coord_graded.x, coord_graded.y);
			else                 robot.MoveRStick(coord_graded.x, coord_graded.y);
		}

		public override void ReleaseAll() {}

		public override void Unfreeze(api.IInputData newInput) => this.DoEvent(newInput);
	}
}