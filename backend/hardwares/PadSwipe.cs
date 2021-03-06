using System;
using System.Collections.Generic;
using System.Diagnostics;

using Newtonsoft.Json;

using api = SteamControllerApi;
using Robot;


namespace Input {
	public class PadSwipe : Trackpad {
		public double MinimunDistance { get => minimumDistance;
			set {
				if (value < 0 || value > 1d) throw new SettingNotProportionException(
					"MinimumDistance must be a proportion of the trackpad's diameter (range [0, 1]).");
				this.minimumDistance = value;
			}
		}
		public double AngleOffset {
			get => this.angleOffset;
			set => this.angleOffset = value % 2;
		}
		public double LongSwipeThreshold { get => longSwipeThreshold;
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
		// Defaults to an impossible input value (1.5 times the diameter of the trackpad)
		// so if the property is never assigned then all swipes will be treated as regular swipes.
		private double longSwipeThreshold = 1.5;
		private double angleOffset = 0;
		private (short x, short y) startingPosition;
		private bool isInitialPress;

		// Fields for continuous settings:
		private Stopwatch stopwatch = new Stopwatch();
		private (short x, short y) previous;

		public PadSwipe() {}

		public PadSwipe(double angleOffset, double minimumDistance) : this() {
			this.AngleOffset = angleOffset;
			this.MinimunDistance = minimumDistance;
			this.LongSwipeThreshold = longSwipeThreshold;
		}

		public PadSwipe(double angleOffset, double minimumDistance,	double longSwipeThreshold) : this(
			angleOffset,
			minimumDistance
		) {
			this.LongSwipeThreshold = longSwipeThreshold;
		}

		public PadSwipe(
			double angleOffset,
			double minimumDistance,
			params Key[] keys
		) : this(angleOffset, minimumDistance) {
			this.Buttons = new List<Button>(keys.Length);
			foreach (Key k in keys) this.Buttons.Add(new ButtonKey(k));
		}

		public PadSwipe(
			double angleOffset, double minimumDistance, double longSwipeThreshold,
			Key[] keys, Key[] longKeys
		) : this(angleOffset, minimumDistance, longSwipeThreshold) {
			this.Buttons = new List<Button>(keys.Length);
			this.LongSwipeButtons = new List<Button?>(keys.Length);
			for (int i = 0; i < keys.Length; i++) {
				this.Buttons.Add(new ButtonKey(keys[i]));
				this.LongSwipeButtons.Add(new ButtonKey(longKeys[i]));
			}
		}

		protected override void DoEventImpl(api.ITrackpadData input) {
			(short x, short y) coord = input.Position;

			// saves the position of the initial press event
			if (isInitialPress) {
				startingPosition = coord;
				previous = coord;
				isInitialPress = false;
				stopwatch.Restart();
				return;
			} else if (input.IsRelease) {
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

		public override void Unfreeze(api.IInputData newInput) {
			// Receive newInput as a new beginning for a swipe if input is being send to the trackpad 
			// when the object is unfrozen.
			isInitialPress = true;
			this.DoEvent(newInput);
		}

		private bool DoTap((short x, short y) releaseCoord) {
			// Compute drawn vector with angle of radians (range [0, 2)).
			var (r, theta) = base.CartesianToPolar(
				releaseCoord.x - startingPosition.x, releaseCoord.y - startingPosition.y);
			theta = theta / Math.PI;
			if (theta < 0) theta += 2;

			if (!Double.IsNaN(theta) && r > minimumDistance * (-Int16.MinValue + Int16.MaxValue)) {
				// Adjust angle to measure starting from the offset.
				theta = (theta - angleOffset) % 2;
				if (theta < 0) theta += 2;

				// Compute size of each section.
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
			} else {
				return false;
			}
		}
	}
}