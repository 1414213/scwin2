namespace SteamControllerApi {
	public interface IData {}

	public struct Coordinate<T, TT> {
		public T x, y, z;
		public TT w;

		public override string ToString() => "x: " + x + " y: " + y + " z: " + z + " w: " + w;

		public static implicit operator (T, T)(Coordinate<T, TT> coord) => (coord.x, coord.y);
		
		public static implicit operator (T, T, T, TT)(Coordinate<T, TT> coord) =>
			(coord.x, coord.y, coord.z, coord.w);

		public static implicit operator Coordinate<T, TT>((T x, T y) coord) =>
			new Coordinate<T, TT>{ x = coord.x, y = coord.y };

		public static implicit operator Coordinate<T, TT>((T yaw, T pitch, T roll, TT w) coord) =>
			new Coordinate<T, TT>{ x = coord.yaw, y = coord.pitch, z = coord.roll, w = coord.w };

		public void Deconstruct(out T x, out T y) { x = this.x; y = this.y;	}

		public void Deconstruct(out T x, out T y, out T z, out TT w) {
			x = this.x;
			y = this.y;
			z = this.z;
			w = this.w;
		}
	}
}