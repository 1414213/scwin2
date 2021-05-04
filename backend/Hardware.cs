using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using Newtonsoft.Json;

using api = SteamControllerApi;
using Robot;


namespace Backend {
	public abstract class Hardware {
		[JsonIgnore]
		public static ConcurrentQueue<SideEffect> sideEffectsPipe = null!;
		[JsonIgnore]
		protected static IRobot robot = null!;
		[JsonIgnore]
		protected static LinkedList<EventDoer.ActionLayer> actionLayering = null!;

		public static void Init(
			bool createVirtualGamepad,
			ConcurrentQueue<SideEffect> sideEffectsPipe,
			LinkedList<EventDoer.ActionLayer> actionLayering
		) {
			robot = new WindowsRobot(createVirtualGamepad);
			Hardware.sideEffectsPipe = sideEffectsPipe;
			Hardware.actionLayering = actionLayering;
		}

		protected Hardware() {
			if (sideEffectsPipe == null || actionLayering == null) {
				throw new InvalidOperationException("Hardware has yet to be initialized; call Hardware.Init().");
			}
		}

		public abstract void DoEvent(api.InputData e);
		public abstract void ReleaseAll();
	}
}