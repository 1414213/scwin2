using System;
using Backend.Math64;
using api = SteamControllerApi;

// x = roll  = heading
// y = pitch = attitude
// z = yaw   = bank

namespace Backend {
	public class GyroMouse : Hardware {
		/// <summary>Amount of pixels covered by the entire range of rotation.</summary>
		public double Sensitivity { get => sensitivity * Math.PI * 2; set => sensitivity = value / (Math.PI * 2); }
		public bool XIsYawElseRoll { get; set; } = true;
		public bool InvertX { get; set; }
		public bool InvertY { get; set; }

		private double sensitivity = 1000;

		//private Quaternion previous = Quaternion.Identity;
		private (double roll, double pitch, double yaw) previous = (0, 0, 0);
		private (double x, double y) amountStore;

		public override void DoEvent(api.IInputData input) {
			var (x, y, z, w) = (input as api.IMotionData ?? throw new ArgumentException("Data not motion.")).Quaternion;

			// Get euler angles.
			var q = (
				x: (double)x / (x > 0 ? Int16.MaxValue : Int16.MinValue),
				y: (double)y / (y > 0 ? Int16.MaxValue : Int16.MinValue),
				z: (double)z / (z > 0 ? Int16.MaxValue : Int16.MinValue),
				w: (double)w / (w > 0 ? Int16.MaxValue : Int16.MinValue)
			);
			var e = this.QuaternionToEuler(q);
			e.roll += 1;
			e.pitch += 1;
			e.yaw += 1;

			// Compute change in rotation to create movement.
			// (double roll, double pitch, double yaw) delta;
			// {
			// 	var twoPi = Math.PI * 2;
			// 	var roll  = (e.roll - previous.roll) % twoPi;
			// 	var pitch = (e.pitch - previous.pitch) % twoPi;
			// 	var yaw   = (e.yaw - previous.yaw) % twoPi;
			// 	//yaw   = yaw < 0 ? yaw + twoPi : yaw > twoPi ? yaw % twoPi : yaw;
			// 	delta = (roll, pitch, yaw);
			// }
			// double Mod(double n, double m) => (n % m + m) % m;
			/// <summary>angles are proportions of pi, measuring from 0 to 2.</summary>
			double AngleDifference(ref double x, ref double y) => (x - y + 1) % 2 - 1;
			var delta = (
				roll: AngleDifference(ref e.roll, ref previous.roll),
				pitch: AngleDifference(ref e.pitch, ref previous.pitch),
				yaw: AngleDifference(ref e.yaw, ref previous.yaw)
			);

			var movement = (
				x: (XIsYawElseRoll ? delta.yaw : delta.roll) * (InvertX ? -1 : 1) * sensitivity,
				y: delta.pitch * (InvertY ? -1 : 1) * sensitivity
			);

			// movement = base.SoftTieredSmooth(movement);
			this.Move(movement);

			previous = e;
		}

		public override void ReleaseAll() {}

		public override void Unfreeze(api.IInputData newInput) {
			var (x, y, z, w) = (newInput as api.IMotionData
				?? throw new ArgumentException("Data not motion.")
				).Quaternion;
			// Update previous.
			var q = (
				x: (double)x / (x > 0 ? Int16.MaxValue : Int16.MinValue),
				y: (double)y / (y > 0 ? Int16.MaxValue : Int16.MinValue),
				z: (double)z / (z > 0 ? Int16.MaxValue : Int16.MinValue),
				w: (double)w / (w > 0 ? Int16.MaxValue : Int16.MinValue)
			);
			previous = this.QuaternionToEuler(q);
			
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
		
		// Reference: https://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToEuler/
		private (double roll, double pitch, double yaw) QuaternionToEuler(double x, double y, double z, double w) {
			double roll, pitch, yaw, test = x * y + z * w;
			if (test > 0.499) {
			//if (test is > 0.499 or < 0.501) {
				roll = 2.0 * Math.Atan2(x, w);
				pitch = Math.PI / 2.0;
				return (roll, pitch, 0);
			} else if (test < -0.499) {
			//} else if (test is < -0.499 or > -0.501) {
				roll = -2.0 * Math.Atan2(x, w);
				pitch = -Math.PI / 2.0;
				return (roll, pitch, 0);
			}
			roll = Math.Atan2(2d * y * w - 2d * x * z, 1d - 2d * y * y - 2d * z * z);
			pitch = Math.Asin(2d * test);
			yaw = Math.Atan2(2d * x * w - 2d * y * z, 1d - 2d * x * x - 2d * z * z);
			return (roll, pitch, yaw);
		}

		private (double roll, double pitch, double yaw) QuaternionToEuler(
			(double x, double y, double z, double w) quaternion
		) => this.QuaternionToEuler(quaternion.x, quaternion.y, quaternion.z, quaternion.w);
	}
}