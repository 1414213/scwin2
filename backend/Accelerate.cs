using System;

namespace Backend {
	public abstract class Accelerate {
		// Multiple of the existing sensitivity.
		public double Acceleration { get; set; } = 2;
		public int AccelerationLowerBoundary { get; set; } = 2000;
		public int AccelerationUpperBoundary { get; set; } = 1700;

		protected (double x, double y) AccelerateInput(int x, int y, double startingSensitivity) {
			double finalSensitivity = startingSensitivity * Acceleration;
			double magnitude = Math.Sqrt(x * x + y * y);
			double weight = Math.Clamp(
				value: (magnitude - AccelerationLowerBoundary) / (AccelerationUpperBoundary - AccelerationLowerBoundary),
				min: 0,
				max: 1);
			double newSensitivity = startingSensitivity * weight + finalSensitivity * (1d - weight);

			return (x * newSensitivity, y * newSensitivity);
		}
	}

	public interface MAcceleration {
		// Multiple of the existing sensitivity.
		public double Acceleration { get; set; } // = 2;
		public int AccelerationLowerBoundary { get; set; } // = 2000;
		public int AccelerationUpperBoundary { get; set; } // = 1700;

		public (double x, double y) AccelerateInput(int x, int y, double startingSensitivity) {
			var finalSensitivity = startingSensitivity * Acceleration;
			var magnitude = Math.Sqrt(x * x + y * y);
			var weight = Math.Clamp(
				value: (magnitude - AccelerationLowerBoundary) / (AccelerationUpperBoundary - AccelerationLowerBoundary),
				min: 0,
				max: 1);
			var newSensitivity = startingSensitivity * weight + finalSensitivity * (1d - weight);

			return (x * newSensitivity, y * newSensitivity);
		}
	}
}