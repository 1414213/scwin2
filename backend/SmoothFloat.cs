// taken from http://gyrowiki.jibbsmart.com/blog:good-gyro-controls-part-1:the-gyro-is-a-mouse

using System;

namespace Backend {
	abstract public class SmoothDouble : Hardware {
		public int Smoothing { get; set; } = 900;

		private (double x, double y)[] buffer = new (double x, double y)[16];
		private int bufferIndex;

		public SmoothDouble(int bufferSize = 16) {
			this.buffer = new (double x, double y)[bufferSize];
		}

		protected (double x, double y) SmoothInput((double x, double y) vector) {
			buffer[bufferIndex] = vector;

			var average = (x: 0d, y: 0d);
			foreach (var v in buffer) {
				average.x += v.x;
				average.y += v.y;
			}

			bufferIndex = (bufferIndex + 1) % buffer.Length;
			return (average.x / buffer.Length, average.y / buffer.Length);
		}

		protected void ClearSmoothingBuffer((double x, double y) toClearTo) {
			for (int i = 0; i < buffer.Length; i++) {
				buffer[i] = toClearTo;
			}
		}

		protected (double x, double y) SoftTieredSmooth((double x, double y) vector) {
			var lowerThreshold = Smoothing / 2d;
			var upperThreshold = (double)Smoothing;
			var magnitude = Math.Sqrt(vector.x * vector.x + vector.y * vector.y);

			// If magnitude is lower that threshold result is < 0 and so clamped value is zero.
			var directWeight = (magnitude - lowerThreshold) / (upperThreshold - lowerThreshold);
			directWeight = Math.Clamp(directWeight, 0, 1);

			var smoothed = ((double x, double y))this.SmoothInput(vector);
			smoothed.x *= 1d - directWeight;
			smoothed.y *= 1d - directWeight;
			var weightedVector = (x: vector.x * directWeight, y: vector.y * directWeight);

			return ((double, double))(weightedVector.x + smoothed.x, weightedVector.y + smoothed.y);
		}
	}

	abstract public class SmoothFloat : Hardware {
		public int Smoothing { get; set; } = 900;

		private (float x, float y)[] buffer = new (float x, float y)[16];
		private int bufferIndex;

		public SmoothFloat(int bufferSize = 16) {
			this.buffer = new (float x, float y)[bufferSize];
		}

		protected (float x, float y) SmoothInput((float x, float y) vector) {
			buffer[bufferIndex] = vector;

			var average = (x: 0f, y: 0f);
			foreach (var v in buffer) {
				average.x += v.x;
				average.y += v.y;
			}

			bufferIndex = (bufferIndex + 1) % buffer.Length;
			return (average.x / buffer.Length, average.y / buffer.Length);
		}

		protected void ClearSmoothingBuffer((float x, float y) toClearTo) {
			for (int i = 0; i < buffer.Length; i++) {
				buffer[i] = toClearTo;
			}
		}

		protected (float x, float y) SoftTieredSmooth((float x, float y) vector) {
			var lowerThreshold = Smoothing / 2d;
			var upperThreshold = (double)Smoothing;
			var magnitude = Math.Sqrt(vector.x * vector.x + vector.y * vector.y);

			// If magnitude is lower that threshold result is < 0 and so clamped value is zero.
			var directWeight = (magnitude - lowerThreshold) / (upperThreshold - lowerThreshold);
			directWeight = Math.Clamp(directWeight, 0, 1);

			var smoothed = ((double x, double y))this.SmoothInput(vector);
			smoothed.x *= 1d - directWeight;
			smoothed.y *= 1d - directWeight;
			var weightedVector = (x: vector.x * directWeight, y: vector.y * directWeight);

			return ((float, float))(weightedVector.x + smoothed.x, weightedVector.y + smoothed.y);
		}
	}
}