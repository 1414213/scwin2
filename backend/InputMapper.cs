using System;
using System.Collections.Generic;
using System.IO;


using Newtonsoft.Json;

using api = SteamControllerApi;
using Robot;


namespace Backend {
	public static class InputMapper {
		// public class Map {
		// 	public Dictionary<string, Dictionary<string, Hardware>> InputMap { get; set; } = CreateBlankInputMap();
		// }
		public class Map {
			public string Name { get; set; } = "";
			public Dictionary<string, InputTypeTable> InputMap { get; set; } = CreateBlankInputMap();
			public Dictionary<string, Dictionary<string, InputTypeTable>> ActionMaps;

			public Map() {
				this.ActionMaps = new Dictionary<string, Dictionary<string, InputTypeTable>>();
			}
		}

		public class InputTypeTable {
			public Hardware? Regular { get; set; }
			public Hardware? ShortPress { get; set; }
			public Hardware? LongPress { get; set; }
			public int TemporalThreshold { get; set; } = 500;
			public bool IsLongPressHeld { get; set; }
		}

		public static string Directory { get; set; } = @"inputmappings\";

		/// <summary>
		/// Returns true if the map was opened and false if no map was found and a blank map
		/// was created instead.
		/// </summary>
		public static (Map, bool) Open(string mapName) {
			string mapPath = Directory + mapName + ".json";
			string jsonString;

			// create the directory to store the input maps in if it doesn't already exist
			FileInfo mapFileInfo = new FileInfo(mapPath);
			try { mapFileInfo.Directory.Create(); }
			catch (NullReferenceException) { throw new FileNotFoundException("Directory not found."); }

			if (!File.Exists(mapPath)) {
				Map blankInputMap = new Map();
				jsonString = JsonConvert.SerializeObject(blankInputMap, Formatting.Indented);
				File.WriteAllText(mapPath, jsonString);

				return (blankInputMap, false);
			}
			jsonString = File.ReadAllText(mapPath);

			var map = JsonConvert.DeserializeObject<Map>(
				jsonString,
				new JsonSerializerSettings{ TypeNameHandling = TypeNameHandling.Auto }
			);
			if (map == null) throw new Exception("Input map couldn't be opened.");
			else return (map, true);
		}

		public static void Save(Map inputMap, string mapName) {
			string jsonString = JsonConvert.SerializeObject(
				inputMap,
				Formatting.Indented,
				new JsonSerializerSettings{ TypeNameHandling = TypeNameHandling.Auto }
			);
			FileInfo file = new FileInfo(Directory + mapName + ".json");

			file.Directory.Create();
			File.WriteAllText(file.FullName, jsonString);
		}

		public static void Print(Map inputMap) {
			var im = inputMap.InputMap;
			foreach (Key k in Enum.GetValues(typeof(Key))) {
				Console.Write(k);
				foreach (KeyValuePair<string, InputTypeTable> entry in im) {
					Console.Write(entry.ToString());
				}
				Console.WriteLine();
			}
		}

		private static Dictionary<string, InputTypeTable> CreateBlankInputMap() {
			var inputMap = new Dictionary<string, InputTypeTable>();
			// keys are stored as strings so that they can be indexed in alphabetical order
			var keylist = new List<string>();

			foreach (api.Key k in Enum.GetValues(typeof(api.Key))) {
				if (k == api.Key.DPadLeft) continue;
				else if (k == api.Key.DPadUp) continue;
				else if (k == api.Key.DPadLeft) continue;
				else if (k == api.Key.DPadDown) continue;
				else keylist.Add(k.ToString());
			}
			keylist.Sort();
			
			foreach (string s in keylist) {
				api.Key key = api.Key.DPadDown; // placeholder
				foreach (api.Key k in Enum.GetValues(typeof(api.Key))) {
					if (k.ToString() == s) key = k;
				}

				InputTypeTable inputTypeTable = new InputTypeTable();
				inputMap.Add(key.ToString(), inputTypeTable);
			}

			return inputMap;
		}

		// public static Map BuildSubnautica() {
		// 	// var map = CreateBlankInputMap();
		// 	// map[api.Key.LTriggerPull.ToString()]["Push"] = new TriggerButton(0.5, Key.MouseRight);
		// 	// map[api.Key.RTriggerPull.ToString()]["Push"] = new TriggerButton(0.5, Key.MouseLeft);
		// 	// map[api.Key.LBumper.ToString()]["Tap"] = new Button(Key.F);
		// 	// map[api.Key.LGrip.ToString()]["Tap"] = new Button(Key.C);
		// 	// map[api.Key.RGrip.ToString()]["Tap"] = new Button(Key.Space);
		// 	// map[api.Key.StickPush.ToString()]["Push"] = new StickButtonCross(Key.D, Key.S, Key.A, Key.W, 0.3, hasOverlap: false);
		// 	// map[api.Key.B.ToString()]["Tap"] = new Button(Key.E);
		// 	// map[api.Key.A.ToString()]["Tap"] = new Button(Key.Q);
		// 	// map[api.Key.X.ToString()]["Tap"] = new Button(Key.R);
		// 	// map[api.Key.Y.ToString()]["Tap"] = new Button(Key.Tab);
		// 	// map[api.Key.RPadTouch.ToString()]["Tap"] = new Trackball(0, 10, false);
		// 	// map[api.Key.Forward.ToString()]["Tap"] = new Button(Key.Escape);
		// 	// map[api.Key.Back.ToString()]["Tap"] = new Button(Key.Tab);  //tab

