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

	public static bool MazeMapEquals(bool[, ,] a, bool[, ,] b) {
		if (a.GetLength (0) != b.GetLength (0) || a.GetLength (1) != b.GetLength (1) || a.GetLength (2) != b.GetLength (2)) {
			return false;
		}

		for (int i = 0; i < a.GetLength (0); ++i) {
			for (int j = 0; j < a.GetLength (1); ++j) {
				for (int k = 0; k < a.GetLength (2); ++k) {
					if (a [i, j, k] != b [i, j, k]) {
						return false;
					}
				}
			}
		}
		return true;
	}
}
