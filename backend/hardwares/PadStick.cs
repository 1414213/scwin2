using System;
using System.Collections.Generic;

using api = SteamControllerApi;


namespace Backend {
	public class PadStick : Trackpad {
		public double Deadzone {
			get => this.stick.Deadzone;
			set {
				if (value < 0 || value > 1.0) throw new ArgumentException("Deadzone must be between 0 and 1.0");
				this.stick.Deadzone = value;
			}
		}
		public double OuterLimit {
			get => this.outerLimit;
			set {
				if (value < 0 || value > 1.0) throw new ArgumentException("OuterLimit must be between 0 and 1.0");
				this.outerLimit = value;
			}
		}
		public bool IsLeftElseRight {
			get => this.stick.IsLeftElseRight;
			set => this.stick.IsLeftElseRight = value;
		}

		private StickStick stick = new StickStick();
		private double outerLimit = 0.8;

		protected override void DoEventImpl(api.InputData e) {
			var (x, y) = e.Coordinates ?? throw new ArgumentException(e + " must be a coordinal event.");

			// convert to polar coordinates
			double r = Math.Sqrt((x * x) + (y * y));
			double theta = 0;
			if (y >= 0 && r != 0) theta = Math.Acos(x / r);
			else if (y < 0) theta = -Math.Acos(x / r);
			else if (r == 0) theta = Double.NaN;

			// normalize r to an equivalent value between 0 and the outer limit
			if (r >= outerLimit * Int16.MaxValue) {
				r = Int16.MaxValue;
			} else r = r / outerLimit;

			// convert back into cartesian coordinates
			if (!Double.IsNaN(theta)) {
				x = Convert.ToInt16(r * Math.Cos(theta));
				y = Convert.ToInt16(r * Math.Sin(theta));
			}

			// handle press or release event
			if ((e.Flags & api.Flags.Pressed) == api.Flags.Pressed) {
				stick.DoEvent(new api.InputData(api.Key.StickPush, x, y, api.Flags.AbsoluteMove));
			} else if ((e.Flags & api.Flags.Released) == api.Flags.Released) {
				stick.DoEvent(new api.InputData(api.Key.StickPush, x, y, api.Flags.AbsoluteMove));
				stick.DoEvent(new api.InputData(api.Key.StickPush, 0, 0, api.Flags.AbsoluteMove));
			} else throw new ArgumentException(e + " isn't a trackpad press.");
		}

		protected override void ReleaseAllImpl() {}

		public void SetStick(StickStick stick) => this.stick = stick;
	}
}