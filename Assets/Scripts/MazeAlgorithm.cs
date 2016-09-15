using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum MazeAlgorithmMode {
	GrowingTree
};

public class MazeAlgorithm {
	
	// Maze Generation Algorithms
	public static void GrowingTree(Maze m) { 
		List<Tuple3<int> > blockList = new List<Tuple3<int> >();
		blockList.Add (new Tuple3<int> (m.startingPosition.first, m.startingPosition.second, m.startingPosition.third));

		Tuple3<int> currentCell;
		int x, y, z;

		while (blockList.Count > 0) {
			currentCell = blockList [blockList.Count - 1];
			x = currentCell.first;
			y = currentCell.second;
			z = currentCell.third;

			// Define true as empty space, false default as block
			m.Carve(x, y, z);

			// Get Unvisited Valid Neighbours
			List<Tuple3<int> > neighbours = m.GetPotentialNeighbours(currentCell);

			// If no neighbours, remove cell
			if (neighbours.Count == 0) {
				//blockList.Remove (currentCell);
				for (int i = 0; i < blockList.Count; ++i) {
					if (blockList [i].first == currentCell.first && blockList[i].second == currentCell.second && blockList[i].third == currentCell.third) {
						blockList.RemoveAt (i);
						break;
					}
				}
				continue;
			}

			// Pick a (random) neighbour
			Tuple3<int> newCell = neighbours[Random.Range(0, neighbours.Count)];

			// Carve to it
			m.CarveTo(currentCell, newCell);

			// Add it to cell list
			blockList.Add (newCell);
		}

		// Carve out full faces for nicer maze
		while(m.CarveFullFaces() > 0) {}
	}
}
