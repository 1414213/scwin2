using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using api = SteamControllerApi;

namespace Backend {
	public class PadTrackball : Trackpad {
		public bool HasInertia { get; set; } = true;
		public double Sensitivity {
			get => (-Int16.MinValue + Int16.MaxValue) * sensitivity;
			set => sensitivity = value / (-Int16.MinValue + Int16.MaxValue);
		}
		public double Decceleration { get => decceleration; set => decceleration = value; }
		public bool InvertX { get; set; }
		public bool InvertY { get; set; }

		private double sensitivity = 0.03;
		private bool isInitialPress = true;
		private (short x, short y) previous;
		private (double x, double y) amountStore;

		//field for calculating rolling
		private Task? doInertia;
		private bool isRolling = false;
		private Stopwatch stopwatch = new Stopwatch();
		private long elapsedTime;
		private double decceleration = 0.1;

		public PadTrackball() {}

		public PadTrackball(int sensitivity, bool hasInertia) {
			this.Sensitivity = sensitivity;
			this.HasInertia = hasInertia;
		}

		protected override void DoEventImpl(api.InputData e) {
			// if event is the initial press, then no movement has occured
			if (isInitialPress) {
				previous = e.Coordinates ?? throw new ArgumentException(e + " must be coordinal.");
				isRolling = false;
				isInitialPress = false;
				this.ClearSmoothingBuffer((0, 0, 0));
				stopwatch.Restart();
				return;
			}

			// else move the mouse relative to the previously touched location
			var coord = e.Coordinates ?? throw new ArgumentException(e + " must be coordinal.");
			stopwatch.Stop();
			elapsedTime = stopwatch.ElapsedMilliseconds;
			stopwatch.Restart();

			// compute mouse movement
			//var delta = this.SmoothInput((x: coord.x - previous.x, y: coord.y - previous.y));
			var delta = this.SoftTieredSmooth((x: coord.x - previous.x, y: coord.y - previous.y));
			//var movement = (x: delta.x * sensitivity, y: delta.y * sensitivity);
			var movement = this.AccelerateInput(delta.x, delta.y, sensitivity);
			if (InvertX) movement.x = -movement.x;
			if (InvertY) movement.y = -movement.y;

			// extract whole values so fractional input isn't lost and store previous
			this.Move(movement);
			previous = coord;
			var speed = (x: delta.x / (double)elapsedTime,
			             y: delta.y / (double)elapsedTime);
			
			// clean up after the trackpad is released
			if ((e.Flags & api.Flags.Released) == api.Flags.Released) {
				isInitialPress = true;
				amountStore = (0, 0);
				//this.coordsToSmooth.Clear();
				stopwatch.Stop();

				if (HasInertia) {
					isRolling = true;

					doInertia = Task.Run(() => {
						var speedMagnitude = (x: Math.Abs(speed.x), y: Math.Abs(speed.y));
						var magnitudeSign = (x: speed.x > 0 ? 1 : -1, y: speed.y > 0 ? 1 : -1);
						Thread.Sleep(10);
						// while guardian is a sanity check; stops rolling by simulating when the trackball loses
						// the momentum needed to overcum friction.  Constant is measured in velocity per millisecond
						while (Math.Sqrt(speed.x * speed.x + speed.y * speed.y) > 5) {
							if (!isRolling) {
								return;
							}
							// remove speed according to amount of decceleration
							speedMagnitude.x -= speedMagnitude.x * decceleration;
							speedMagnitude.y -= speedMagnitude.y * decceleration;
							
							var movement = (x: speedMagnitude.x * 10 * sensitivity * magnitudeSign.x,
							                y: speedMagnitude.y * 10 * sensitivity * magnitudeSign.y);
							//Console.WriteLine($"{speedMagnitude}");
							this.Move(movement);
							Thread.Sleep(10);
							//flickDistance.x += (-(mu * g) * 0.1);
							//flickDistance.y += (-(mu * g) * 0.1);
						}
					});
				}
			}
		}

		protected override void ReleaseAllImpl() {}

		// sends input while storing fractional values in a store so that the amount isn't
		// lost during the whole number conversion
		private void Move(double x, double y) {
			amountStore.x += x;
			amountStore.y += y;
			robot.MoveMouse((int)amountStore.x, (int)amountStore.y, relative: true);
			amountStore.x -= (int)amountStore.x;
			amountStore.y -= (int)amountStore.y;
		}

		private void Move((double x, double y) movement) => this.Move(movement.x, movement.y);
	}
}