		// 	// if I write it like above every key is bound to the last created button but using 
		// 	// collection initializers works.  Help.

		// 	var map2 = new Dictionary<string, Dictionary<string, Hardware>>{
		// 		{ api.Key.LTriggerPull.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Push", new TriggerButton(Key.MouseRight, 0.5) },
		// 		} },
		// 		{ api.Key.RTriggerPull.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Push", new TriggerButton(Key.MouseLeft, 0.5) },
		// 		} },
		// 		{ api.Key.LTriggerClick.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", null }
		// 		} },
		// 		{ api.Key.RTriggerClick.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", null }
		// 		} },
		// 		{ api.Key.LBumper.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new ButtonKey(Key.F) }
		// 		} },
		// 		{ api.Key.RBumper.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", null }
		// 		} },
		// 		{ api.Key.LGrip.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new ButtonKey(Key.C) }
		// 		} },
		// 		{ api.Key.RGrip.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new ButtonKey(Key.Space) }
		// 		} },
		// 		{ api.Key.StickPush.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Push", new StickButtonCross(Key.D, Key.W, Key.A, Key.S, 0.3, hasOverlap: true) }
		// 		} },
		// 		{ api.Key.StickClick.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new ButtonKey(Key.LeftShift) }
		// 		} },
		// 		{ api.Key.B.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new ButtonKey(Key.E) }
		// 		} },
		// 		{ api.Key.A.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new ButtonKey(Key.Q) }
		// 		} },
		// 		{ api.Key.X.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new ButtonKey(Key.R) }
		// 		} },
		// 		{ api.Key.Y.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new ButtonKey(Key.Tab) }
		// 		} },
		// 		{ api.Key.LPadTouch.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new PadButtonCross(Key.D, Key.W, Key.A, Key.S, 0.4, hasOverlap: true) }
		// 		} },
		// 		{ api.Key.LPadClick.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", null }
		// 		} },
		// 		{ api.Key.RPadTouch.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new Trackball(100, 50, true) }
		// 		} },
		// 		{ api.Key.RPadClick.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new PadButtonCross{
		// 				West = new ButtonScroll { Amount = -1, AsClicks = true },
		// 				East = new ButtonScroll { Amount = 1, AsClicks = true }
		// 			} }
		// 		} },
		// 		{ api.Key.Forward.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new ButtonKey(Key.Escape) }
		// 		} },
		// 		{ api.Key.Back.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new ButtonKey(Key.Tab) }
		// 		} },
		// 		{ api.Key.Steam.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", null }
		// 		} },
		// 		{ api.Key.GyroMove.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Push", null }
		// 		} },
		// 	};

		// 	return new Map{ InputMap=map2 };
		// }

		// public static Map BuildXbox() {
		// 	var map = new Dictionary<string, Dictionary<string, Hardware>> {
		// 		{ api.Key.LTriggerPull.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Push", new TriggerTrigger{ IsLeftElseRight = true } },
		// 		} },
		// 		{ api.Key.RTriggerPull.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Push", new TriggerTrigger{ IsLeftElseRight = false } },
		// 		} },
		// 		{ api.Key.LTriggerClick.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", null }
		// 		} },
		// 		{ api.Key.RTriggerClick.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", null }
		// 		} },
		// 		{ api.Key.LBumper.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new ButtonKey(Key.LBumper) }
		// 		} },
		// 		{ api.Key.RBumper.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new ButtonKey(Key.RBumper) }
		// 		} },
		// 		{ api.Key.LGrip.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new ButtonKey(Key.None) }
		// 		} },
		// 		{ api.Key.RGrip.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new ButtonKey(Key.None) }
		// 		} },
		// 		{ api.Key.StickPush.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Push", new StickStick{ Deadzone = 0.1, IsLeftElseRight = true } }
		// 		} },
		// 		{ api.Key.StickClick.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new ButtonKey(Key.LStickClick) }
		// 		} },
		// 		{ api.Key.B.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new ButtonKey(Key.Face_East) }
		// 		} },
		// 		{ api.Key.A.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new ButtonKey(Key.Face_South) }
		// 		} },
		// 		{ api.Key.X.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new ButtonKey(Key.Face_West) }
		// 		} },
		// 		{ api.Key.Y.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new ButtonKey(Key.Face_North) }
		// 		} },
		// 		{ api.Key.LPadTouch.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new PadButtonCross(Key.Dpad_Right, Key.Dpad_Up, Key.Dpad_Left, Key.Dpad_Down, 0.4) }
		// 		} },
		// 		{ api.Key.LPadClick.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", null }
		// 		} },
		// 		{ api.Key.RPadTouch.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new PadStick { Deadzone = 0.1, OuterLimit = 0.7, IsLeftElseRight = false } }
		// 		} },
		// 		{ api.Key.RPadClick.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new ButtonKey(Key.RStickClick) }
		// 		} },
		// 		{ api.Key.Forward.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new ButtonKey(Key.Start) }
		// 		} },
		// 		{ api.Key.Back.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new ButtonKey(Key.Back) }
		// 		} },
		// 		{ api.Key.Steam.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new ButtonKey(Key.GamepadHome) }
		// 		} },
		// 		{ api.Key.GyroMove.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Push", null }
		// 		} },
		// 	};
		// 	return new Map{ InputMap=map };
		// }

