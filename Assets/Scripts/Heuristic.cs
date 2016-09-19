using System;
using System.Collections;
using System.Collections.Generic;

public enum HeuristicMode {
	Misplaced
};

public class Heuristic {

	public static int Misplaced(bool[, ,] initial, bool[, ,] target) {
		int misplaced = 0;
		for (int i = 0; i < initial.GetLength(0); ++i) {
			for (int j = 0; j < initial.GetLength(1); ++j) {
				for (int k = 0; k < initial.GetLength(2); ++k) {
					// If it's a block, check if there is no block in target
					if (!initial [i, j, k]) {
						if (target [i, j, k]) {
							++misplaced;
						} 
					}	
				}
			}
		}
		return misplaced;
	}

}
