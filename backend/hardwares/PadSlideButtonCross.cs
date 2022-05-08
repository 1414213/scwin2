using System;
using Newtonsoft.Json;
using api = SteamControllerApi;
using Robot;

namespace Input {
	public class PadSlideButtonCross : Trackpad {
		// Wrap a StickButtonCross object.
		public Button East         { get => buttonCross.East;        set => buttonCross.East = value; }
		public Button North        { get => buttonCross.North;       set => buttonCross.North = value; }
		public Button West         { get => buttonCross.West;        set => buttonCross.West = value; }
		public Button South        { get => buttonCross.South;       set => buttonCross.South = value; }
		public Button Inner        { get => buttonCross.Inner;       set => buttonCross.Inner = value; }
		public Button Outer        { get => buttonCross.Outer;       set => buttonCross.Outer = value; }
		public bool HasOverlap     { get => buttonCross.HasOverlap;  set => buttonCross.HasOverlap = value; }
		public double OverlapIgnoranceRadius {
			get => buttonCross.OverlapIgnoranceRadius;
			set => buttonCross.OverlapIgnoranceRadius = value;
		}
		public double InnerRadius  { get => buttonCross.InnerRadius; set => buttonCross.InnerRadius = value; }
		public double OuterRadius  { get => buttonCross.OuterRadius; set => buttonCross.OuterRadius = value; }
		public double Deadzone     { get => buttonCross.Deadzone;    set => buttonCross.Deadzone = value; }

		private StickButtonCross buttonCross = new StickButtonCross();

		public double RelativeSize {
			get => this.relativeSize;
			set {
				if (value < 0 || value > 1.0)
					throw new SettingNotProportionException("RelativeSize must be a proportion of 0 to 1.");
				this.relativeSize = value;
			}
		}
		public bool Anchored { get; set; }

		[JsonIgnore]
		public short X => (short)Math.Clamp(this.RealX, Int16.MinValue, Int16.MaxValue);
		[JsonIgnore]
		public short Y => (short)Math.Clamp(this.RealY, Int16.MinValue, Int16.MaxValue);
		[JsonIgnore]
		public long RealX => (long)(this.position.x * (1 / this.relativeSize));
		[JsonIgnore]
		public long RealY => (long)(this.position.y * (1 / this.relativeSize));

		private double relativeSize = 0.5;
		private (long x, long y) position;
		private (short x, short y)? previousCoord;

		public PadSlideButtonCross() {}

		public PadSlideButtonCross(Key east, Key north, Key west, Key south, bool hasOverlap = false) : this() {
			this.East = new ButtonKey(east);
			this.North = new ButtonKey(north);
			this.West = new ButtonKey(west);
			this.South = new ButtonKey(south);
			this.HasOverlap = hasOverlap;
		}

		protected override void DoEventImpl(api.ITrackpadData input) {
			(short x, short y) coord = input.Position;
			
			// if e is an initial press, no movement has occured
			if (!previousCoord.HasValue) {
				previousCoord = coord;
				return;
			}

			// else move the respective thumbstick by a relative amount
			position.x += (coord.x - previousCoord.Value.x);
			position.y += (coord.y - previousCoord.Value.y);
			previousCoord = coord;

			// convert to polar coordinates.
			double r = Math.Sqrt((position.x * position.x) + (position.y * position.y));
			double theta = 0;
			if (position.y >= 0 && r != 0) theta = Math.Acos(position.x / r);
			else if (position.y < 0) theta = -Math.Acos(position.x / r);
			else if (r == 0) theta = Double.NaN;

			// reduce the length of the simulated stick push (r) to be <= maximum short value
			if (r > Int16.MaxValue * relativeSize) r = Int16.MaxValue * relativeSize;

			// convert back to cartesian coordinates and store the pad's current position
			var roundCoord = (x: (short)Math.Clamp(r * Math.Cos(theta), Int16.MinValue, Int16.MaxValue),
			                  y: (short)Math.Clamp(r * Math.Sin(theta), Int16.MinValue, Int16.MaxValue));
			
			if (!Anchored) {
				// If the stick isn't anchored, then the reduced polar coordinates are assigned to
				// its stored position so the center point of the simulated thumbstick slides as the
				// user travels outside its range.
				position = roundCoord;
			}
			
			// simulate stick input and scale the simulated thumbstick's current position
			// NOTE: if still buggy try clamping before converting
			double movementMultiple = 1 / relativeSize;
			roundCoord.x = Convert.ToInt16(roundCoord.x * movementMultiple);
			roundCoord.y = Convert.ToInt16(roundCoord.y * movementMultiple);

			buttonCross.DoEvent(new api.StickData(roundCoord, api.Flags.None));

			// if e is being released reset the respective thumbstick
			if (input.IsRelease) {
				previousCoord = null;
				position = (0, 0);
				buttonCross.DoEvent(new api.StickData((0, 0), api.Flags.None));
			}
		}

		protected override void ReleaseAllImpl() {
			East.ReleaseAll();
			North.ReleaseAll();
			West.ReleaseAll();
			South.ReleaseAll();
			Inner.ReleaseAll();
			Outer.ReleaseAll();
		}

		public override void Unfreeze(api.IInputData newInput) => this.DoEvent(newInput);
	}
}