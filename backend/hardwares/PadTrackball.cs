using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using api = SteamControllerApi;

namespace Input {
	public class PadTrackball : Trackpad {
		public Accelerator Acceleration { get; set; } = new Accelerator(amount: 2, kind: Curve.Wide);
		public bool HasInertia { get; set; } = true;
		public double Sensitivity {
			get => (-Int16.MinValue + Int16.MaxValue) * sensitivity;
			set => sensitivity = value / (-Int16.MinValue + Int16.MaxValue);
		}
		public double Decceleration { get => decceleration; set => decceleration = value; }
		public int Smoothing { get => smoother.Smoothing; set => smoother.Smoothing = value; }
		public bool InvertX { get; set; }
		public bool InvertY { get; set; }

		private double sensitivity = 0.03;
		private bool isInitialPress = true;
		private (int x, int y) previous;
		private (double x, double y) amountStore;
		private PointSmoother smoother = new PointSmoother(500, bufferSize: 16);

		// Fields for calculating rolling:
		private Task? doInertia;
		private bool isRolling = false;
		private Stopwatch stopwatch = new Stopwatch();
		private long elapsedTime;
		private double decceleration = 0.1;

		protected override void DoEventImpl(api.ITrackpadData input) {
			var e = input as api.ITrackpadData ?? throw new ArgumentException(input + " must be of trackpad.");
			
			// If event is the initial press, then no movement has occured -
			if (isInitialPress) {
				previous = e.Position;
				isRolling = false;
				isInitialPress = false;
				smoother.ClearSmoothingBuffer();
				stopwatch.Restart();
				return;
			}

			// - else move the mouse relative to the previously touched location.
			stopwatch.Stop();
			elapsedTime = stopwatch.ElapsedMilliseconds;
			stopwatch.Restart();

			// Compute mouse movement:
			//var coord = smoother.SoftTieredSmooth(e.Position);
			var coord = e.Position;
			//var delta = this.SmoothInput((x: coord.x - previous.x, y: coord.y - previous.y));
			var delta = smoother.SoftTieredSmooth((x: coord.x - previous.x, y: coord.y - previous.y));
			//var delta = (x: coord.x - previous.x, y: coord.y - previous.y);
			//var movement = (x: delta.x * sensitivity, y: delta.y * sensitivity);
			var movement = this.AccelerateInput(delta.x, delta.y, sensitivity);
			if (InvertX) movement.x = -movement.x;
			if (InvertY) movement.y = -movement.y;

			// Extract whole values so fractional input isn't lost and store previous.
			this.Move(movement);
			previous = coord;
			
			// Clean up after the trackpad is released.
			if (e.IsRelease) {
				isInitialPress = true;
				amountStore = (0, 0);
				smoother.ClearSmoothingBuffer();
				stopwatch.Stop();

				if (HasInertia) {
					isRolling = true;
					var speed = (x: delta.x / (double)elapsedTime, y: delta.y / (double)elapsedTime);

					doInertia = Task.Run(() => {
						var speedMagnitude = (x: Math.Abs(speed.x), y: Math.Abs(speed.y));
						var magnitudeSign = (x: speed.x > 0 ? 1 : -1, y: speed.y > 0 ? 1 : -1);
						var currentSensitivity = movement.x / delta.x;
						Thread.Sleep(10);

						// While guardian is a sanity check; stops rolling by simulating when the trackball loses
						// the momentum needed to overcum friction.  Constant is measured in velocity per millisecond.
						while (Math.Sqrt(
							speedMagnitude.x * speedMagnitude.x + speedMagnitude.y * speedMagnitude.y
						) > 5 && isRolling) {
							// Remove speed according to amount of decceleration.
							speedMagnitude.x -= speedMagnitude.x * decceleration;
							speedMagnitude.y -= speedMagnitude.y * decceleration;
							
							var movement = (x: speedMagnitude.x * 10 * currentSensitivity * magnitudeSign.x,
							                y: speedMagnitude.y * 10 * currentSensitivity * magnitudeSign.y);
							this.Move(movement);
							Thread.Sleep(10);
						}
					});
				}
			}
		}

		protected override void ReleaseAllImpl() {}

		public override void Unfreeze(api.IInputData newInput) {
			// Reset previous input.
			isInitialPress = true;
			smoother.ClearSmoothingBuffer();

			this.DoEvent(newInput);
		}

		// Sends input while storing fractional values in a store so that the amount isn't
		// lost during the whole number conversion.
		private void Move(double x, double y) {
			amountStore.x += x;
			amountStore.y += y;
			robot.MoveMouse((int)amountStore.x, (int)amountStore.y, relative: true);
			amountStore.x -= (int)amountStore.x;
			amountStore.y -= (int)amountStore.y;
		}

		private void Move((double x, double y) movement) => this.Move(movement.x, movement.y);

		private (double x, double y) AccelerateInput(int x, int y, double startingSensitivity) {
			return Acceleration.AccelerateInput(x, y, startingSensitivity);
		}
	}
}