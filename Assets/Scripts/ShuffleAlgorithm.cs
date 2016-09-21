﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum ShuffleAlgorithmMode {
	AStar,
	Directed
};

public class ShuffleAlgorithm {

	public delegate int HeuristicDelegate (bool[, ,] a, bool[, ,] b);

	// Returns a sequence of block movements
	public static List<Tuple2<Tuple3<int> > > AStar(Maze m, HeuristicMode hm) { 
		HeuristicDelegate heuristic = null;
		switch (hm) {
		case HeuristicMode.Misplaced:
			heuristic = new HeuristicDelegate (Heuristic.Misplaced);
			break;
		}
			
		SortedDictionary<int, List<TreeNode<bool[, ,]> > > leaves = new SortedDictionary<int, List<TreeNode<bool[, ,]> > >();
		bool[, ,] targetMap = m.GetBlockMap ();
		leaves.Add (heuristic (m.GetBlockMapOld (), targetMap), new List<TreeNode<bool[, ,]> > {new TreeNode<bool[, ,]> (m.GetBlockMapOld ())});

		while (leaves.Count > 0) {
			// Pop lowest key off leaves, q
			KeyValuePair<int, List<TreeNode<bool[, ,]> > > kvp = leaves.First();
			int qKey = kvp.Key;
			TreeNode<bool[, ,]> node = kvp.Value[0];
			leaves [qKey].RemoveAt (0);

			// If that key# is empty, remove
			if (leaves [qKey].Count == 0) {
				leaves.Remove (qKey);
			}

			// Check if heuristic is 0
			if (qKey == 0) {
				// Generate path of moves and return
				List<Tuple2<Tuple3<int> > > moves = new List<Tuple2<Tuple3<int> > >();

				while (node.parent != null) {
					Tuple3<int> from = node.changeFrom;
					Tuple3<int> to = node.changeTo;
					Tuple2<Tuple3<int> > move = new Tuple2<Tuple3<int> > (from, to);
					moves.Add (move);
					node = node.parent;
				}
				return moves;
			}

			// Generate q's children
			int childrenCount = 0;
			for (int x = 0; x < node.value.GetLength(0); ++x) {
				for (int y = 0; y < node.value.GetLength(1); ++y) {
					for (int z = 0; z < node.value.GetLength(2); ++z) {
						// If it's a block, check for spaces around it
						if (!node.value [x, y, z]) {
							List<Tuple3<int> > children = m.GetSpaceNeighbours (x, y, z, BlockMap.Custom, node.value);

							// Attach parent to children treenode and add to leaves while calculating their heuristic value
							for (int i = 0; i < children.Count; ++i) {
								// Get childmap by swapping the block with the space
								bool[, ,] childMap = node.value;
								node.value [x, y, z] = true;
								node.value [children [i].first, children [i].second, children [i].third] = false;

								// Attach parent to children and add to leaves with heuristic
								Tuple3<int> changeFrom = new Tuple3<int>(x, y, z);
								Tuple3<int> changeTo = new Tuple3<int>(children [i].first, children [i].second, children [i].third);
								TreeNode<bool[, ,]> child = new TreeNode<bool[, ,]> (childMap, node, changeFrom, changeTo);
								int heuristicValue = heuristic (child.value, targetMap) + child.level;
								if (leaves.ContainsKey (heuristicValue)) {
									leaves [heuristicValue].Add (child);
								} else {
									leaves.Add (heuristicValue, new List<TreeNode<bool[, ,]> > {child});
								}
								++childrenCount;
							}
						}
					}
				}
			}
			Debug.Log(childrenCount);
		}
		return null;
	}

	// Directed movement
	public static List<Tuple2<Tuple3<int> > > Directed(Maze m, HeuristicMode hm) {
		// Number each block in the original state
		bool[, ,] originalMap = m.GetBlockMapOld();
		Dictionary<int, Tuple3<int>> numberBlockMap = new Dictionary<int, Tuple3<int>> ();
		int number = 0;
		for (int i = 0; i < originalMap.GetLength (0); ++i) {
			for (int j = 0; j < originalMap.GetLength (1); ++j) {
				for (int k = 0; k < originalMap.GetLength (2); ++k) {
					if (!originalMap [i, j, k]) {
						numberBlockMap.Add(number++, new Tuple3<int>(i, j, k));
					}
				}
			}
		}

		// Number each block in the target state
		bool[, ,] targetMap = m.GetBlockMap();
		Dictionary<int, Tuple3<int>> numberBlockMap2 = new Dictionary<int, Tuple3<int>> ();
		number = 0;
		for (int i = 0; i < targetMap.GetLength (0); ++i) {
			for (int j = 0; j < targetMap.GetLength (1); ++j) {
				for (int k = 0; k < targetMap.GetLength (2); ++k) {
					if (!targetMap [i, j, k]) {
						numberBlockMap2.Add(number++, new Tuple3<int>(i, j, k));
					}
				}
			}
		}

		// Move each block to it's target space
		List<Tuple2<Tuple3<int>>> moves = new List<Tuple2<Tuple3<int>>>();
		for (int i = 0; i < number; ++i) {
			Tuple3<int> inital = numberBlockMap [i];
			Tuple3<int> target = numberBlockMap2 [i];

			// Do a maze search algorithm on the old map, while checking for "new" roadblocks in the current state

		}

		return moves;
	}
}

