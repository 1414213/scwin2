using System;

namespace SteamControllerApi {
	public enum Key : uint {
		RGrip = 0x00_00_01,
		LPadClick = 0x00_00_02,
		RPadClick = 0x00_00_04,
		LPadTouch = 0x00_00_08,
		RPadTouch = 0x00_00_10,
		StickClick = 0x00_00_42,
		// DPad...'s represent the cardinal orientation of the position of the user's thumb
		// on the left trackpad when it is clicked.  I labeled these as being
		// part of a "dpad" because that what it seems the firmware is trying to emulate by doing
		// this.  Maybe these were the keycodes for the dpad buttons on the prototypes that had one,
		// idk.  Pretty much useless.
		DPadUp = 0x00_01_00,
		DPadRight = 0x00_02_00,
		DPadLeft = 0x00_04_00,
		DPadDown = 0x00_08_00,
		Back = 0x00_10_00,
		Steam = 0x00_20_00,
		Forward = 0x00_40_00,
		LGrip = 0x00_80_00,
		RTriggerClick = 0x01_00_00,
		LTriggerClick = 0x02_00_00,
		RBumper = 0x04_00_00,
		LBumper = 0x08_00_00,
		Y = 0x10_00_00,
		B = 0x20_00_00,
		X = 0x40_00_00,
		A = 0x80_00_00,
	}

	public enum KeyInternal : uint {
		RGrip = 0x00_00_01,
		LPadClick = 0x00_00_02,
		RPadClick = 0x00_00_04,
		LPadTouch = 0x00_00_08,
		RPadTouch = 0x00_00_10,
		StickClick = 0x00_00_42,
		// DPad...'s represent the cardinal orientation of the position of the user's thumb
		// on the left trackpad when it is clicked.  I labeled these as being
		// part of a "dpad" because that what it seems the firmware is trying to emulate by doing
		// this.  Maybe these were the keycodes for the dpad buttons on the prototypes that had one,
		// idk.  Pretty much useless.
		DPadUp = 0x00_01_00,
		DPadRight = 0x00_02_00,
		DPadLeft = 0x00_04_00,
		DPadDown = 0x00_08_00,
		Back = 0x00_10_00,
		Steam = 0x00_20_00,
		Forward = 0x00_40_00,
		LGrip = 0x00_80_00,
		RTriggerClick = 0x01_00_00,
		LTriggerClick = 0x02_00_00,
		RBumper = 0x04_00_00,
		LBumper = 0x08_00_00,
		Y = 0x10_00_00,
		B = 0x20_00_00,
		X = 0x40_00_00,
		A = 0x80_00_00,
		// The following keys don't exist in the controller's byte codes.
		// They are only used by the API.
		LTrigger,
		RTrigger,
		Stick,
		Motion,
	}

	public static class Extensions {
		public static KeyInternal ToInternal(this Key key) => (KeyInternal)((int)key);

		public static bool IsButton(this KeyInternal key) => key switch {
			KeyInternal.LTrigger   => false,
			KeyInternal.RTrigger   => false,
			KeyInternal.Stick      => false,
			KeyInternal.Motion     => false,
			_                      => true
		};
	}

	[Flags]
	public enum Flags {
		None = 0b0000,
		Pressed = 0b0001,
		Released = 0b0010,
		RelativeMove = 0b0100,
	}

	public interface IInputData {
		Flags Flags { get; init; }
		/// <summary> Returns a string identifying what input this is (A, LTrigger, et cetera). </summary>
		string Identity { get; }
		bool IsPress => (Flags & Flags.Pressed) == Flags.Pressed;
		bool IsRelease => (Flags & Flags.Released) == Flags.Released;
		bool IsRelativeMovement => (Flags & Flags.RelativeMove) == Flags.RelativeMove;
	}

	public interface IButtonData : IInputData {
		Key Key { get; init; }
		long? TimeHeld { get; init; }
	}

	public interface ITriggerData : IInputData { byte Trigger { get; init; } }

	public interface IPositional : IInputData { (short x, short y) Position { get; init; } }

	public interface ITrackpadData : IButtonData, IPositional {}

	public interface IMotionData : IInputData {
		(short roll, short pitch, short yaw, short w) Gyroscope { get; init; }
		(short x, short y, short z) Accelerometer { get; init; }
	}

