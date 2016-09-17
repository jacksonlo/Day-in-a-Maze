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
			bool[, ,] qMap = leaves [qKey] [0];
			leaves [qKey].RemoveAt (0);

			// If that key# is empty, remove
			if (leaves [qKey].Count == 0) {
				leaves.Remove (qKey);
			}

			// Generate q's children
			for (int x = 0; x < qMap.Length; ++x) {
				for (int y = 0; y < qMap.Length; ++y) {
					for (int z = 0; z < qMap.Length; ++z) {
						// If it's a block, check for spaces around it
						if (!qMap [x, y, z]) {
							List<Tuple3<int> > children = m.GetSpaceNeighbours (x, y, z, BlockMap.Custom, qMap);

							// Attach children to parent treenode and add to leaves while calculating their heuristic value

						}
					}
				}
			}

		}

		return null;
	}
}

