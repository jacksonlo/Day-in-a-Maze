using System;
using System.Collections;
using System.Collections.Generic;

public enum HeuristicMode {
	Misplaced
};

public class Heuristic {

	public static int Misplaced(bool[, ,] initial, bool[, ,] target) {
		int misplaced = 0;
		for (int i = 0; i < initial.Length; ++i) {
			for (int j = 0; j < initial.Length; ++j) {
				for (int k = 0; k < initial.Length; ++k) {
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
