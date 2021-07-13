using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

		public int Debug { get; init; }
		public LinkedList<ActionLayer> ActionLayering { get; } = new ();

		public ConcurrentQueue<SideEffect> SideEffectsPipe { get; } = new ();
		public InputMapper.Map Map { get => map; init {
			map = value;
			ActionLayering.AddLast(new ActionLayer(map.Name, false, map.InputMap));
		} }
		
		InputMapper.Map map = new ();

		public EventDoer(bool createVirtualGamepad) => Hardware.Init(createVirtualGamepad, this);

		public EventDoer(InputMapper.Map map, bool createVirtualGamepad) : this(createVirtualGamepad) {
			this.Map = map;
		}

		~EventDoer() {
			foreach (var layer in ActionLayering) foreach (var entry in layer.inputMap) entry.Value?.ReleaseAll();
		}

		//public void InitHardware(bool createVirtualGamepad) => Hardware.Init(createVirtualGamepad, this);

		public void DoEvents(IList<api.IInputData> events) {
			foreach (var e in events) {
				ActionLayer layer;
				for (var n = ActionLayering.Last; n != null; n = n.Previous) {
					layer = n!.Value;
					if (layer.inputMap.ContainsKey(e.Identity)) {
						this.DoAction(e, layer.inputMap[e.Identity]);
						break;
					} else if (!layer.isTransparent) break;
				}
			}
		}

		private void DoAction(api.IInputData e, Hardware? mapEntry) {
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
						for (var n = ActionLayering.First!.Next; n != null; n = n.Next) if (n.Value.name == r.name) {
							ActionLayering.Remove(n);
							break;
						}
						break;
					}
					default: throw new NotImplementedException(
						$"SideEffect type {sideEffect.GetType().ToString()} doesn't exist."
					);
				}
			}
		}
	}
}