namespace SteamControllerApi {
	public abstract class Data {
		public abstract DataType Type { get; }
	}

	// enum matches value of input type from firmware
	public enum DataType {
		Input = 1,
		Connection = 3,
		State = 4
	}
}