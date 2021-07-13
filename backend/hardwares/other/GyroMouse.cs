using System;
using Newtonsoft.Json;
using api = SteamControllerApi;

// x = roll   = heading
// y = pitch = attitude
// z = yaw  = bank

namespace Backend {
	public class GyroMouse : Hardware {
		/// <summary>Amount of pixels covered by the entire range of rotation.</summary>
		public double Sensitivity { get => sensitivity; set => sensitivity = value; }
		public bool XIsYawElseRoll { get; set; } = true;
		public bool InvertX { get; set; }
		public bool InvertY { get; set; }

		private double sensitivity = 2000;

		private (double x, double y) previous;
		private (double x, double y) amountStore;

		public override void DoEvent(api.IInputData input) {
			var gyroscope = (input as api.IMotionData ?? throw new ArgumentException("Data not motion.")).Gyroscope;

			var (roll, pitch, yaw) = this.QuaternionToEuler(gyroscope);
			var x = XIsYawElseRoll ? yaw : roll;
			var y = pitch;
			var delta = (x: (x - previous.x) / Math.PI * sensitivity,
			             y: (y - previous.y) / Math.PI * sensitivity);
			if (Hardware.EventDoer is not null) {
				if (Hardware.EventDoer.Debug == 4)
					Console.WriteLine($"x: {x} y: {y} {delta}");
			}

			if (InvertX) delta.x = -delta.x;
			if (InvertY) delta.y = -delta.y;
			this.Move(delta);
			
			previous = (x, y);
		}

		public override void ReleaseAll() {}

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