using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using api = SteamControllerApi;


namespace Backend {
	public class StickStick : Hardware {
		/// <summary>
		/// Proportion of the thumbstick's radius.
		/// </summary>
		public double Deadzone {
			get => this.deadzone;
			set {
				if (value < 0 || value > 1.0) throw new SettingNotProportionException(
					"Deadzone must be proportion of the thumbstick's radius ([0, 1])"
				);
				this.deadzone = value;
			}
		}
		/// <summary>
		/// Sends inputs to either left or right virtual thumbstick.
		/// <summary>
		public bool IsLeftElseRight { get; set; }

		private double deadzone = 0.2;

		public override void DoEvent(api.InputData e) {
			var coord = e.Coordinates ?? throw new ArgumentException(e + " must be a coordinal event.");

			// check if coordinate is inside the thumbstick's deadzone
			double r = Math.Sqrt(coord.x * coord.x + coord.y * coord.y) / Int16.MaxValue;
			if (r < deadzone) {
				if (IsLeftElseRight) robot.MoveLStick(0, 0);
				else robot.MoveRStick(0, 0);
				return;
			}

			// send the steam controller's stick data to the virtual gamepad
			if (IsLeftElseRight) robot.MoveLStick(e.Coordinates.Value.x, e.Coordinates.Value.y);
			else robot.MoveRStick(e.Coordinates.Value.x, e.Coordinates.Value.y);
		}

		public override void ReleaseAll() {}
	}
}