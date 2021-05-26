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
		// The following keys don't exist in the controller's byte codes.
		// They are only used by the API.
		LTriggerPull,
		RTriggerPull,
		StickPush,
		GyroMove,
	}

	[Flags]
	public enum Flags {
		None = 0b0000,
		Pressed = 0b0001,
		Released = 0b0010,
		RelativeMove = 0b0100,
		AbsoluteMove = 0b1000,
	}

	public class InputData : Data {
		public struct Coords {
			public short x, y, z, a;

			public override string ToString() => "x: " + x + " y: " + y + " z: " + z + " a: " + a;

			public static Coords operator +(Coords coord) => coord;

			public static Coords operator +(Coords c1, Coords c2) => new Coords{
				x = (short)(c1.x + c2.x),
				y = (short)(c1.y + c2.y),
				z = (short)(c1.z + c2.z),
				a = (short)(c1.a + c2.a)
			};

			public static Coords operator -(Coords c) {
				c.x = (short)(-c.x);
				c.y = (short)(-c.y);
				c.z = (short)(-c.z);
				c.a = (short)(-c.a);
				return c;
			}

			public static Coords operator -(Coords c1, Coords c2) => c1 + (-c2);

			public static implicit operator (short, short)(Coords coords) => (coords.x, coords.y);
			
			public static implicit operator (short, short, short, short)(Coords coords) =>
				(coords.x, coords.y, coords.z, coords.a);

			public static implicit operator Coords((short x, short y) coords) => new Coords{ x = coords.x, y = coords.y };

			public static implicit operator Coords((short yaw, short pitch, short roll, short a) coords) =>
				new Coords{ x = coords.yaw, y = coords.pitch, z = coords.roll, a = coords.a };

			public void Deconstruct(out short x, out short y) { x = this.x; y = this.y;	}

			public void Deconstruct(out short x, out short y, out short z, out short a) {
				x = this.x;
				y = this.y;
				z = this.z;
				a = this.a;
			}
		}

		public Key Key { get; init; }
		public Flags Flags { get; init; }
		public byte? TriggerPull { get; init; } = null;
		public Coords? Coordinates { get; init; } = null;
		public (short x, short y, short z)? ParallelCoordinates { get; init; } = null;
		public readonly long? TimeHeld = null;

		public override DataType Type => DataType.Input;
		public bool IsTriggerPull => this.Key is Key.LTriggerPull or Key.RTriggerPull;

		public bool IsDuallyCoordinal => this.Key is Key.LPadTouch
			or Key.LPadClick or Key.RPadTouch or Key.RPadClick or Key.StickPush;

		public bool IsButton => ((this.Flags & Flags.Pressed) == Flags.Pressed)
			|| ((this.Flags & Flags.Released) == Flags.Released);

		public bool IsGyroscopic => this.Key == Key.GyroMove;

		public InputData(Key key, Flags flags, long? timeHeld = null) {
			this.Key = key;
			this.Flags = flags;
			this.TimeHeld = timeHeld;
		}

		public InputData(Key key, byte triggerPull, Flags flags, long? timeHeld = null) {
			this.Key = key;
			this.Flags = flags;
			this.TriggerPull = triggerPull;
			this.TimeHeld = timeHeld;
		}

		public InputData(Key key, short x, short y, Flags flags, long? timeHeld = null) {
			this.Key = key;
			this.Flags = flags;
			// this can cause coords to be unpacked then repacked when copying events
			this.Coordinates = (Coords)(x, y);
			this.TimeHeld = timeHeld;
		}
		
		public InputData(
			Key key, Coords coordinates,
			(short, short, short) parallelCoordinates,
			Flags flags,
			long? timeHeld = null
		) {
			this.Key = key;
			this.Coordinates = coordinates;
			this.ParallelCoordinates = parallelCoordinates;
			this.Flags = flags;
			this.TimeHeld = timeHeld;
		}

		public override int GetHashCode() => (int)((uint)Key + (uint)Flags);

		public override string ToString() {
			string str = Key.ToString();
			str += " " + (Flags==Flags.None ? "" : Flags.ToString());

			if (TriggerPull.HasValue) str += " " + TriggerPull;
			if (Coordinates.HasValue) str += " " + Coordinates;
			if (ParallelCoordinates.HasValue) str += " ParallelTo " + ParallelCoordinates;

			if (TimeHeld.HasValue) str += " TimeHeld: " + TimeHeld.Value;

			return str;
		}
	}
}