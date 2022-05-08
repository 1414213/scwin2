using System;
using Input.MathDouble;
using api = SteamControllerApi;

// x = roll  = heading
// y = pitch = attitude
// z = yaw   = bank

namespace Input {
	public class GyroMouse : Hardware {
		/// <summary>Amount of pixels covered by the entire range of rotation.</summary>
		public double Sensitivity { get => sensitivity * Math.PI * 2; set => sensitivity = value / (Math.PI * 2); }
		public bool XIsYawElseRoll { get; set; } = true;
		public bool InvertX { get; set; }
		public bool InvertY { get; set; }

		private double sensitivity = 1000;

		private Quaternion previous = Quaternion.Identity;
		//private (double roll, double pitch, double yaw) previous = (0, 0, 0);
		private (double x, double y) amountStore;

		public override void DoEvent(api.IInputData input) {
			var (x, y, z, w) = (input as api.IMotionData
				?? throw new ArgumentException("Data not motion.")).Quaternion;

			// Get change in orientation.
			var q = new Quaternion(
				x: (double)x / (x > 0 ? Int16.MaxValue : Int16.MinValue),
				y: (double)y / (y > 0 ? Int16.MaxValue : Int16.MinValue),
				z: (double)z / (z > 0 ? Int16.MaxValue : Int16.MinValue),
				w: (double)w / (w > 0 ? Int16.MaxValue : Int16.MinValue));
			var (roll, pitch, yaw) = q.Difference(previous).ToEuler();

			// Set to range of [-1, 1] so that one rotation is one pixel of movement.
			roll /= Math.PI;
			pitch /= Math.PI;
			yaw /= Math.PI;
			Console.WriteLine($"{q} roll {roll} pitch {pitch} yaw {yaw}");

			var movement = (
				x: (XIsYawElseRoll ? yaw : roll) * (InvertX ? -1 : 1) * sensitivity,
				y: pitch * (InvertY ? -1 : 1) * sensitivity
			);

			// movement = base.SoftTieredSmooth(movement);
			this.Move(movement);

			previous = q;
		}

		public override void ReleaseAll() {}

		public override void Unfreeze(api.IInputData newInput) {
			var (x, y, z, w) = (newInput as api.IMotionData
				?? throw new ArgumentException("Data not motion.")).Quaternion;

			// Update previous.
			previous = new Quaternion(
				x: (double)x / (x > 0 ? Int16.MaxValue : Int16.MinValue),
				y: (double)y / (y > 0 ? Int16.MaxValue : Int16.MinValue),
				z: (double)z / (z > 0 ? Int16.MaxValue : Int16.MinValue),
				w: (double)w / (w > 0 ? Int16.MaxValue : Int16.MinValue));
			
			// Clear smoothing buffer since previous input has been reset.
			// base.ClearSmoothingBuffer((0, 0));
		}

		private void Move((double x, double y) movement) {
			amountStore.x += movement.x;
			amountStore.y += movement.y;
			robot.MoveMouse((int)amountStore.x, (int)amountStore.y, relative: true);
			amountStore.x -= (int)amountStore.x;
			amountStore.y -= (int)amountStore.y;
		}
	}
}