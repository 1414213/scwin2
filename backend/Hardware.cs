using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using Newtonsoft.Json;

using api = SteamControllerApi;
using Robot;


namespace Backend {
	public abstract class Hardware {
		[JsonIgnore]
		protected static ConcurrentQueue<SideEffect> SideEffectsPipe => eventDoer?.SideEffectsPipe
			?? throw new InvalidOperationException("Hardware has yet to be initialized; call Hardware.Init().");
		[JsonIgnore]
		protected static IRobot robot = null!;
		[JsonIgnore]
		protected static LinkedList<EventDoer.ActionLayer> actionLayering => eventDoer?.ActionLayering
			?? throw new InvalidOperationException("Hardware has yet to be initialized; call Hardware.Init().");

		static EventDoer? eventDoer;

		public static void Init(bool createVirtualGamepad, EventDoer eventDoer) {
			robot = new WindowsRobot(createVirtualGamepad);
			Hardware.eventDoer = eventDoer;
		}

		public abstract void DoEvent(api.InputData e);
		public abstract void ReleaseAll();
	}
}