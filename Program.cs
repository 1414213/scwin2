using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using Device.Net;
using Hid.Net.Windows;

using api = SteamControllerApi;


class Program {
	public static async Task Main(string[] args) {
		// parse given arguments
		bool noGamepad = false;
		string? inputMapName = null;
		for (int i = 0; i < args.Length; i++) {
			switch (args[i]) {
				case "-n":
				case "-no-gamepad":
				case "--no-gamepad":
					noGamepad = true;
					break;
				default:
					if (inputMapName == null) inputMapName = args[i];
					break;
			}
		}
		if (inputMapName == null) {
			Console.WriteLine("ERROR: no input map given.");
			return;
		}

		DebugLogger logger = new DebugLogger();
		DebugTracer tracer = new DebugTracer();
		api.Controller steamcon = new api.Controller();

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
		
		// read inputs from the HID device
		ReadResult input;
		IList<api.InputData> events;

		// prepare to handle input events from steam controller
		var (map, isNotBlank) = Backend.InputMapper.Open(inputMapName);
		if (!isNotBlank) {
			Console.WriteLine($"New input map {map} created!");
			return;
		}
		Backend.EventDoer doer = new Backend.EventDoer(map, createVirtualGamepad: !noGamepad);

		// app loop
		while (true) {
			input = await device.ReadAsync();
			//PrintData(input.Data);
			//Console.WriteLine(BitConverter.ToString(input.Data));

			if (input.Data[2] == 0x01) {
				events = steamcon.GenerateEvents(input);
				//foreach (var e in events) Console.WriteLine(e);
				//foreach (var e in events) if (e.Key != api.Key.GyroMove) Console.WriteLine(e);
				foreach (var e in events) if (e.Key == api.Key.GyroMove) Console.WriteLine(e);
				doer.DoEvents(events);
			}
		}
	}

	private static void PrintData(byte[] data) {
		StringBuilder sb = new StringBuilder();

		for (int i = 1; i <= data.Length; i++) {
			sb.AppendFormat("{0,2:X2}", data[i-1]);
			if (i % 8 == 0) sb.Append("  ");
			else if (i <= data.Length - 1) sb.Append(" ");
		}
		Console.WriteLine(sb.ToString());
	}

	private static async Task<int> MeasureInputRate(IDevice device, bool onlyKeys = true) {
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