using System;

public static class Extensions {
	/// <summary>
	/// Both the start and the end are inclusive.  The array returned contains shallow copies.
	/// </summary>
	public static T[] SliceNew<T>(this T[] sourceArray, int startingIndex, int endingIndex) {
		T[] destination = new T[(endingIndex - startingIndex) + 1];
		for (int i = 0; i < destination.Length; i++) {
			destination[i] = sourceArray[i + startingIndex];
		}
		return destination;
	}

	/// <summary>Perform Array.Reverse(T[] array) as a member and return a reference to this array.</summary>
	public static T[] Reverse<T>(this T[] array) {
		Array.Reverse(array);
		return array;
	}

	public static void Flip(this bool b) => b = b ? false : true;
}