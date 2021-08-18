using System;
using System.Diagnostics;
using Newtonsoft.Json;
using api = SteamControllerApi;

namespace Backend {
	public abstract class Trackpad : SmoothInt, MAcceleration {
		public double Acceleration { get; set; } = 2;
		public int AccelerationLowerBoundary { get; set; } = 2000;
		public int AccelerationUpperBoundary { get; set; } = 1700;

		public Button DoubleTapButton { get; set; } = new ButtonKey();
		public bool IsDoubleTapHeld { get; set; }

		private MAcceleration accel => this as MAcceleration;

		private long tapTime = 250; // milliseconds
		private Stopwatch stopwatch = new Stopwatch();
		private bool doingSecondTap;

		public override void DoEvent(api.IInputData input) {
			var e = input as api.ITrackpadData ?? throw new ArgumentException("Not trackpad data.");
			
			// Code for double tapping to press a button.
			// This is a mess of state checks, not sure how else to do this.
			if (doingSecondTap) {
				if (stopwatch.ElapsedMilliseconds < tapTime) {
					if (IsDoubleTapHeld) {
						if (e.IsPress) {
							DoubleTapButton.Press();
							stopwatch.Reset();
						} else {
							DoubleTapButton.Release();
							doingSecondTap = false;
						}
					} else {
						if (e.TimeHeld.HasValue) { // Is release event.
							if (e.TimeHeld.Value < tapTime) DoubleTapButton.Tap();
							stopwatch.Reset();
							doingSecondTap = false;
						}
					}
				} else this.FirstPress(e);
			} else this.FirstPress(e);

			// After performing shared functionality, execute specific function.
			this.DoEventImpl(e);
		}

		public override void ReleaseAll() {
			DoubleTapButton.Release();
			this.ReleaseAllImpl();
		}

		protected abstract void DoEventImpl(api.ITrackpadData e);

		protected abstract void ReleaseAllImpl();

		protected (double x, double y) AccelerateInput(int x, int y, double startingSensitivity) => accel.AccelerateInput(x, y, startingSensitivity);

		private void FirstPress(api.ITrackpadData e) {
			if (e.TimeHeld.HasValue) { // Is release event.
				if (e.TimeHeld.Value < tapTime) {
					doingSecondTap = true;
					stopwatch.Restart();
				}
			}
		}
	}
}