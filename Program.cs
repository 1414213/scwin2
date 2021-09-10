using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Device.Net;
using Hid.Net.Windows;
using api = SteamControllerApi;

class Program {
	public static async Task Main(string[] args) {
		// Parse given arguments:
		bool noGamepad = false;
		int debugType = 0, debugGui = 0;
		string? inputMapName = null;
		for (int i = 0; i < args.Length; i++) {
			Action<Action<string>, string> DoFlag = (action, errorMessage) => {
				i++;
				if (i >= args.Length) {
					Console.WriteLine("ERROR: " + errorMessage);
					Environment.Exit(1);
				}
				action(args[i]);
			};

			switch (args[i]) {
				case "-n":
				case "-no-gamepad":
				case "--no-gamepad":
					noGamepad = true;
					break;
				case "-d":
				case "-directory":
				case "--directory": {
					DoFlag(directory => {
						if (!(directory.EndsWith("\\") || directory.EndsWith("/"))) directory += "/";
						Backend.InputMapper.Directory = directory;
					}, "No directory given.");
					break;
				}
				case "--debug": {
					DoFlag(level => {
						try { debugType = Int32.Parse(level); }
						catch {
							Console.WriteLine("ERROR: debug level isn't a number");
							Environment.Exit(1);
							return;
						}
						if (debugType < 0) debugType = 0;
					}, "No debug level given.");
					break;
				}
				case "--debug-gui": {
					DoFlag(level => {
						try { debugGui = Math.Clamp(Int32.Parse(level), 0, 3); }
						catch {
							Console.WriteLine("ERROR: debug gui level isn't a number");
							Environment.Exit(1);
							return;
						}
					}, "No debug level given.");
					break;
				}
				default:
					if (args[i][0] == '-') {
						Console.WriteLine($"ERROR: Command {args[i]} not known.");
						Environment.Exit(1);
					}
					if (inputMapName == null) {
						inputMapName = args[i];
					} else {
						Console.WriteLine(
							$"ERROR: Inputmap {args[i]} cannot be opened because {inputMapName} was already given.");
					}
					break;
			}
		}
		if (inputMapName == null) {
			Console.WriteLine("ERROR: no input map given.");
			return;
		}

		// Connect to controller via HID driver.
		var logger = new DebugLogger();
		var tracer = new DebugTracer();
		var steamcon = new api.Controller();

		WindowsHidDeviceFactory.Register(logger, tracer);

		var deviceDefinitions = new List<FilterDeviceDefinition>{
			new FilterDeviceDefinition{ 
				DeviceType = DeviceType.Hid, 
				VendorId = steamcon.VendorId,
				ProductId = steamcon.ProductId,
				Label = "Steam Controller",
			},
		};
		List<IDevice> devices = await DeviceManager.Current.GetDevicesAsync(deviceDefinitions);
		
		if (devices == null || devices.Count == 0) {
			throw new System.InvalidOperationException("device couldn't be found");
		}
		var device = devices[0];
		await device.InitializeAsync();

		// Prepare to handle input events from steam controller.
		var (map, isNotBlank) = Backend.InputMapper.Open(inputMapName);
		if (!isNotBlank) {
			Console.WriteLine($"New input map {map.Name} created!");
			return;
		}
		var doer = new Backend.EventDoer(map, steamcon, !noGamepad){ Debug = debugType };

		// Haptic tests:
		// var haptic = new byte[64] {
		// 	0x8F,
		// 	0x07,
		// 	0x00, // Left (1) or right (0) trackpad.
		// 	0xFF, // LSB pulse high duration
		// 	0xFF, // MSB pulse high duration
		// 	0xFF, // LSB pulse low duration
		// 	0xFF, // MSB pulse low duration
		// 	0xFF, // LSB pulse repeat count
		// 	0x04, // MSB pulse repeat count
		// 	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		// 	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		// 	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		// 	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		// 	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		// 	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		// 	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
		// };
		// var factor = 495483.0;

		// haptic[3] = 128;
		// haptic[4] = 20;
		// haptic[5] = 128;
		// haptic[6] = 20;

		// await device.WriteAsync(haptic);
		// await device.WriteAsync(haptic);
		// return;

		// App loop:
		ReadResult input;
		IList<api.IInputData> events;
		while (true) {
			input = await device.ReadAsync();

			if (debugType is 1) PrintData(input.Data);

			if (input.Data[2] == 0x01) {
				events = steamcon.GenerateEvents(input);
				if (debugType is 2) foreach (var e in events) Console.WriteLine(e);
				if (debugType is 3) foreach (var e in events) if (e is not api.IMotionData) Console.WriteLine(e);
				doer.DoEvents(events);
			}
		}
	}

	private static void PrintData(byte[] data) {
		var sb = new StringBuilder();

		for (int i = 1; i <= data.Length; i++) {
			sb.AppendFormat("{0,2:X2}", data[i-1]);
			if (i % 8 == 0) sb.Append("  ");
			else if (i <= data.Length - 1) sb.Append(" ");
		}
		Console.WriteLine(sb.ToString());
	}

	private static async Task<int> MeasureInputRateAsync(IDevice device, bool onlyKeys = true) {
		var sw = new System.Diagnostics.Stopwatch();
		int count = 0;
		sw.Start();
		while (sw.ElapsedMilliseconds < 1000) {
			ReadResult input = await device.ReadAsync();
			if (onlyKeys) if (input.Data[2] == 1) count++;
		}
		return count;
	}
}