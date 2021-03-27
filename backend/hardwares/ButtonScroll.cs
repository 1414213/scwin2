using System;
using System.Threading;
using System.Threading.Tasks;

//using api = SteamControllerApi;


namespace Backend {
	public class ButtonScroll : Button {
		public double Amount { get; set; } = 1;
		public bool AsClicks { get; set; } = true;
		public bool IsContinuous { get; set; }

		private double amountStore;
		private Task doScroll;

		public ButtonScroll() {
			this.doScroll = new Task(() => {
				while (IsPressed) {
					Thread.Sleep(10);
					amountStore += Amount;
					robot.ScrollMouseWheel((int)amountStore, asClicks: AsClicks);
					amountStore -= (int)amountStore;
				}
			});
		}

		public ButtonScroll(int amount, bool asClicks, bool IsContinuous = false) : this() {
			this.Amount = amount;
			this.AsClicks = asClicks;
		}

		protected override void PressImpl() {
			if (IsContinuous) doScroll.Start();
			else robot.ScrollMouseWheel((int)Amount, asClicks: AsClicks);
		}

		protected override void ReleaseImpl() {}

		public void ScrollOneClick() => robot.ScrollMouseWheel(1, asClicks: false);
	}
}