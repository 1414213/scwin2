using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using api = SteamControllerApi;


namespace Backend {
	public class EventDoer {
		// dumb boilerplate
		private class HeldLongPressEntry {
			public Task task;
			public bool doHold;

			public HeldLongPressEntry(Task task, bool doHold) {
				this.task = task;
				this.doHold = doHold;
			}
		}

		public class ActionLayer {
			public string name = "";
			public bool isTransparent;
			public Dictionary<string, Hardware?> inputMap;

			public ActionLayer(string name, bool isTransparent, Dictionary<string, Hardware?> inputMap) {
				this.name = name;
				this.isTransparent = isTransparent;
				this.inputMap = inputMap;
			}
		}

		public LinkedList<ActionLayer> ActionLayering { get; } = new ();

		public ConcurrentQueue<SideEffect> SideEffectsPipe { get; } = new ();
		// BUG
		// public InputMapper.Map Map { get => Map; init {
		// 	Map = value;
		// 	ActionLayering.AddLast(new ActionLayer(this.Map.Name, false, this.Map.InputMap));
		// } }
		public InputMapper.Map Map { get => map; init {
			map = value;
			ActionLayering.AddLast(new ActionLayer(map.Name, false, map.InputMap));
		} }
		
		Dictionary<api.Key, Task> heldLongPressTasks = new ();
		Dictionary<api.Key, CancellationTokenSource> heldLongPressTaskTokens = new ();
		InputMapper.Map map = new ();

		public EventDoer(bool createVirtualGamepad) => Hardware.Init(createVirtualGamepad, this);

		public EventDoer(InputMapper.Map map, bool createVirtualGamepad) : this(createVirtualGamepad) {
			this.Map = map;
		}

		~EventDoer() {
			foreach (var layer in ActionLayering) foreach (var entry in layer.inputMap) entry.Value?.ReleaseAll();
		}

		//public void InitHardware(bool createVirtualGamepad) => Hardware.Init(createVirtualGamepad, this);

		public void DoEvents(IList<api.InputData> events) {
			foreach (api.InputData e in events) {
				var itr = ActionLayering.Last;
				for (int i = 0; i < ActionLayering.Count; i++) {
					if (itr!.Value.inputMap.ContainsKey(e.Key.ToString())) {
						this.DoAction(e, itr.Value.inputMap[e.Key.ToString()]);
						break;
					} else if (!itr.Value.isTransparent) break;
					itr = itr.Previous;
				}
			}
		}

		private void DoAction(api.InputData e, Hardware? mapEntry) {
			mapEntry?.DoEvent(e);

			// Handle any side effects produced by the hardwares
			bool wasSuccessful;
			SideEffect sideEffect;
			while (!SideEffectsPipe.IsEmpty) {
				wasSuccessful = SideEffectsPipe.TryDequeue(out sideEffect!);
				if (wasSuccessful) switch (sideEffect) {
					// match for different side effects the event doer needs to produce
					case ActionMapAddition a: {
						if (Map.ActionMaps.ContainsKey(a.name)) {
							ActionLayering.AddLast(new ActionLayer(a.name, a.isTransparent, Map.ActionMaps[a.name]));
						} else throw new ActionMapNotFoundException(a.name);
						break;
					}
					case ActionMapRemoval r: {
						var itr = ActionLayering.First!.Next;
						while (itr != null) {
							if (itr.Value.name == r.name) {
								ActionLayering.Remove(itr);
								break;
							}
							itr = itr.Next;
						}
						break;
					}
					default: throw new NotImplementedException(
						$"SideEffect type {sideEffect.GetType().ToString()} doesn't exist."
					);
				}
			}
		}

		private static bool IsKeyButton(api.Key key) => key switch {
			api.Key.LTriggerPull => false,
			api.Key.RTriggerPull => false,
			api.Key.StickPush    => false,
			api.Key.GyroMove     => false,
			_                    => true
		};
	}
}