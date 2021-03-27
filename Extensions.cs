using System;
//using System.Collections.Generic;

namespace MethodExtensions {
	public static class Extensions {

		/* Both the start and the end are inclusive.
		 * The array returned contains shallow copies.
		 * */
		public static T[] SliceNew<T>(this T[] sourceArray, int startingIndex, int endingIndex) {
			T[] destination = new T[(endingIndex - startingIndex) + 1];
			for (int i = 0; i < destination.Length; i++) {
				destination[i] = sourceArray[i + startingIndex];
			}
			return destination;
		}

		public static T[] Reverse<T>(this T[] array) {
			Array.Reverse(array);
			return array;
		}

		public static void Flip(this bool b) => b = b ? false : true;
	}
}