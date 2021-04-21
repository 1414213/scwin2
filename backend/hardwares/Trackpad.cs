using System;
using System.Diagnostics;
using Newtonsoft.Json;
using api = SteamControllerApi;

namespace Backend {
	public abstract class Trackpad : SmoothedHardware {
		public Button DoubleTapButton { get; set; } = new ButtonKey();
		public bool IsDoubleTapHeld { get; set; }
		[JsonIgnore]
		public override string HardwareType => "Trackpad";

		private long tapTime = 250; // milliseconds
		private Stopwatch stopwatch = new Stopwatch();
		private bool doingSecondTap;

		public override void DoEvent(api.InputData e) {
			// code for double tapping to press a button
			// this is a mess of state checks, not sure how else to do this
			if (doingSecondTap) {
				if (stopwatch.ElapsedMilliseconds < tapTime) {
					if (IsDoubleTapHeld) {
						if ((e.Flags & api.Flags.Pressed) == api.Flags.Pressed) {
							DoubleTapButton.Press();
							stopwatch.Reset();
						}
						else {
							DoubleTapButton.Release();
							doingSecondTap = false;
						}
					}
					else {
						if (e.TimeHeld.HasValue) { // is release event
							if (e.TimeHeld.Value < tapTime) DoubleTapButton.Tap();
							stopwatch.Reset();
							doingSecondTap = false;
						}
					}
				}
				else this.FirstPress(e);
			}
			else this.FirstPress(e);

			// after performing shared functionality, execute specific function
			this.DoEventImpl(e);
		}

		public override void ReleaseAll() {
			DoubleTapButton.Release();
			this.ReleaseAllImpl();
		}

		protected abstract void DoEventImpl(api.InputData e);
		protected abstract void ReleaseAllImpl();

		private void FirstPress(api.InputData e) {
			if (e.TimeHeld.HasValue) { // is release event
				if (e.TimeHeld.Value < tapTime) {
					doingSecondTap = true;
					stopwatch.Restart();
				}
			}
		}
	}
}