using System;
using Newtonsoft.Json;
using SteamControllerApi;
using api = SteamControllerApi;

namespace Backend {
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
		private (short x, short y, short z) previous;
		private (double x, double y) amountStore;

		public override void DoEvent(api.IInputData input) {
			var (x, y, z, a) = (input as api.IMotionData ?? throw new ArgumentException("Data not motion.")).Gyroscope;
			
			
		}

		public override void ReleaseAll() {}

		public override void Unfreeze(IInputData newInput) => this.DoEvent(newInput);

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