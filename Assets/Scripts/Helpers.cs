using System;

public class Util {

	public static bool[, ,] CopyMap(bool[, ,] given) {
		bool[, ,] ret = new bool[given.GetLength (0), given.GetLength (1), given.GetLength (2)];
		for (int i = 0; i < given.GetLength (0); ++i) {
			for (int j = 0; j < given.GetLength (1); ++j) {
				for (int k = 0; k < given.GetLength (2); ++k) {
					ret [i, j, k] = given [i, j, k];
				}
			}
		}
		return ret;
	}
}
