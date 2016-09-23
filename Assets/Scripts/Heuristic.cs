using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HeuristicMode {
	Misplaced,
	MisplacedManhattan
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

	public static int MisplacedManhattan(bool[, ,] initial, bool[, ,] target) {
		// Number every block in initial
		int currentNumber = 0;
		Dictionary<int, Tuple3<int>> initialNumbering = new Dictionary<int, Tuple3<int>> ();
		for (int i = 0; i < initial.GetLength(0); ++i) {
			for (int j = 0; j < initial.GetLength(1); ++j) {
				for (int k = 0; k < initial.GetLength(2); ++k) {
					// If it's a block, check if there is no block in target
					if (!initial [i, j, k]) {
						initialNumbering.Add(currentNumber++, new Tuple3<int>(i, j, k));
					}	
				}
			}
		}

		// Number every block in target
		int targetNumber = 0;
		Dictionary<int, Tuple3<int>> targetNumbering = new Dictionary<int, Tuple3<int>> ();
		for (int i = 0; i < target.GetLength(0); ++i) {
			for (int j = 0; j < target.GetLength(1); ++j) {
				for (int k = 0; k < target.GetLength(2); ++k) {
					// If it's a block, check if there is no block in target
					if (!target [i, j, k]) {
						targetNumbering.Add(targetNumber++, new Tuple3<int>(i, j, k));
					}	
				}
			}
		}

		if (currentNumber != targetNumber) {
			Debug.Log ("Error Block Mismatch! Current Blocks: " + currentNumber + " Target Blocks: " + targetNumber);
			return -1;
		}

		// Get manhattan distance for each numbered block
		int cost = 0;
		for (int i = 0; i < currentNumber; ++i) {
			Tuple3<int> manhattan = initialNumbering [i] - targetNumbering [i];
			cost += Math.Abs (manhattan.first) + Math.Abs (manhattan.second) + Math.Abs (manhattan.third);
		}

		return cost;
	}

}
