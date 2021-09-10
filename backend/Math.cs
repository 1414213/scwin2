using System;

namespace Backend.CustomMath {
	public struct Vec3 {
		public double x, y, z;

		public Vec3(double x, double y, double z) {
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public static double Dot(Vec3 v, Vec3 u) => v.x * u.x + v.y * u.y + v.z * u.z;

		public static Vec3 Cross(Vec3 v, Vec3 u) {
			return new Vec3(
				x: v.y * u.z - v.z * u.y,
				y: v.z * u.x - v.x * u.z,
				z: v.x * u.y - v.y * u.x);
		}

		public static Vec3 operator +(Vec3 v) => v;

		public static Vec3 operator +(Vec3 v, Vec3 u) => new Vec3(v.x + u.x, v.y + u.y, v.z + u.z);

		public static Vec3 operator -(Vec3 v) => new Vec3(-v.x, -v.y, -v.z);

		public static Vec3 operator -(Vec3 v, Vec3 u) => v + (-u);

		public static Vec3 operator *(double scalar, Vec3 v) => new Vec3(scalar * v.x, scalar * v.y, scalar * v.z);

		public double Dot(Vec3 right) => Vec3.Dot(this, right);

		public Vec3 Cross(Vec3 right) => Vec3.Cross(this, right);

		public override string ToString() => "(x:" + x + ", y:" + y + ", z:" + z + ")";
	}

	public struct Quaternion {
		private double w;
		private Vec3 v;

		public double X => v.x;
		public double Y => v.y;
		public double Z => v.z;
		public double W => w;

		public static Quaternion Identity => new Quaternion(0, 0, 0, 1);

		public Quaternion(double x, double y, double z, double w) {
			var e = new ArgumentException("Quaternion values must be between -1 and 1.");
			if (x is < -1 or > 1) throw e;
			if (y is < -1 or > 1) throw e;
			if (z is < -1 or > 1) throw e;
			if (w is < -1 or > 1) throw e;

			this.v = new Vec3(x, y, z);
			this.w = w;
		}

		public Quaternion(double w, Vec3 v) : this(v.x, v.y, v.z, w) {}

		public static Quaternion operator +(Quaternion a) => a;

		public static Quaternion operator +(
			Quaternion a,
			Quaternion b
		) => new Quaternion(a.W + b.W, a.ToVec3().v + b.ToVec3().v);

		public static Quaternion operator *(Quaternion a, Quaternion b) {
			var (_, v) = a.ToVec3();
			var (_, u) = b.ToVec3();
			return new Quaternion(
				w: a.W * b.W - v.Dot(u),
				v: a.W * u + b.W * v + v.Cross(u));
		}

		public static Quaternion Difference(Quaternion a, Quaternion b) => a.Difference(b);

		public Quaternion Conjugate() => new Quaternion(w, -v);

		public Quaternion Inverse() {
			var magnitude = Math.Sqrt(W * W + X * X + Y * Y + Z * Z);
			return new Quaternion(W / (magnitude * magnitude), (1 / (magnitude * magnitude)) * v);
		}

		public Quaternion Difference(Quaternion right) => this * right.Inverse();

		// Reference: https://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToEuler/
		public (double roll, double pitch, double yaw) ToEuler() {
			double roll, pitch, yaw, test = X * Y + Z * W;
			if (test > 0.499) {
				roll = 2.0 * Math.Atan2(X, w);
				pitch = Math.PI / 2.0;
				return (roll, pitch, 0);
			} else if (test < -0.499) {
				roll = -2.0 * Math.Atan2(X, w);
				pitch = -Math.PI / 2.0;
				return (roll, pitch, 0);
			}
			roll = Math.Atan2(2d * Y * W - 2d * X * Z, 1d - 2d * Y * Y - 2d * Z * Z);
			pitch = Math.Asin(2d * test);
			yaw = Math.Atan2(2d * X * W - 2d * Y * Z, 1d - 2d * X * X - 2d * Z * Z);
			return (roll, pitch, yaw);
		}

		public (double x, double y, double z, double w) ToTuple() => (X, Y, Z, W);

		public (double w, Vec3 v) ToVec3() => (W, v);

		public override string ToString() => "(W:" + W + " " + v + ")";
	}
}