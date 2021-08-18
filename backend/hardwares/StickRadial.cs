using System;
using Newtonsoft.Json;
using api = SteamControllerApi;

namespace Backend {
	public class StickRadial : Hardware {
		public Button[] Buttons { get; set; } = {};
		public double Deadzone { get => deadzone; set {
			if (value < 0 || value > 1d) throw new SettingNotProportionException(
				"Deadzone must be a ratio of the trackpad's radius ([0, 1]).");
			this.deadzone = value;
		} }
		public double AngleOffset { get => angleOffset / Math.PI; set => angleOffset = value * Math.PI; }
		public bool IncrementsLeftElseRight { get; set; } = true;
		public bool TapsElseHolds { get; set; }

		private double deadzone = 0.1;
		private double angleOffset;
		private int? previousSliceIndex;

		public override void DoEvent(api.IInputData input) {
			var positional = input as api.IPositional ?? throw new ArgumentException(input + " isn't coordinal.");

			if (positional.IsRelease) {
				if (!TapsElseHolds) foreach (var b in Buttons) b.Release();
				previousSliceIndex = null;
				return;
			}
			
			// compute polar coordinates
			var (r, theta) = base.CartesianToPolar(positional.Position.x, positional.Position.y);
			if (r < deadzone * Int16.MaxValue) {
				foreach (var b in Buttons) b.Release();
				return;
			}

			// adjust angle to start from the offset
			theta = (theta - angleOffset) % (2 * Math.PI);
			if (theta < 0) theta += 2 * Math.PI;

			// find the radial slice which the user has pressed
			double sliceSize = (2 * Math.PI) / Buttons.Length;
			if (Double.IsNaN(sliceSize)) return;
			int indexOfPressed = (int)(theta / sliceSize);

			// press the pressed slice while releasing all else
			if (!IncrementsLeftElseRight) {
				indexOfPressed = Buttons.Length - 1 - indexOfPressed;
			}
			if (TapsElseHolds) {
				if (!previousSliceIndex.HasValue || previousSliceIndex != indexOfPressed)
					Buttons[indexOfPressed].Tap();
				previousSliceIndex = indexOfPressed;
			} else {
				for (int i = 0; i < Buttons.Length; i++) {
					if (i == indexOfPressed) Buttons[i].Press();
					else Buttons[i].Release();
				}
			}
		}

		public override void ReleaseAll() { foreach (var b in Buttons) b.ReleaseAll(); }

		public override void Unfreeze(api.IInputData newInput) => this.DoEvent(newInput);
	}
}