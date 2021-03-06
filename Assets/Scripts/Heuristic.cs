﻿using System;
using System.Collections;
using System.Collections.Generic;

public enum HeuristicMode {
	Misplaced,
	MisplacedManhattan,
	None
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
		// Can refactor this to use 1 loop and 0 storage

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

		Console.Write ("Current Blocks: " + currentNumber + " Target Blocks: " + targetNumber);

		// Get manhattan distance for each numbered block
		int cost = 0;
		int c;
		for (c = 0; c < Math.Min(currentNumber, targetNumber); ++c) {
			cost += Math.Abs (initialNumbering [c].first - targetNumbering[c].first);
			cost += Math.Abs (initialNumbering [c].second - targetNumbering[c].second);
			cost += Math.Abs (initialNumbering [c].third - targetNumbering[c].third);
		}

		// Add extra blocks cost to get outside of the cube
		for (; c < Math.Max (currentNumber, targetNumber); ++c) {
			if (currentNumber > targetNumber) {
				cost += Math.Min (initialNumbering [c].first, Math.Min (initialNumbering [c].second, initialNumbering [c].third));
			} else {
				cost += Math.Min (targetNumbering [c].first, Math.Min (targetNumbering [c].second, targetNumbering [c].third));
			}
		}

		return cost;
	}

}
