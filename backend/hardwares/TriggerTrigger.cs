using System;
using api = SteamControllerApi;

namespace Backend {
	public class TriggerTrigger : Hardware {
		public bool IsLeftElseRight { get; set; }
		public bool IncludeSwitchInRange { get; set; }

		private const double softRange = 237;
		
		public override void DoEvent(api.IInputData input) {
			var pull = (input as api.ITriggerData ?? throw new ArgumentException(input + " must be a trigger pull."))
			           .Trigger;

			if (!IncludeSwitchInRange) pull = (byte)Math.Clamp((pull / softRange) * 255, 0, 255);

			if (IsLeftElseRight) robot.PullLTrigger(pull);
			else robot.PullRTrigger(pull);
		}

		public override void ReleaseAll() {}

		public override void Unfreeze(api.IInputData newInput) => this.DoEvent(newInput);
	}
}