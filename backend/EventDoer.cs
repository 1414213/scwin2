using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using api = SteamControllerApi;


namespace Backend {
	public class EventDoer {
		public LinkedList<ActionLayer> ActionLayering => actionLayering;

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
			public Dictionary<string, InputMapper.InputTypeTable> map;

			public ActionLayer(string name, bool isTransparent, Dictionary<string, InputMapper.InputTypeTable> inputMap) {
				this.name = name;
				this.isTransparent = isTransparent;
				this.map = inputMap;
			}
		}

		private InputMapper.Map map;
		private Dictionary<api.Key, Task> heldLongPressTasks;
		private Dictionary<api.Key, CancellationTokenSource> heldLongPressTaskTokens;
		private LinkedList<ActionLayer> actionLayering = new LinkedList<ActionLayer>();

		private ConcurrentQueue<SideEffect> sideEffectsPipe = new ConcurrentQueue<SideEffect>();

		public EventDoer(InputMapper.Map inputMap, bool createVirtualGamepad = true) {
			this.map = inputMap;
			this.heldLongPressTasks = new Dictionary<api.Key, Task>();
			this.heldLongPressTaskTokens = new Dictionary<api.Key, CancellationTokenSource>();

			actionLayering.AddLast(new ActionLayer(this.map.Name, false, this.map.InputMap));

			Hardware.Init(createVirtualGamepad, this.sideEffectsPipe, this.actionLayering);
		}
		~EventDoer() {
			foreach (var layer in actionLayering) this.ReleaseAll(layer.map);
		}

		public void DoEvents(IList<api.InputData> events) {
			foreach (api.InputData e in events) {
				var itr = actionLayering.Last;
				for (int i = 0; i < actionLayering.Count; i++) {
					if (itr!.Value.map.ContainsKey(e.Key.ToString())) {
						this.DoAction(e, itr.Value.map[e.Key.ToString()]);
						break;
					} else if (!itr.Value.isTransparent) break;
					itr = itr.Previous;
				}
			}
		}

		private void DoAction(api.InputData e, InputMapper.InputTypeTable mapEntry) {
			var regular = mapEntry.Regular;
			var shortpress = mapEntry.ShortPress;
			var longpress = mapEntry.LongPress;

			// handle regular actions (i.e. analog inputs, normal button presses)
			regular?.DoEvent(e);

			// handle short and long presses
			// for now, this only performs input if applied to a button
			if (shortpress != null || longpress != null) {
				if ((e.Flags & api.Flags.Pressed) == api.Flags.Pressed) {
					heldLongPressTaskTokens[e.Key] = new CancellationTokenSource();
					if (mapEntry.IsLongPressHeld) {
						// on press, starts task to measure if release occurs before given time
						heldLongPressTasks[e.Key] = Task.Run(() => {
							Thread.Sleep(mapEntry.TemporalThreshold);
							// return if press time was shorter than threshold to trigger the long press button
							heldLongPressTaskTokens[e.Key].Token.ThrowIfCancellationRequested();
							if (longpress is Button b) b.Press();
						}, heldLongPressTaskTokens[e.Key].Token);
					}
				} else if ((e.Flags & api.Flags.Released) == api.Flags.Released) {
					// temporal binidngs only apply to things that can be pressed
					long timeHeld = e.TimeHeld ?? 0;

					if (mapEntry.IsLongPressHeld) {
						if (timeHeld < mapEntry.TemporalThreshold) {
							heldLongPressTaskTokens[e.Key].Cancel();
							if (shortpress is Button b) { b.Tap(); }
						} else { if (longpress is Button b) b.Release(); }
					} else {
						if (timeHeld < mapEntry.TemporalThreshold) { if (shortpress is Button b) b.Tap(); }
						else { if (longpress is Button b) b.Tap(); }
					}
				}
			}

			// Handle any side effects produced by the hardwares
			bool wasSuccessful;
			SideEffect sideEffect;
			while (!sideEffectsPipe.IsEmpty) {
				wasSuccessful = sideEffectsPipe.TryDequeue(out sideEffect!);
				if (wasSuccessful) switch (sideEffect) {
					// match for different side effects the event doer needs to produce
					case ActionMapAddition a: {
						if (map.ActionMaps.ContainsKey(a.name)) {
							actionLayering.AddLast(new ActionLayer(a.name, a.isTransparent, map.ActionMaps[a.name]));
						} else throw new ActionMapNotFoundException(a.name);
						break;
					}
					case ActionMapRemoval r: {
						var itr = actionLayering.First!.Next;
						while (itr != null) {
							if (itr.Value.name == r.name) {
								actionLayering.Remove(itr);
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

		private void ReleaseAll(InputMapper.Map map) => this.ReleaseAll(map.InputMap);

		private void ReleaseAll(Dictionary<string, InputMapper.InputTypeTable> map) {
			foreach (var entry in map) {
				entry.Value.Regular?.ReleaseAll();
				entry.Value.ShortPress?.ReleaseAll();
				entry.Value.LongPress?.ReleaseAll();
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