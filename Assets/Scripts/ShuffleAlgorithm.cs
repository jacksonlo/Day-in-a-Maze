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
		case HeuristicMode.Manhattan:
			heuristic = new HeuristicDelegate (Heuristic.Manhattan);
			break;
		}

		return null;
	}
}

