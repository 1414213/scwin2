using System;
using api = SteamControllerApi;

namespace Backend {
	public class PadStick : Trackpad {
		public double Deadzone { get => stick.Deadzone; set => stick.Deadzone = value; }
		public double Gradingzone { get => stick.Gradingzone; set => stick.Gradingzone = value; }
		public bool IsLeftElseRight { get => stick.IsLeftElseRight; set => stick.IsLeftElseRight = value; }

		private StickStick stick = new StickStick{ Deadzone = 0.2, Gradingzone = 0.8 };

		protected override void DoEventImpl(api.InputData e) {
			var (x, y) = e.Coordinates ?? throw new ArgumentException(e + " must be a coordinal event.");

			// handle press or release event
			if ((e.Flags & api.Flags.Pressed) == api.Flags.Pressed) {
				stick.DoEvent(new api.InputData(api.Key.StickPush, x, y, api.Flags.AbsoluteMove));
			} else if ((e.Flags & api.Flags.Released) == api.Flags.Released) {
				stick.DoEvent(new api.InputData(api.Key.StickPush, x, y, api.Flags.AbsoluteMove));
				stick.DoEvent(new api.InputData(api.Key.StickPush, 0, 0, api.Flags.AbsoluteMove));
			} else {
				throw new ArgumentException(e + " isn't a trackpad press.");
			}
		}

		protected override void ReleaseAllImpl() {}

		public void SetStick(StickStick stick) => this.stick = stick;
	}
}