		// public static Map BuildXboxRelative() {
		// 	var map = new Dictionary<string, Dictionary<string, Hardware>> {
		// 		{ api.Key.LTriggerPull.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Push", new TriggerTrigger{ IsLeftElseRight = true } },
		// 		} },
		// 		{ api.Key.RTriggerPull.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Push", new TriggerTrigger{ IsLeftElseRight = false } },
		// 		} },
		// 		{ api.Key.LTriggerClick.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", null }
		// 		} },
		// 		{ api.Key.RTriggerClick.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", null }
		// 		} },
		// 		{ api.Key.LBumper.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new ButtonKey(Key.LBumper) }
		// 		} },
		// 		{ api.Key.RBumper.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new ButtonKey(Key.RBumper) }
		// 		} },
		// 		{ api.Key.LGrip.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new ButtonKey(Key.None) }
		// 		} },
		// 		{ api.Key.RGrip.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new ButtonKey(Key.None) }
		// 		} },
		// 		{ api.Key.StickPush.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Push", new StickStick{ IsLeftElseRight = true, Deadzone = 0.1 } }
		// 		} },
		// 		{ api.Key.StickClick.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new ButtonKey(Key.LStickClick) }
		// 		} },
		// 		{ api.Key.B.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new ButtonKey(Key.Face_East) }
		// 		} },
		// 		{ api.Key.A.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new ButtonKey(Key.Face_South) }
		// 		} },
		// 		{ api.Key.X.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new ButtonKey(Key.Face_West) }
		// 		} },
		// 		{ api.Key.Y.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new ButtonKey(Key.Face_North) }
		// 		} },
		// 		{ api.Key.LPadTouch.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new PadSlideStick{ RelativeSize = 0.4, Deadzone = 0.1, IsLeftElseRight = true } }
		// 		} },
		// 		{ api.Key.LPadClick.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new PadButtonCross(Key.Dpad_Right, Key.Dpad_Up, Key.Dpad_Left, Key.Dpad_Down, 0.5, hasOverlap: true) }
		// 		} },
		// 		{ api.Key.RPadTouch.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new PadStick { Deadzone = 0.1, OuterLimit = 0.7, IsLeftElseRight = false } }
		// 		} },
		// 		{ api.Key.RPadClick.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new ButtonKey(Key.RStickClick) }
		// 		} },
		// 		{ api.Key.Forward.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new ButtonKey(Key.Start) }
		// 		} },
		// 		{ api.Key.Back.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new ButtonKey(Key.Back) }
		// 		} },
		// 		{ api.Key.Steam.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Tap", new ButtonKey(Key.GamepadHome) }
		// 		} },
		// 		{ api.Key.GyroMove.ToString(), new Dictionary<string, Hardware> {
		// 			{ "Push", null }
		// 		} },
		// 	};
		// 	return new Map{ InputMap=map };
		// }

		// public class HardwareConverter : JsonConverter<Hardware> {
		// 	public override bool CanConvert(Type typeToConvert) =>
		// 		typeof(Hardware).IsAssignableFrom(typeToConvert);
			
		// 	public override Hardware Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
		// 		if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException("Start not found.");

		// 		reader.Read();
		// 		if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException("Property name not found.");

		// 		string propertyName = reader.GetString();
		// 		if (propertyName != "HardwareType")
		// 			throw new JsonException("Object doesn't begin with type discriminator (HardwareType).");

		// 		reader.Read();
		// 		if (reader.TokenType != JsonTokenType.String)
		// 			throw new JsonException("Type discriminator (HardwareType) must be a string.");

		// 		string hardwareType = reader.GetString();
				
		// 		return new ButtonKey();
		// 	}

		// 	public override void Write(Utf8JsonWriter writer, Hardware hardware, JsonSerializerOptions options) {
		// 		writer.WriteStartObject();
		// 		writer.WriteString("HardwareType", hardware.HardwareType);


		// 	}
		// }
	}
}