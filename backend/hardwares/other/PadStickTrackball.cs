using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using SteamControllerApi;
using api = SteamControllerApi;


namespace Backend {
	public class PadStickTrackball : Trackpad, MAcceleration {
		public double Acceleration { get; set; } = 2;
		public int AccelerationLowerBoundary { get; set; } = 2000;
		public int AccelerationUpperBoundary { get; set; } = 1700;

		public bool HasInertia { get; set; } = true;
		public bool InvertX { get; set; }
		public bool InvertY { get; set; }
		public double Sensitivity {
			get => (-Int16.MinValue + Int16.MaxValue) * sensitivity;
			set => sensitivity = value / (-Int16.MinValue + Int16.MaxValue);
		}
		public double Decceleration { get; set; } = 0.1;

		private MAcceleration accel => this as MAcceleration;
		private bool isInitialPress = true, isRolling;
		private long elapsedTime;
		private double sensitivity = 0.03;
		private (short x, short y) previous;
		private (double x, double y) amountStore;
		private Task doInertia = null!;
		private CancellationToken cancelrolling;
		private Stopwatch stopwatch = new Stopwatch();

		public PadStickTrackball() {}

		public PadStickTrackball(double sensitivity, bool invertX, bool invertY) {
			this.Sensitivity = sensitivity;
			this.InvertX = invertX;
			this.InvertY = invertY;
			this.HasInertia = false;
		}

		public PadStickTrackball(double sensitivity,
		                         double decceleration, 
		                         bool invertX = false, bool invertY = false) : this(sensitivity, invertX, invertY) {
			this.Decceleration = decceleration;
			this.HasInertia = true;
		}

		protected override void DoEventImpl(api.ITrackpadData input) {
			var coord = input.Position;
			// if event is the initial press, then no movement has occured
			if (isInitialPress) {
				previous = coord;
				isRolling = false;
				isInitialPress = false;
				stopwatch.Restart();
				return;
			}

			// else move the mouse relative to the previously touched location
			stopwatch.Stop();
			elapsedTime = stopwatch.ElapsedMilliseconds;
			stopwatch.Restart();

			// compute mouse movement
			var delta = (x: coord.x - previous.x, y: coord.y - previous.y);
			var movement = this.AccelerateInput(delta.x, delta.y, sensitivity);
			if (InvertX) movement.x = -movement.x;
			if (InvertY) movement.y = -movement.y;

			// extract whole values so fractional input isn't lost and store previous
			this.Move(movement);
			previous = coord;
			
			// clean up after the trackpad is released
			if (input.IsRelease) {
				isInitialPress = true;
				amountStore = (0, 0);
				stopwatch.Stop();

				if (HasInertia) {
					isRolling = true;
					var speed = (x: delta.x / (double)elapsedTime, y: delta.y / (double)elapsedTime);

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
							speedMagnitude.x -= speedMagnitude.x * Decceleration;
							speedMagnitude.y -= speedMagnitude.y * Decceleration;
							
							var movement = (x: speedMagnitude.x * 10 * sensitivity * magnitudeSign.x,
							                y: speedMagnitude.y * 10 * sensitivity * magnitudeSign.y);
							//Console.WriteLine($"{speedMagnitude}");
							this.Move(movement);
							Thread.Sleep(10);
						}
					});
				}
			}
		}

		protected override void ReleaseAllImpl() {}

		public override void Unfreeze(IInputData newInput) {
			// Reset previous input.
			isInitialPress = true;
			base.ClearSmoothingBuffer((0, 0, 0));

			this.DoEvent(newInput);
		}

		private void Move((double x, double y) movement) {
			amountStore.x += movement.x;
			amountStore.y += movement.y;
			robot.MoveLStick((short)amountStore.x, (short)amountStore.y);
			amountStore.x -= (int)amountStore.x;
			amountStore.y -= (int)amountStore.y;
		}

		private (double x, double y) AccelerateInput(int x, int y, double startingSensitivity) {
			return accel.AccelerateInput(x, y, startingSensitivity);
		}
	}
}

