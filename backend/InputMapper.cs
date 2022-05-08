using System;
using System.Collections.Generic;
using System.IO;


using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using api = SteamControllerApi;
using Robot;


namespace Input {
	public static class InputMapper {
		public class Map {
			public string Name { get; set; } = "";
			public Dictionary<string, Hardware?> InputMap { get; set; } = CreateBlankInputMap();
			public Dictionary<string, Dictionary<string, Hardware?>> ActionMaps = new ();

			// Ensure that no keys are being held by the OS when the program stops.
			~Map() {
				foreach (var entry in InputMap) {
					entry.Value?.ReleaseAll();
				}
				foreach (var mapEntry in ActionMaps) {
					foreach (var entry in mapEntry.Value) {
						entry.Value?.ReleaseAll();
					}
				}
			}
		}

		public static string Directory { get; set; } = @"inputmaps\";

		/// <summary>
		/// Returns true if the map was opened and false if no map was found and a blank map
		/// was created instead.
		/// </summary>
		public static (Map, bool) Open(string mapName) {
			string mapPath = Directory + mapName + ".json";
			string jsonString;

			// create the directory to store the input maps in if it doesn't already exist
			var mapFileInfo = new FileInfo(mapPath);
			mapFileInfo.Directory?.Create();

			if (!File.Exists(mapPath)) {
				var blankInputMap = new Map{ Name = mapName };
				jsonString = JsonConvert.SerializeObject(blankInputMap, Formatting.Indented);
				File.WriteAllText(mapPath, jsonString);

				return (blankInputMap, false);
			}
			jsonString = File.ReadAllText(mapPath);

			JsonConvert.DefaultSettings = () => new JsonSerializerSettings{
				TypeNameHandling = TypeNameHandling.Auto,
				MissingMemberHandling = MissingMemberHandling.Error
			};
			var map = JsonConvert.DeserializeObject<Map>(
				jsonString,
				new StringEnumConverter()
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
			var file = new FileInfo(Directory + mapName + ".json");

			file.Directory?.Create();
			File.WriteAllText(file.FullName, jsonString);
		}

		public static void Print(this Map map) {
			Action<Dictionary<string, Hardware?>> PrintInputMap = inputs => {
				foreach (var entry in inputs) {
					Console.WriteLine($"{entry.Key}: {entry.Value}");
				}
			};

			Console.WriteLine(map.Name + ":");
			PrintInputMap(map.InputMap);
			Console.WriteLine();
			foreach (var entry in map.ActionMaps) {
				Console.WriteLine($"ActionMap {entry.Key}:");
				PrintInputMap(entry.Value);
				Console.WriteLine();
			}			
		}

		private static Dictionary<string, Hardware?> CreateBlankInputMap() {
			var inputMap = new Dictionary<string, Hardware?>();
			// keys are stored as strings so that they can be indexed in alphabetical order
			var keyStrings = new List<string>(20);

			foreach (api.KeyInternal k in Enum.GetValues(typeof(api.KeyInternal))) {
				if (k is api.KeyInternal.DPadLeft
					or api.KeyInternal.DPadUp
					or api.KeyInternal.DPadRight
					or api.KeyInternal.DPadDown
				) { 
					continue; 
				} else keyStrings.Add(k.ToString());
			}

			keyStrings.Sort();
			foreach (string s in keyStrings) inputMap.Add(s, null);

			return inputMap;
		}

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