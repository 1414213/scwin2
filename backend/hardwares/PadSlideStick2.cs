using System;

using api = SteamControllerApi;


namespace Backend {
	class PadSlideStick : Trackpad {
		public double RelativeSize {
			get => this.relativeSize;
			set {
				if (value < 0 || value > 1.0) throw new SettingNotProportionException(
					"RelativeSize must be a proportion of the diameter [0, 1]."
				);
				this.relativeSize = value;
			}
		}
		public double Deadzone {
			get => this.deadzone;
			set {
				if (value < 0 || value > 1.0)
					throw new SettingNotProportionException("RelativeSize must be a proportion of the radius [0, 1].");
				this.deadzone = value;
			}
		}
		public bool IsLeftElseRight { get; set; }
		public bool Anchored { get; set; }

		// // Gets the position of the simulated thumbstick.
		// [JsonIgnore]
		// public short X => (short)Math.Clamp(this.RealX, Int16.MinValue, Int16.MaxValue);
		// [JsonIgnore]
		// public short Y => (short)Math.Clamp(this.RealY, Int16.MinValue, Int16.MaxValue);

		// // Gets the actual stored position of the thumbstick before clamping to the 
		// // maximum push distance of a thumbstick.
		// [JsonIgnore]
		// public int RealX => (int)(this.position.x * (1 / this.relativeSize));
		// [JsonIgnore]
		// public long RealY => (int)(this.position.y * (1 / this.relativeSize));

		private double relativeSize = 0.5;
		private double deadzone = 0.1;
		private double outerLimit = 0.9;
		private (long x, long y) position;
		private (short x, short y) previousCoord;
		private bool isInitialPress = true;

		protected override void DoEventImpl(api.ITrackpadData input) {
			(short x, short y) coord = input.Position;
			
			// if e is an initial press, no movement has occured
			if (isInitialPress) {
				previousCoord = coord;
				isInitialPress = false;
				return;
			}
			
			// else move the thumbstick by a relative amount
			position.x += coord.x - previousCoord.x;
			position.y += coord.y - previousCoord.y;
			previousCoord = coord;

			// convert to polar coordinates
			// position must be casted to prevent an overflow
			var psquared = (x: (ulong)(position.x * position.x), y: (ulong)(position.y * position.y));
			double r = Math.Sqrt(psquared.x + psquared.y);
			if (r < deadzone * Int16.MaxValue) {
				if (IsLeftElseRight) robot.MoveLStick(0, 0);
				else robot.MoveRStick(0, 0);
				return;
			}
			double theta = 0;
			if (position.y >= 0 && r != 0) theta = Math.Acos(position.x / r);
			else if (position.y < 0) theta = -Math.Acos(position.x / r);
			else if (r == 0) theta = Double.NaN;

			// if the respective thumbstick is now within the deadzone, reset it
			if (Double.IsNaN(theta)) {
				// if r is 0, then stick was reset to position 0,0.  No input is determinable.
				if (IsLeftElseRight) robot.MoveLStick(0, 0);
				else robot.MoveRStick(0, 0);
				return;
			}

			// reduce the length of the simulated stick push (r) to be <= maximum short value
			// Any distance overshooting the range of the stick (short int) is treated as a
			// maximum push
			if (r > Int16.MaxValue * relativeSize) r = Int16.MaxValue * relativeSize;

			// convert back to cartesian coordinates and store the pad's current position
			if (!Anchored) {
				// If the stick isn't anchored, then the reduced polar coordinates are assigned to
				// its stored position so the center point of the simulated thumbstick slides as the
				// user travels outside its range.
				// Conversion from polar to cartesian isn't entirely accurate and will sometimes produce a
				// value slightly above or below a int16's range so the value saved as a larger format and
				// then clamped to a short's range.
				position.x = (short)Math.Clamp(r * Math.Cos(theta), Int16.MinValue, Int16.MaxValue);
				position.y = (short)Math.Clamp(r * Math.Sin(theta), Int16.MinValue, Int16.MaxValue);

				// normalize r to an equivalent value between 0 and the outer limit
				if (r >= outerLimit * Int16.MaxValue * relativeSize) r = Int16.MaxValue * relativeSize;
				else r = r / outerLimit;
			}

			// coordinate is a fraction of the pad's range, so after conversion it is multiplied by a multiple
			// which is relative to the relativeSize to increase it into the range of a short
			// Reconvert into cartesian, clamping as before, since r is mutated if we aren't anchored.
			double movementMultiple = 1 / relativeSize;
			(short x, short y) simulated = (
				(short)Math.Clamp(r * Math.Cos(theta) * movementMultiple, Int16.MinValue, Int16.MaxValue),
				(short)Math.Clamp(r * Math.Sin(theta) * movementMultiple, Int16.MinValue, Int16.MaxValue)
			);
			if (IsLeftElseRight) robot.MoveLStick(simulated.x, simulated.y);
			else robot.MoveRStick(simulated.x, simulated.y);

			Console.WriteLine();

			// if e is being released reset the respective thumbstick
			if (input.IsRelease) {
				isInitialPress = true;
				position = (0, 0);
				if (IsLeftElseRight) robot.MoveLStick(0, 0);
				else robot.MoveRStick(0, 0);
			}
		}

		protected override void ReleaseAllImpl() {}
	}
}