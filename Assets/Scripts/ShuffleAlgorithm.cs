using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ShuffleAlgorithmMode {
	AStar
};

public class ShuffleAlgorithm {

	public delegate float HeuristicDelegate (int a, int b);

	// Returns a sequence of block movements
	public static List<Tuple2<Tuple3<int> > > AStar(Maze m, HeuristicMode hm) { 
		HeuristicDelegate heuristic;
		switch (hm) {
		case HeuristicMode.Misplaced:
			heuristic = new HeuristicDelegate (Heuristic.Misplaced);
			break;
		}
			
		SortedDictionary<int, List<TreeNode<bool[, ,]> > > leaves = new SortedDictionary<int, List<TreeNode<bool[, ,]> > >();

		leaves.Add (heuristic(m.GetBlockMapOld(), m.GetBlockMap()), new TreeNode<bool[, ,]>(m.GetBlockMapOld ()));

		bool done = false;
		while (leaves.Count > 0) {
			// Pop lowest key off leaves, q
			int qKey = leaves.Keys [0];
			TreeNode<bool[, ,]> node = leaves [qKey] [0];
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
			for (int x = 0; x < node.value.Length; ++x) {
				for (int y = 0; y < node.value.Length; ++y) {
					for (int z = 0; z < node.value.Length; ++z) {
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
								int heuristicValue = heuristic (node.value, child.value) + child.level;
								if (leaves.ContainsKey (heuristicValue)) {
									leaves [heuristicValue].Add (child);
								} else {
									leaves.Add (heuristicValue, new List<TreeNode<bool[, ,]> > (child));
								}
							}
						}
					}
				}
			}
		}
		return null;
	}
}