	public record ButtonData(Key Key, Flags Flags, long? TimeHeld = null) : IButtonData {
		public string Identity => Key.ToString();
	}

	public record TriggerData(byte Trigger, bool IsLeftElseRight, Flags Flags) : ITriggerData {
		public string Identity => IsLeftElseRight ? KeyInternal.LTrigger.ToString() : KeyInternal.RTrigger.ToString();
	}

	public record StickData((short x, short y) Position, Flags Flags) : IPositional {
		public string Identity => KeyInternal.Stick.ToString();
	}

	public record TrackpadData(
		Key Key,
		(short x, short y) Position,
		Flags Flags,
		long? TimeHeld = null
	) : ITrackpadData {
		public string Identity => Key switch {
			Key.LPadTouch => Key.ToString(),
			Key.LPadClick => Key.ToString(),
			Key.RPadTouch => Key.ToString(),
			Key.RPadClick => Key.ToString(),
			_ => throw new ArgumentException("TrackpadData doesn't contain a trackpad key.")
		};
	}

	public record MotionData(
		(short roll, short pitch, short yaw, short w) Gyroscope,
		(short x, short y, short z) Accelerometer,
		Flags Flags
	) : IMotionData {
		public string Identity => KeyInternal.Motion.ToString();
	}

	// public record InputData : IData, IInputData, IButtonData, ITriggerData, IPositional, ITrackpadData, IMotionData {
	// 	public enum Type { Button, Trigger, Stick, Trackpad, TrackpadClick, Motion, }

	// 	public Key Key { get; init; }
	// 	public Flags Flags { get; init; }
	// 	public byte Trigger { get; init; }
	// 	public (short x, short y) Position { get; init; }
	// 	public (short yaw, short pitch, short roll, short w) Gyroscope { get; init; }
	// 	public (short x, short y, short z) Accelerometer { get; init; }
	// 	public long? TimeHeld { get; init; }

	// 	public string Identity => InputType switch {
	// 		Type.Button => Key.ToString(),
	// 		Type.Trigger => this.IsLeft ? KeyInternal.LTrigger.ToString() : KeyInternal.RTrigger.ToString(),
	// 		Type.Stick => KeyInternal.Stick.ToString(),
	// 		Type.Trackpad => this.IsLeft ? KeyInternal.LPadTouch.ToString() : KeyInternal.RPadTouch.ToString(),
	// 	};
	// 	public Type InputType { get; init; }
	// 	public bool IsPosition => this.InputType is Type.Stick or Type.Trackpad;

	// 	// any
	// 	public InputData() {}

	// 	private InputData(Type type, Flags flags, long? timeHeld) {
	// 		this.InputType = type;
	// 		this.Flags = flags;
	// 		this.TimeHeld = timeHeld;
	// 	}

	// 	// button
	// 	public InputData(Key key, Flags flags, long? timeHeld = null) : this(Type.Button, flags, timeHeld) {
	// 		this.Key = key;
	// 	}

	// 	// trigger
	// 	public InputData(Type type, byte trigger, Flags flags, long? timeHeld = null) : this(type, flags, timeHeld) {
	// 		this.Trigger = trigger;
	// 	}

	// 	// stick
	// 	public InputData((short x, short y) position, Flags flags, long? timeHeld = null)
	// 	: this(Type.Stick, flags, timeHeld) {
	// 		this.Position = position;
	// 	}

	// 	// motion
	// 	public InputData((ushort yaw, ushort pitch, ushort roll, short w) gyroscope,
	// 	                 (short x, short y, short z) accelerometer,
	// 	                 Flags flags,
	// 	                 long? timeHeld = null) : this(Type.Motion, flags, timeHeld) {
	// 		this.Gyroscope = gyroscope;
	// 		this.Accelerometer = accelerometer;
	// 	}

	// 	public string ToStringAbridged() {
	// 		var str = Key.ToString();
	// 		str += " " + (Flags == Flags.None ? "" : Flags.ToString());
	// 		str += " " + this.InputType switch {
	// 			Type.Trigger  => Trigger,
	// 			Type.Stick    => Position,
	// 			Type.Trackpad => Position,
	// 			Type.Motion   => Gyroscope + " " + Accelerometer,
	// 			Type.Button   => "",
	// 		};
	// 		if (TimeHeld.HasValue) str += " TimeHeld: " + TimeHeld.Value;
	// 		return str;
	// 	}
}