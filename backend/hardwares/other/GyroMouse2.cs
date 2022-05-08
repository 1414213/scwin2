using System;
using Newtonsoft.Json;
using SteamControllerApi;
using api = SteamControllerApi;

namespace Input {
	public class GyroMouse2 : Hardware {
		public double Sensitivity {
			get => (-Int16.MinValue + Int16.MaxValue) * sensitivity;
			set => sensitivity = value / (-Int16.MinValue + Int16.MaxValue);
		}
		public bool IsXYawElseRoll { get; set; } = true;
		public bool InvertX { get; set; }
		public bool InvertY { get; set; }

		private double sensitivity = 0.2;

		// yaw, pitch, and roll, respectively
		private (double x, double y, double z) previous = (0, 0, 0);
		private (double x, double y) amountStore;

		public override void DoEvent(api.IInputData input) {
			// x = roll  = heading
			// y = pitch = attitude
			// z = yaw   = bank
			var (x, y, z) = (input as api.IMotionData ?? throw new ArgumentException("Data not motion.")).Euler;
			var e = this.GetEuler(x, y, z);
			var movement = (
				x: (IsXYawElseRoll ? e.z - previous.z : e.x - previous.x) * (InvertX ? -1 : 1) * sensitivity,
				y: (e.y - previous.y) * (InvertY ? -1 : 1) * sensitivity
			);
			this.Move(movement);
			previous = e;
		}

		public override void ReleaseAll() {}

		public override void Unfreeze(IInputData newInput) {
			// Update previous.
			var (x, y, z) = (newInput as api.IMotionData ?? throw new ArgumentException("Data not motion.")).Euler;
			previous = this.GetEuler(x, y, z);
		}

		private (double x, double y, double z) GetEuler(short x, short y, short z) => (
			x: x / (x > 0 ? Int16.MaxValue : Int16.MinValue),
			y: y / (y > 0 ? Int16.MaxValue : Int16.MinValue),
			z: z / (z > 0 ? Int16.MaxValue : Int16.MinValue)
		);

		private void Move((double x, double y) movement) {
			amountStore.x += movement.x;
			amountStore.y += movement.y;
			robot.MoveMouse((int)amountStore.x, (int)amountStore.y, relative: true);
			amountStore.x -= (int)amountStore.x;
			amountStore.y -= (int)amountStore.y;
		}

		// Returns the rotation measured in radians.  Each coordinate has a range of [-pi, pi].
		private (double x, double y, double z) CoordinateAsRadians(short x, short y, short z) {
			return (this.NormalizeCoordinate(x), this.NormalizeCoordinate(y), this.NormalizeCoordinate(z));
		}

		private double NormalizeCoordinate(short c) {
			double n = 0;
			if (c > 0) n = c / (double)Int16.MaxValue;
			else if (c < 0) {
				n = c / (double)Int16.MinValue;
				n = -n;
			}
			n *= Math.PI;
			return n;
		}

		private int Difference(int current, int previous) {
			if (current <= Int16.MinValue/2 && previous > Int16.MaxValue/2) {
				return (current - Int16.MinValue + Int16.MaxValue) - previous;
			} else if (current > Int16.MaxValue/2 && previous <= Int16.MinValue/2) {
				return (current - Int16.MaxValue + Int16.MinValue) - previous;
			} else return current - previous;
		}
	}
}