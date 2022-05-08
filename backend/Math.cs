using System;

namespace Input.MathDouble {
	public struct Vec3 {
		public double x, y, z;

		public Vec3(double x, double y, double z) {
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public static Vec3 operator +(Vec3 v) => v;

		public static Vec3 operator +(Vec3 v, Vec3 u) => new Vec3(v.x + u.x, v.y + u.y, v.z + u.z);

		public static Vec3 operator -(Vec3 v) => new Vec3(-v.x, -v.y, -v.z);

		public static Vec3 operator -(Vec3 v, Vec3 u) => v + (-u);

		public static Vec3 operator *(Vec3 v, double scalar) => new Vec3(v.x * scalar, v.y * scalar, v.z * scalar);

		public static Vec3 operator /(Vec3 v, double scalar) => new Vec3(v.x / scalar, v.y / scalar, v.z / scalar);

		public static double Dot(Vec3 v, Vec3 u) => v.x * u.x + v.y * u.y + v.z * u.z;

		public static Vec3 Cross(Vec3 v, Vec3 u) => new Vec3(
			x: v.y * u.z - v.z * u.y,
			y: v.z * u.x - v.x * u.z,
			z: v.x * u.y - v.y * u.x);

		public double Dot(Vec3 right) => Vec3.Dot(this, right);

		public Vec3 Cross(Vec3 right) => Vec3.Cross(this, right);

		public (double x, double y, double z) ToTuple() => (x, y, z);

		public override string ToString() => "{x: " + x + ", y: " + y + ", z: " + z + "}";
	}

	public struct Quaternion {
		private double w;
		private Vec3 v;

		public double X => v.x;
		public double Y => v.y;
		public double Z => v.z;
		public double W => w;

		public static Quaternion Identity => new Quaternion(0, 0, 0, 1);

		public Quaternion(double w, Vec3 v) { this.w = w; this.v = v; }

		public Quaternion(double x, double y, double z, double w) : this(w, new Vec3(x, y, z)) {}

		public static Quaternion operator +(Quaternion a) => a;

		public static Quaternion operator +(Quaternion a, Quaternion b) => new Quaternion(a.W + b.W, a.v + b.v);

		// References Handmade-Math: https://github.com/HandmadeMath/Handmade-Math

		public static Quaternion operator -(Quaternion a) => new Quaternion(-a.X, -a.Y, -a.Z, -a.W);

		public static Quaternion operator -(Quaternion a, Quaternion b) =>
			new Quaternion(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);

		public static Quaternion operator *(Quaternion a, Quaternion b) => new Quaternion(
			x: a.X * b.W + a.Y * b.Z - a.Z * b.Y + a.W * b.X,
			y: (-a.X * b.Z) + a.Y * b.W + a.Z * b.X + a.W * b.Y,
			z: a.X * b.Y - a.Y * b.X + a.Z * b.W + a.W * b.Z,
			w: (-a.X * b.X) - a.Y * b.Y - a.Z * b.Z + a.W * b.W);

		public static Quaternion operator *(Quaternion a, double scalar) =>
			new Quaternion(a.X * scalar, a.Y * scalar, a.Z * scalar, a.W * scalar);

		public static Quaternion operator /(Quaternion a, double scalar) =>
			new Quaternion(a.X / scalar, a.Y / scalar, a.Z / scalar, a.W / scalar);

		public static double Dot(Quaternion a, Quaternion b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;

		public double Dot(Quaternion b) => Dot(this, b);

		public static Quaternion Inverse(Quaternion a) => new Quaternion(-a.X, -a.Y, -a.Z, a.W);

		public Quaternion Inverse() => Inverse(this);

		public static Quaternion Difference(Quaternion a, Quaternion b) => a * b.Inverse();

		public Quaternion Difference(Quaternion b) => Difference(this, b);

		public static Quaternion Normalize(Quaternion a) => a / Math.Sqrt(a.Dot(a));

		public Quaternion Normalize() => Normalize(this);

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

		public override string ToString() => "{W: " + W + ", X: " + X + ", Y: " + Y + ", Z: " + Z + "}";
	}
}