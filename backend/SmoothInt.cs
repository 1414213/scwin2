// taken from http://gyrowiki.jibbsmart.com/blog:good-gyro-controls-part-1:the-gyro-is-a-mouse

using System;

namespace Backend {
	abstract public class SmoothInt : Hardware {
		public int Smoothing { get; set; } = 900;

		private (int x, int y, int z)[] buffer = new (int x, int y, int z)[16];
		private int bufferIndex;

		public SmoothInt(int bufferSize = 16) {
			this.buffer = new (int x, int y, int z)[bufferSize];
		}

		protected (int x, int y, int z) SmoothInput((int x, int y, int z) vector) {
			buffer[bufferIndex] = vector;

			var average = (x: 0, y: 0, z: 0);
			foreach (var v in buffer) {
				average.x += v.x;
				average.y += v.y;
				average.z += v.z;
			}

			bufferIndex = (bufferIndex + 1) % buffer.Length;
			return (average.x / buffer.Length, average.y / buffer.Length, average.z / buffer.Length);
		}

		protected (int x, int y) SmoothInput((int x, int y) vector) {
			var (x, y, _) = SmoothInput((vector.x, vector.y, 0));
			return (x, y);
		}

		protected void ClearSmoothingBuffer((int x, int y, int z) toClearTo) {
			for (int i = 0; i < buffer.Length; i++) {
				buffer[i] = toClearTo;
			}
		}

		protected (int x, int y, int z) SoftTieredSmooth((int x, int y, int z) vector) {
			double lowerThreshold = Smoothing / 2d;
			double upperThreshold = Smoothing;
			double magnitude = Math.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);

			// if magnitude is lower that threshold result is < 0 and so clamped value is zero.
			double directWeight = (magnitude - lowerThreshold) / (upperThreshold - lowerThreshold);
			directWeight = Math.Clamp(directWeight, 0, 1);

			(double x, double y, double z) smoothed = this.SmoothInput(vector);
			smoothed.x *= 1d - directWeight;
			smoothed.y *= 1d - directWeight;
			smoothed.z *= 1d - directWeight;
			var weightedVector = (x: vector.x * directWeight, y: vector.y * directWeight, z: vector.z * directWeight);
			//Console.WriteLine($"dw {directWeight}, ma {magnitude}");

			return ((short, short, short))(weightedVector.x + smoothed.x,
			                               weightedVector.y + smoothed.y,
			                               weightedVector.z + smoothed.z);
		}

		protected (int x, int y) SoftTieredSmooth((int x, int y) vector) {
			var v = SoftTieredSmooth((vector.x, vector.y, 0));
			return (v.x, v.y);
		}

		// private (short x, short y) SmoothCoords(short x, short y) {
		// 	if (queueCapacity <= 0) return (x, y);
		// 	else if (coordsToSmooth.Count < queueCapacity) coordsToSmooth.Enqueue((x, y));
		// 	else {
		// 		coordsToSmooth.Dequeue();
		// 		coordsToSmooth.Enqueue((x, y));
		// 	}
			
		// 	(int x, int y) smoothed = (0, 0);
		// 	foreach (var coord in coordsToSmooth) {
		// 		smoothed.x += coord.x;
		// 		smoothed.y += coord.y;
		// 	}
		// 	smoothed.x = smoothed.x / coordsToSmooth.Count;
		// 	smoothed.y = smoothed.y / coordsToSmooth.Count;
		// 	return ((short)smoothed.x, (short)smoothed.y);
		// }
	}

	public interface MSmoothing {
		int Smoothing { get; set; } // 900

		protected (int x, int y, int z)[] Buffer { get; set; } // new (int x, int y, int z)[16];
		protected int BufferIndex { get; set; }

		protected (int x, int y, int z) SmoothInput((int x, int y, int z) vector) {
			Buffer[BufferIndex] = vector;

			var average = (x: 0, y: 0, z: 0);
			foreach (var v in Buffer) {
				average.x += v.x;
				average.y += v.y;
				average.z += v.z;
			}

			BufferIndex = (BufferIndex + 1) % Buffer.Length;
			return (average.x / Buffer.Length, average.y / Buffer.Length, average.z / Buffer.Length);
		}

		protected (int x, int y) SmoothInput((int x, int y) vector) {
			var (x, y, _) = SmoothInput((vector.x, vector.y, 0));
			return (x, y);
		}

		protected void ClearSmoothingBuffer((int x, int y, int z) toClearTo) {
			for (int i = 0; i < Buffer.Length; i++) Buffer[i] = toClearTo;
		}

		protected (int x, int y, int z) SoftTieredSmooth((int x, int y, int z) vector) {
			double lowerThreshold = Smoothing / 2d;
			double upperThreshold = Smoothing;
			double magnitude = Math.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);

			// if magnitude is lower that threshold result is < 0 and so clamped value is zero.
			double directWeight = (magnitude - lowerThreshold) / (upperThreshold - lowerThreshold);
			directWeight = Math.Clamp(directWeight, 0, 1);

			(double x, double y, double z) smoothed = this.SmoothInput(vector);
			smoothed.x *= 1d - directWeight;
			smoothed.y *= 1d - directWeight;
			smoothed.z *= 1d - directWeight;
			var weightedVector = (x: vector.x * directWeight, y: vector.y * directWeight, z: vector.z * directWeight);
			//Console.WriteLine($"dw {directWeight}, ma {magnitude}");

			return ((short, short, short))(weightedVector.x + smoothed.x,
			                               weightedVector.y + smoothed.y,
			                               weightedVector.z + smoothed.z);
		}

		protected (int x, int y) SoftTieredSmooth((int x, int y) vector) {
			var (x, y, _) = SoftTieredSmooth((vector.x, vector.y, 0));
			return (x, y);
		}

		// private (short x, short y) SmoothCoords(short x, short y) {
		// 	if (queueCapacity <= 0) return (x, y);
		// 	else if (coordsToSmooth.Count < queueCapacity) coordsToSmooth.Enqueue((x, y));
		// 	else {
		// 		coordsToSmooth.Dequeue();
		// 		coordsToSmooth.Enqueue((x, y));
		// 	}
			
		// 	(int x, int y) smoothed = (0, 0);
		// 	foreach (var coord in coordsToSmooth) {
		// 		smoothed.x += coord.x;
		// 		smoothed.y += coord.y;
		// 	}
		// 	smoothed.x = smoothed.x / coordsToSmooth.Count;
		// 	smoothed.y = smoothed.y / coordsToSmooth.Count;
		// 	return ((short)smoothed.x, (short)smoothed.y);
		// }
	}
}