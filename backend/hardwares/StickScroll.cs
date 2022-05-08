using System;
using api = SteamControllerApi;

namespace Input {
	public class StickScroll : Hardware {
		public double Sensitivity { get; set; } = 0.8;
		public double Deadzone { get => deadzone; set {
			if (value < 0 && value > 1d)
				throw new SettingNotProportionException("Deadzone must be a ratio of the radius [0, 1].");
			this.deadzone = value;
		} }
		public bool Reversed { get; set; }
		public bool ScrollAlongXElseY { get; set; } = true;

		// ratio of the radius
		private double deadzone = 0.2;
		private double amount;

		public override void DoEvent(api.IInputData input) {
			(int x, int y) coord = (input as api.IPositional ?? throw new ArgumentException(input + "isn't coordinal."))
			                       .Position;

			// check if thumbstick position is within the deadzone
			double r = Math.Sqrt((coord.x * coord.x) + (coord.y * coord.y));
			if (r < deadzone * Int16.MaxValue) {
				amount = 0;
				return;
			}
			
			// increment amount to scroll by by the current offset of the stick
			double amountToAdd = (double)(this.ScrollAlongXElseY ? coord.x : coord.y) / Int16.MaxValue;
			amountToAdd *= this.Sensitivity;
			
			// send scroll input.  Input is stored so that fractional input
			// isn't lost during the conversion to a whole number
			amount += this.Reversed ? -amountToAdd : amountToAdd;
			robot.ScrollMouseWheel((int)amount);
			amount -= (int)amount;
		}

		public override void ReleaseAll() {}

		public override void Unfreeze(api.IInputData newInput) => this.DoEvent(newInput);
	}
}