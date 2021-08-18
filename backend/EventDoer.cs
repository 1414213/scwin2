using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Backend {
	using SteamControllerApi;

	public class EventDoer {
		public struct ActionLayer {
			public string name;
			public Dictionary<string, Hardware?> inputMap;
			public bool isTransparent, isFrozen;


			public ActionLayer(
				string name, Dictionary<string, Hardware?> inputMap, bool isTransparent, bool isFrozen
			) {
				this.name = name;
				this.inputMap = inputMap;
				this.isTransparent = isTransparent;
				this.isFrozen = isFrozen;
			}
		}

		public LinkedList<ActionLayer> ActionLayering { get; } = new ();
		public InputMapper.Map Map { get => map; init {
			map = value;
			ActionLayering.AddLast(new ActionLayer(map.Name, map.InputMap, false, false));
		} }

		private InputMapper.Map map = new InputMapper.Map();

		// Stuff for changing state of this EventDoer from the hardware classes.
		// Currently Hardware stores a static reference to some EventDoer object so to connect multiple
		// steamcons the references would need to be swapped for each steamcon.
		public int Debug { get; init; }

		private ConcurrentQueue<IInputData> eventPipe = new ();
		private Task processEvents;
		private readonly object actionLayeringLock = new {};
		private Controller steamcon;
		
		private EventDoer(bool createVirtualGamepad, Controller steamcon) {
			Hardware.Init(createVirtualGamepad, this);
			this.steamcon = steamcon;

			// For if event generation from DoEvent methods is added.  Currently does nothing.
			this.processEvents = Task.Run(() => {
				while (true) {
					while (eventPipe.IsEmpty) {} // Wait for side effects to be added.
					var e = (IInputData?)null;
					_ = eventPipe.TryDequeue(out e);
					if (e is null) continue;
					lock (actionLayeringLock) {
						for (var n = ActionLayering.Last; n != null; n = n.Previous) {
							ref var layer = ref n!.ValueRef;
							if (layer.inputMap.ContainsKey(e.Identity)) {
								if (layer.isFrozen) {
									layer.isFrozen = false;
									layer.inputMap[e.Identity]?.Unfreeze(e);
								} else {
									layer.inputMap[e.Identity]?.DoEvent(e);
								}
								break;
							} else if (!layer.isTransparent) {
								break;
							}
						}
					}
				}
			});
		}

		public EventDoer(InputMapper.Map map, Controller steamcon, bool createVirtualGamepad) : this(
			createVirtualGamepad,
			steamcon
		) {
			this.Map = map;
		}

		~EventDoer() {
			foreach (var layer in ActionLayering) foreach (var entry in layer.inputMap) entry.Value?.ReleaseAll();
		}

		/// <summary>Take in input and then simulate input.</summary>
		public void DoEvents(IList<IInputData> events) {
			foreach (var e in events) eventPipe.Enqueue(e);
		}

		public IInputData GetStateOf(KeyInternal key) => steamcon.State[key];

		public IInputData GetStateOfButton(Key key) => steamcon.State[key.ToInternal()];

		public ITriggerData GetStateOfTrigger(bool leftElseRight) {
			return (ITriggerData)steamcon.State[leftElseRight ? KeyInternal.LTrigger : KeyInternal.RTrigger];
		}

		public IPositional GetStateOfStick() => (IPositional)steamcon.State[KeyInternal.Stick];

		public IMotionData GetStateOfMotion() => (IMotionData)steamcon.State[KeyInternal.Motion];

		public long GetTimingOf(Key key) => steamcon.StateTimings[key.ToInternal()].ElapsedMilliseconds;

		//public void AddSideEffect(SideEffect sideEffect) => sideEffectsPipe.Enqueue(sideEffect);

		/// <summary>Thread-safe.</summary>
		public void AddActionLayer(string name, bool isTransparent) {
			lock (actionLayeringLock) {
				if (Map.ActionMaps.ContainsKey(name)) {
					ref var last = ref ActionLayering.Last!.ValueRef;
					last.isFrozen = true;
					ActionLayering.AddLast(
						new ActionLayer(name, Map.ActionMaps[name], isTransparent, false));
				} else {
					throw new ActionMapNotFoundException(name);
				}
			}
		}

		/// <summary>Thread-safe.</summary>
		public void RemoveActionLayer(string name) {
			lock (actionLayeringLock) {
				for (var n = ActionLayering.First!.Next; n != null; n = n.Next) if (n.Value.name == name) {
					ActionLayering.Remove(n);
					break;
				}
			}
		}

		// private void HandleSideEffects() {
		// 	bool wasSuccessful;
		// 	SideEffect sideEffect;

		// 	while (!sideEffectsPipe.IsEmpty) {
		// 		wasSuccessful = sideEffectsPipe.TryDequeue(out sideEffect!);
		// 		if (wasSuccessful) switch (sideEffect) {
		// 			// match for different side effects the event doer needs to produce
		// 			case ActionMapAddition a: {
		// 				this.AddActionLayer(a.name, a.isTransparent);
		// 				break;
		// 			}
		// 			case ActionMapRemoval r: {
		// 				this.RemoveActionLayer(r.name);
		// 				break;
		// 			}
		// 			default: throw new NotImplementedException(
		// 				$"SideEffect type {sideEffect.GetType().ToString()} doesn't exist.");
		// 		}
		// 	}
		// }
	}
}