using System;
using System.Collections.Concurrent;

using Newtonsoft.Json;

using api = SteamControllerApi;
using Robot;


namespace Backend {
	public abstract class Hardware {
		[JsonIgnore]
		public abstract string HardwareType { get; }
		// this might need to be local later on
		[JsonIgnore]
		public static ConcurrentQueue<SideEffect> sideEffectsPipe = null!;

		protected static IRobot robot = null!;

		public static void Init(bool createVirtualGamepad, ConcurrentQueue<SideEffect> sideEffectsPipe) {
			robot = new WindowsRobot(createVirtualGamepad);
			Hardware.sideEffectsPipe = sideEffectsPipe;
		}

		public abstract void DoEvent(api.InputData e);
		public abstract void ReleaseAll();
	}
}