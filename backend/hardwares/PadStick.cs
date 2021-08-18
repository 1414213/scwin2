using System;
using SteamControllerApi;
using api = SteamControllerApi;

namespace Backend {
	public class PadStick : Trackpad {
		public double Deadzone { get => stick.Deadzone; set => stick.Deadzone = value; }
		public double Gradingzone { get => stick.Gradingzone; set => stick.Gradingzone = value; }
		public bool IsLeftElseRight { get => stick.IsLeftElseRight; set => stick.IsLeftElseRight = value; }

		private StickStick stick = new StickStick{ Deadzone = 0.2, Gradingzone = 0.8 };

		protected override void DoEventImpl(api.ITrackpadData input) {
			var position = input.Position;

			// Handle press or release event.
			if (input.IsPress) {
				stick.DoEvent(input);
			} else if (input.IsRelease) {
				stick.DoEvent(input);
				stick.DoEvent(new api.StickData((0, 0), api.Flags.None));
			} else {
				throw new ArgumentException(input + " isn't a trackpad press.");
			}
		}

		protected override void ReleaseAllImpl() {}

		public override void Unfreeze(IInputData newInput) => this.DoEvent(newInput);

		public void SetStick(StickStick stick) => this.stick = stick;
	}
}