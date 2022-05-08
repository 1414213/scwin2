using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Input {
	using SteamControllerApi;

	public static class EventDoer {
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

		static public LinkedList<ActionLayer> ActionLayering { get; } = new ();
		static public InputMapper.Map Map { get => map; set {
			map = value;
			ActionLayering.AddLast(new ActionLayer(map.Name, map.InputMap, false, false));
		} }
		static public IInputData[] CurrentEvents => currentEvents;

		static private InputMapper.Map map = new InputMapper.Map();
		static private IInputData[] currentEvents = {};

		// Stuff for changing state of this EventDoer from the hardware classes.
		// Currently Hardware stores a static reference to some EventDoer object so to connect multiple
		// steamcons the references would need to be swapped for each steamcon.
		static public int Debug { get; set; }

		static private ConcurrentQueue<IInputData> eventPipe = new ();
		static private Task processEvents;
		static private readonly object actionLayeringLock = new {};
		
		static EventDoer() {
			// For if event generation from DoEvent methods is added.  Currently does nothing.
			EventDoer.processEvents = Task.Run(() => {
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

		/// <summary>Take in input and then simulate input.</summary>
		static public void DoEvents(IInputData[] events) {
			EventDoer.currentEvents = events;
			foreach (var e in events) eventPipe.Enqueue(e);
		}

		//public void AddSideEffect(SideEffect sideEffect) => sideEffectsPipe.Enqueue(sideEffect);

		/// <summary>Thread-safe.</summary>
		static public void AddActionLayer(string name, bool isTransparent) {
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
		static public void RemoveActionLayer(string name) {
			lock (actionLayeringLock) {
				for (var n = ActionLayering.First!.Next; n != null; n = n.Next) if (n.Value.name == name) {
					ActionLayering.Remove(n);
					break;
				}
			}
		}
	}
}