using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using api = SteamControllerApi;
using Robot;


namespace Backend {
	// NOTE: If PullThreshold is not set on construction, then it defaults to 0.
	public class TriggerButton : Hardware {
		public Button Button { get; set; } = new ButtonKey(Key.None);
		public double PullThreshold {
			get => this.pullThreshold;
			set {
				if (value < 0 || value > 1.0)
					throw new SettingNotProportionException("PullThreshold must be between 0 and 1.");
				else this.pullThreshold = value;
			}
		}
		public bool IncludeSwitchInRange { get; set; } = false;

		private double pullThreshold = 0.5;
		private const double softRange = 237;

		public TriggerButton() => this.pullThreshold = 0.5;

		public TriggerButton(Key key, double pullThreshold, bool includeSwitchInRange = false) {
			this.Button = new ButtonKey(key);
			this.PullThreshold = pullThreshold;
			this.IncludeSwitchInRange = includeSwitchInRange;
		}

		public override void DoEvent(api.IInputData input) {
			// TriggerButton will always repeat press and release inputs to the wrapped button, so that
			// that button's logic can then decide whether to be repetitious
			byte pullDistance = (input as api.ITriggerData
			                     ?? throw new ArgumentException(input + " isn't a trigger pull.")).Trigger;

			if (!IncludeSwitchInRange) pullDistance = (byte)Math.Clamp((pullDistance / softRange) * 255, 0, 255);

			if (pullDistance > (PullThreshold * 255)) {
				// else if trigger is below its threshold
				Button.Press();
			} else {
				Button.Release();
			}
		}

		public override void ReleaseAll() => Button.ReleaseAll();

		public override void Unfreeze(api.IInputData newInput) => this.DoEvent(newInput);
	}
}