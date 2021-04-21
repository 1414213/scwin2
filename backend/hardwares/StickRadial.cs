using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using api = SteamControllerApi;

namespace Backend {
	public class StickRadial : Hardware {
		public IList<Button> Buttons { get; set; } = new List<Button>();
		public double Deadzone {
			get => this.deadzone;
			set {
				if (value < 0 || value > 1d) throw new SettingNotProportionException(
					"Deadzone must be a ratio of the trackpad's radius ([0, 1])."
				);
				this.deadzone = value;
			}
		}
		public double AngleOffset {
			get => this.angleOffset / Math.PI;
			set => this.angleOffset = value * Math.PI;
		}
		public bool IncrementsLeftElseRight { get; set; } = true;
		public bool TapsElseHolds { get; set; }
		[JsonIgnore]
		public override string HardwareType => "Thumbstick";

		private double deadzone = 0.1;
		private double angleOffset;
		private int? previousSliceIndex;

		public override void DoEvent(api.InputData e) {
			var coord = e.Coordinates ?? throw new ArgumentException(e + " isn't coordinal.");

			if ((e.Flags & api.Flags.Released) == api.Flags.Released) {
				if (!TapsElseHolds) foreach (var b in Buttons) b.Release();
				previousSliceIndex = null;
				return;
			}
			
			// compute polar coordinates
			double r = Math.Sqrt(coord.x * coord.x + coord.y * coord.y);
			double theta = Double.NaN;
			if (coord.y >= 0 && r != 0) theta = Math.Acos(coord.x / r);
			else if (coord.y < 0) theta = -Math.Acos(coord.x / r);
			else if (r == 0) theta = Double.NaN;
			if (theta < 0) theta += 2 * Math.PI;

			if (r < deadzone * Int16.MaxValue) {
				foreach (var b in Buttons) b.Release();
				return;
			}

			// adjust angle to start from the offset
			theta -= angleOffset;
			if (theta < 0) theta += 2 * Math.PI;

			// find the radial slice which the user has pressed
			double sliceSize = (2 * Math.PI) / Buttons.Count;
			int indexOfPressed = (int)(theta / sliceSize);

			// press the pressed slice while releasing all else
			if (!IncrementsLeftElseRight) {
				indexOfPressed = Buttons.Count - 1 - indexOfPressed;
			}
			if (TapsElseHolds) {
				if (!previousSliceIndex.HasValue || previousSliceIndex != indexOfPressed)
					Buttons[indexOfPressed].Tap();
				previousSliceIndex = indexOfPressed;
			}
			else {
				for (int i = 0; i < Buttons.Count; i++) {
					if (i == indexOfPressed) Buttons[i].Press();
					else Buttons[i].Release();
				}
			}
		}

		public override void ReleaseAll() {
			foreach (var b in Buttons) b.ReleaseAll();
		}
	}
}