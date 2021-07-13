using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using Newtonsoft.Json;

using api = SteamControllerApi;
using Robot;


namespace Backend {
	public abstract class Hardware {
		[JsonIgnore]
		protected static ConcurrentQueue<SideEffect> SideEffectsPipe => EventDoer?.SideEffectsPipe
			?? throw new InvalidOperationException("Hardware has yet to be initialized; call Hardware.Init().");
		[JsonIgnore]
		protected static IRobot robot = null!;
		[JsonIgnore]
		protected static LinkedList<EventDoer.ActionLayer> actionLayering => EventDoer?.ActionLayering
			?? throw new InvalidOperationException("Hardware has yet to be initialized; call Hardware.Init().");

		protected static EventDoer? EventDoer { get; private set; }

		public static void Init(bool createVirtualGamepad, EventDoer eventDoer) {
			robot = new WindowsRobot(createVirtualGamepad);
			Hardware.EventDoer = eventDoer;
		}

		public abstract void DoEvent(api.IInputData input);
		public abstract void ReleaseAll();

		/// <summary>r is the range of a signed short, angle is between -PI and PI.</summary>
		protected (double r, double theta) CartesianToPolar(int x, int y) {
			double r = Math.Sqrt(x * x + y * y);
			double theta = Double.NaN;
			if (y >= 0 && r != 0) theta = Math.Acos(x / r);
			else if (y < 0)       theta = -Math.Acos(x / r);
			else if (r == 0)      theta = Double.NaN;
			return (r, theta);
		}

		protected (double r, double cos, double sin) CartesianToAngle(int x, int y) {
			double r = Math.Sqrt(x * x + y * y);
			return (r, x / r, y / r);
		}
	}
}