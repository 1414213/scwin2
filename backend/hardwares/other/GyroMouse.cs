using System;
using System.Numerics;
using api = SteamControllerApi;

// x = roll  = heading
// y = pitch = attitude
// z = yaw   = bank

namespace Backend {
	public class GyroMouse : SmoothFloat {
		/// <summary>Amount of pixels covered by the entire range of rotation.</summary>
		public float Sensitivity { get => sensitivity; set => sensitivity = value; }
		public bool XIsYawElseRoll { get; set; } = true;
		public bool InvertX { get; set; }
		public bool InvertY { get; set; }

		private float sensitivity = 10000;

		private Quaternion previous = Quaternion.Identity;
		private (double x, double y) amountStore;

		public override void DoEvent(api.IInputData input) {
			var (x, y, z, w) = (input as api.IMotionData ?? throw new ArgumentException("Data not motion.")).Gyroscope;

			var quaternion = new Quaternion(
				(float)x / (x > 0 ? Int16.MaxValue : Int16.MinValue),
				(float)y / (y > 0 ? Int16.MaxValue : Int16.MinValue),
				(float)z / (z > 0 ? Int16.MaxValue : Int16.MinValue),
				(float)w / (w > 0 ? Int16.MaxValue : Int16.MinValue));
			var delta = Quaternion.Subtract(quaternion, previous);
			var movement = (
				x: (XIsYawElseRoll ? delta.Z : delta.X) * (InvertX ? -1 : 1) * sensitivity,
				y: delta.Y * (InvertY ? -1 : 1) * sensitivity
			);
			movement = base.SoftTieredSmooth(movement);
			if (Hardware.EventDoer.Debug == 4) {
				Console.WriteLine("" + quaternion + " " + movement);
			}
			this.Move(movement);

			previous = quaternion;
		}

		public override void ReleaseAll() {}

		public override void Unfreeze(api.IInputData newInput) {
			var (x, y, z, w) = (newInput as api.IMotionData ?? throw new ArgumentException("Data not motion."))
			                   .Gyroscope;
			// Update previous.
			previous = new Quaternion(
				(float)x / (x > 0 ? Int16.MaxValue : Int16.MinValue),
				(float)y / (y > 0 ? Int16.MaxValue : Int16.MinValue),
				(float)z / (z > 0 ? Int16.MaxValue : Int16.MinValue),
				(float)w / (w > 0 ? Int16.MaxValue : Int16.MinValue));
			
			// Clear smoothing buffer since previous input has been reset.
			base.ClearSmoothingBuffer((0, 0));

			this.DoEvent(newInput);
		}

		private void Move((double x, double y) movement) {
			amountStore.x += movement.x;
			amountStore.y += movement.y;
			robot.MoveMouse((int)amountStore.x, (int)amountStore.y, relative: true);
			amountStore.x -= (int)amountStore.x;
			amountStore.y -= (int)amountStore.y;
		}
		
		// Reference: https://automaticaddison.com/how-to-convert-a-quaternion-into-euler-angles-in-python/
		// Pitch is wrong in previous reference
		// Correct reference: https://graphics.fandom.com/wiki/Conversion_between_quaternions_and_Euler_angles
		private (double roll, double pitch, double yaw) QuaternionToEuler(short x, short y, short z, short w) {
			double roll = Math.Atan2(2d * (w * x + y * z), 1d - 2d * (x * x + y * y));
			double yy = 2d * (w * y - z * x);
			double pitch = Math.Asin(yy > 1 ? 1 : (yy < -1 ? -1 : yy));
			double yaw = Math.Atan2(2d * (w * z + x * y), 1d - 2d * (y * y + z * z));
			return (roll, pitch, yaw);
		}

		private (double roll, double pitch, double yaw) QuaternionToEuler(
			(short x, short y, short z, short w) quaternion
		) => this.QuaternionToEuler(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
	}
}