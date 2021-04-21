using System;
using Newtonsoft.Json;
using api = SteamControllerApi;

namespace Backend {
	public class TriggerTrigger : Hardware {
		public bool IsLeftElseRight { get; set; }
		public bool IncludeSwitchInRange { get; set; }
		[JsonIgnore]
		public override string HardwareType => "Trigger";

		private const double softRange = 237;
		
		public override void DoEvent(api.InputData e) {
			var pull = e.TriggerPull ?? throw new ArgumentException(e + " must be a trigger pull.");

			if (!IncludeSwitchInRange) pull = (byte)Math.Clamp((pull / softRange) * 255, 0, 255);

			if (IsLeftElseRight) robot.PullLTrigger(pull);
			else robot.PullRTrigger(pull);
		}

		public override void ReleaseAll() {}
	}
}