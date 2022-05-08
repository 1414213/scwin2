using System;
using System.Threading;
using System.Threading.Tasks;

namespace Input {
	public class ButtonTemporal : Button {
		public Button Short { get; set; } = new ButtonKey();
		public Button Long { get; set; } = new ButtonKey();
		public int TemporalThreshold { get; set; } = 500; // milliseconds
		public bool IsLongPressHeld { get; set; }

		CancellationTokenSource? cancel;
		Task? doHold;

		protected override void PressImpl() {
			if (IsLongPressHeld) {
				cancel = new CancellationTokenSource();
				doHold = Task.Run(() => {
					var myToken = cancel.Token;
					Thread.Sleep(TemporalThreshold);
					// return if press time was shorter than threshold to trigger a press of Long
					myToken.ThrowIfCancellationRequested();
					Long.Press();
				}, cancel.Token);
			}
		}

		protected override void ReleaseImpl() {
			var timeHeld = base.Input?.TimeHeld ?? 0;
			if (IsLongPressHeld) {
				if (timeHeld < TemporalThreshold) {
					cancel?.Cancel(); // shouldn't be null
					Short.Tap();
				}
				Long.Release();
			} else {
				if (timeHeld < TemporalThreshold) Short.Tap();
				else Long.Tap();
			}
		}
	}
}