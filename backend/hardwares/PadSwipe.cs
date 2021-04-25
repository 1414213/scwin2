using System;
using System.Collections.Generic;
using System.Diagnostics;

using Newtonsoft.Json;

using api = SteamControllerApi;
using Robot;


namespace Backend {
	public class PadSwipe : Trackpad {
		public double MinimunDistance {
			get => this.minimumDistance;
			set {
				if (value < 0 || value > 1d) throw new SettingNotProportionException(
					"MinimumDistance must be a proportion of the trackpad's diameter (range [0, 1])."
				);
				this.minimumDistance = value;
			}
		}
		public double AngleOffset {
			get => this.angleOffset;
			set => this.angleOffset = value % 2;
		}
		public double LongSwipeThreshold {
			get => this.longSwipeThreshold;
			set {
				if (value < 0) throw new SettingInvalidException("LongSwipeThreshold must be a multiple > 0");
				this.longSwipeThreshold = value;
			}
		}
		public IList<Button> Buttons { get; set; } = new List<Button>();
		public IList<Button?> LongSwipeButtons { get; set; } = new List<Button?>();
		public bool IsContinuous { get; set; } = false;
		public double MinimumSpeed { get; set; } = 80;
		[JsonIgnore]
		public int Amount => this.Buttons.Count;

		private double minimumDistance = 0.25;
		// defaults to an impossible input value (1.5 times the diameter of the trackpad)
		// so if the property is never assigned all swipes will be treated as regular swipes
		private double longSwipeThreshold = 1.5;
		private double angleOffset = 0;
		private (short x, short y) startingPosition;
		private bool isInitialPress;

		// fields for continuous setting
		private Stopwatch stopwatch = new Stopwatch();
		private (short x, short y) previous;

		public PadSwipe() {}

		public PadSwipe(double angleOffset, double minimumDistance) : this() {
			this.AngleOffset = angleOffset;
			this.MinimunDistance = minimumDistance;
			this.LongSwipeThreshold = longSwipeThreshold;
		}

		public PadSwipe(double angleOffset, 
		                double minimumDistance, double longSwipeThreshold) : this(angleOffset, minimumDistance) {
			this.LongSwipeThreshold = longSwipeThreshold;
		}

		public PadSwipe(double angleOffset,
		                double minimumDistance, params Key[] keys): this(angleOffset, minimumDistance) {
			this.Buttons = new List<Button>();
			foreach (Key k in keys) this.Buttons.Add(new ButtonKey(k));
		}

		public PadSwipe(double angleOffset, double minimumDistance, double longSwipeThreshold,
		                Key[] keys, Key[] longKeys) : this(angleOffset, minimumDistance, longSwipeThreshold) {
			this.Buttons = new List<Button>(keys.Length);
			this.LongSwipeButtons = new List<Button?>(keys.Length);
			for (int i = 0; i < keys.Length; i++) {
				this.Buttons.Add(new ButtonKey(keys[i]));
				this.LongSwipeButtons.Add(new ButtonKey(longKeys[i]));
			}
		}

		protected override void DoEventImpl(api.InputData e) {
			(short x, short y) coord = e.Coordinates ?? throw new ArgumentException(e + " must be coordinal.");
			if (!e.IsButton) throw new ArgumentException(e + " isn't a button press.");

			// saves the position of the initial press event
			if (isInitialPress) {
				startingPosition = coord;
				previous = coord;
				isInitialPress = false;
				stopwatch.Restart();
				return;
			} else if ((e.Flags & api.Flags.Released) == api.Flags.Released) {
				stopwatch.Stop();
				isInitialPress = true;
				this.DoTap(coord);
				return;
			} else if (IsContinuous) {
				stopwatch.Stop();
				long elapsedTime = stopwatch.ElapsedMilliseconds;
				stopwatch.Restart();
				//Console.Write($"time {elapsedTime} ");

				var speed = (x: (coord.x - previous.x) / (double)elapsedTime,
				             y: (coord.y - previous.y) / (double)elapsedTime);
				double speedMagnitude = Math.Sqrt(speed.x * speed.x + speed.y * speed.y);

				if (speedMagnitude < MinimumSpeed) {
					var wasTapped = this.DoTap(coord);
					//Console.WriteLine($"speed: {speed} tap!");
					startingPosition = coord;
				} //else Console.WriteLine($"speed: {speed} do nothing");
				previous = coord;
			}
		}

		protected override void ReleaseAllImpl() {
			foreach (var b in Buttons) b.ReleaseAll();
			foreach (var b in LongSwipeButtons) b?.ReleaseAll();
		}

		private bool DoTap((short x, short y) releaseCoord) {
			// compute drawn vector with angle of radians (range [0, 2))
			(long x, long y) delta = (releaseCoord.x - startingPosition.x,
			                          releaseCoord.y - startingPosition.y);
			double r = Math.Sqrt((delta.x * delta.x) + (delta.y * delta.y));
			double theta = 0;

			if (delta.y >= 0 && r != 0) theta = Math.Acos(delta.x / r);
			else if (delta.y < 0) theta = -Math.Acos(delta.x / r);
			else if (r == 0) theta = Double.NaN;
			theta = theta / Math.PI;
			if (theta < 0) theta += 2;

			if (!Double.IsNaN(theta) && r > minimumDistance * (-Int16.MinValue + Int16.MaxValue)) {
				// adjust angle to measure starting from the offset
				theta = (theta - angleOffset) % 2;
				if (theta < 0) theta += 2;

				// compute size of each section
				var sliceSize = 2d / Amount;
				if (Double.IsNaN(sliceSize)) return false;
				var indexOfButtonToTap = (int)(theta / sliceSize);
				var isLongSwipe = r > longSwipeThreshold * (-Int16.MinValue + Int16.MaxValue);

				// Tap button corresponding to the direction of the swipe.
				// Check if there exists buttons within long swipe list.
				if (isLongSwipe && (indexOfButtonToTap < LongSwipeButtons.Count)) {
					if (LongSwipeButtons[indexOfButtonToTap] is Button b) b.Tap();
					else Buttons[indexOfButtonToTap].Tap();
					return true;
				}
				Buttons[indexOfButtonToTap].Tap();
				return true;
			} else return false;
		}
	}
}