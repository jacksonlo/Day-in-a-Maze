using System;
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
		case HeuristicMode.MisplacedManhattan:
			heuristic = new HeuristicDelegate (Heuristic.MisplacedManhattan);
			break;
		}
			
		// Set up current and target maps
		Block[, ,] currentMaze = m.GetBlockMaze ();
		bool[, ,] currentMap = new bool[currentMaze.GetLength (0), currentMaze.GetLength (1), currentMaze.GetLength (2)];
		bool[, ,] targetMap = m.GetBlockMap ();

		for (int i = 0; i < currentMap.GetLength(0); ++i) {
			for (int j = 0; j < currentMap.GetLength(1); ++j) {
				for (int k = 0; k < currentMap.GetLength(2); ++k) {
					// If it's a block, check if there is no block in target
					if (currentMaze [i, j, k] == null) {
						currentMap [i, j, k] = true;
					}	
				}
			}
		}

		// Set up leaves datastruct
		SortedDictionary<int, List<TreeNode<bool[, ,]> > > leaves = new SortedDictionary<int, List<TreeNode<bool[, ,]> > >();
		leaves.Add (heuristic (currentMap, targetMap), new List<TreeNode<bool[, ,]> > {new TreeNode<bool[, ,]> (currentMap)});

		// Perform A*
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

			// Check if goal is reached
			if (Util.MazeMapEquals(node.value, targetMap)) {
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
							List<Tuple3<int> > children = m.GetSpaceNeighbours (x, y, z, node.value);

							// Attach parent to children treenode and add to leaves while calculating their heuristic value
							for (int i = 0; i < children.Count; ++i) {
								// Get childmap by swapping the block with the space
								bool[, ,] childMap = Util.CopyMap(node.value);
								childMap [x, y, z] = true;
								childMap [children [i].first, children [i].second, children [i].third] = false;

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
			Console.Write(childrenCount);
		}
		return null;
	}

	// Directed movement
	public static List<Tuple2<Tuple3<int> > > Directed(Maze m, HeuristicMode hm) {
		// Number each block in the current state and map it in the bool[, ,]
		Block[, ,] originalMap = m.GetBlockMaze();
		Dictionary<int, Tuple3<int>> currentBlockMap = new Dictionary<int, Tuple3<int>> ();
		bool[, ,] currentMap = new bool[originalMap.GetLength (0), originalMap.GetLength (1), originalMap.GetLength (2)];

		int currentNumber = 0;
		for (int i = 0; i < originalMap.GetLength (0); ++i) {
			for (int j = 0; j < originalMap.GetLength (1); ++j) {
				for (int k = 0; k < originalMap.GetLength (2); ++k) {
					if (originalMap [i, j, k] != null) {
						currentBlockMap.Add (currentNumber++, new Tuple3<int> (i, j, k));
					} else {
						currentMap [i, j, k] = true;
					}
				}
			}
		}

		// Number each block in the target state
		bool[, ,] targetMap = m.GetBlockMap();
		Dictionary<int, Tuple3<int>> targetBlockMap = new Dictionary<int, Tuple3<int>> ();
		int targetNumber = 0;
		for (int i = 0; i < targetMap.GetLength (0); ++i) {
			for (int j = 0; j < targetMap.GetLength (1); ++j) {
				for (int k = 0; k < targetMap.GetLength (2); ++k) {
					if (!targetMap [i, j, k]) {
						targetBlockMap.Add(targetNumber++, new Tuple3<int>(i, j, k));
					}
				}
			}
		}

		if (currentNumber != targetNumber) {
			Console.Write ("Error Block Mismatch! Current Blocks: " + currentNumber + " Target Blocks: " + targetNumber);
			return null;
		}

		// Move each block to it's target space
		List<Tuple2<Tuple3<int>>> moves = new List<Tuple2<Tuple3<int>>>();
		for (int i = 0; i < currentNumber; ++i) {
			//Tuple3<int> inital = currentBlockMap [i];
			//Tuple3<int> target = targetBlockMap [i];
		}


		return moves;
	}
}

