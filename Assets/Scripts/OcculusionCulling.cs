using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class OcculusionCulling : MonoBehaviour {

	//private GameObject _maze;
	private Maze _maze;
	private bool _mazeReady;
	private int _blockWidth;

	// Use this for initialization
	void Start () {
		_mazeReady = false;
		_blockWidth = 5;
	}
	
	// Update is called once per frame
	void Update () {
		if (_mazeReady) {
			// Render/Unrender each block depending on of it is in view of the camera by a margin
			Block[, ,] blocks = _maze.GetBlockMaze();

			// Get surrounding blocks to render
			HashSet<Tuple3<int>> renderThese = new HashSet<Tuple3<int>>();
		
			// (x-1, y, ALL z), (x, y, ALL z), (x+1, y, ALL z), (x, y+1, ALL z), (x, y-1, ALL z)
			// (ALL x, y-1, z), (ALL x, y, z), (ALL x, y+1, z), (ALL x, y, z-1), (ALL x, y, z+1)
			// (x-1, ALL y, z), (x, ALL y, z), (x+1, ALL y, z), (x, ALL y, z-1), (x, ALL y, z+1)

			int x = (int)Math.Round(transform.position.x / _blockWidth, MidpointRounding.ToEven);
			int y = (int)Math.Round(transform.position.y / _blockWidth, MidpointRounding.ToEven);
			int z;
			for (z = 0; z < _maze.mazeDimensions.third; ++z) {
				// Left
				if (x - 1 > 0 && blocks [x - 1, y, z] != null) {
					renderThese.Add (new Tuple3<int> (x - 1, y, z));
				}

				// Middle
				if (blocks [x, y, z] != null) {
					renderThese.Add (new Tuple3<int> (x, y, z));
				}

				// Right
				if (x + 1 < _maze.mazeDimensions.first && blocks [x + 1, y, z] != null) {
					renderThese.Add (new Tuple3<int> (x + 1, y, z));
				}

				// Top 
				if (y + 1 < _maze.mazeDimensions.second && blocks [x, y + 1, z] != null) {
					renderThese.Add (new Tuple3<int> (x, y + 1, z));
				}

				// Bottom
				if (y - 1 > 0 && blocks [x, y + 1, z] != null) {
					renderThese.Add (new Tuple3<int> (x, y + 1, z));
				}
			}


			y = (int)Math.Round(transform.position.y / _blockWidth, MidpointRounding.ToEven);
			z = (int)Math.Round(transform.position.z / _blockWidth, MidpointRounding.ToEven);
			for (x = 0; x < _maze.mazeDimensions.first; ++x) {
				// Left
				if (y - 1 > 0 && blocks [x, y - 1, z] != null) {
					renderThese.Add (new Tuple3<int> (x, y - 1, z));
				}

				// Middle
				if (blocks [x, y, z] != null) {
					renderThese.Add (new Tuple3<int> (x, y, z));
				}

				// Right
				if (y + 1 < _maze.mazeDimensions.second && blocks [x, y + 1, z] != null) {
					renderThese.Add (new Tuple3<int> (x, y + 1, z));
				}

				// Top 
				if (z + 1 < _maze.mazeDimensions.third && blocks [x, y, z + 1] != null) {
					renderThese.Add (new Tuple3<int> (x, y, z + 1));
				}

				// Bottom
				if (z - 1 > 0 && blocks [x, y, z - 1] != null) {
					renderThese.Add (new Tuple3<int> (x, y, z - 1));
				}
			}

			x = (int)Math.Round(transform.position.x / _blockWidth, MidpointRounding.ToEven);
			z = (int)Math.Round(transform.position.z / _blockWidth, MidpointRounding.ToEven);
			for (y = 0; y < _maze.mazeDimensions.second; ++y) {
				// Left
				if (x - 1 > 0 && blocks [x - 1, y, z] != null) {
					renderThese.Add (new Tuple3<int> (x - 1, y, z));
				}

				// Middle
				if (blocks [x, y, z] != null) {
					renderThese.Add (new Tuple3<int> (x, y, z));
				}

				// Right
				if (x + 1 < _maze.mazeDimensions.second && blocks [x + 1, y, z] != null) {
					renderThese.Add (new Tuple3<int> (x + 1, y, z));
				}

				// Top 
				if (z + 1 < _maze.mazeDimensions.third && blocks [x, y, z + 1] != null) {
					renderThese.Add (new Tuple3<int> (x, y, z + 1));
				}

				// Bottom
				if (z - 1 > 0 && blocks [x, y, z - 1] != null) {
					renderThese.Add (new Tuple3<int> (x, y, z - 1));
				}
			}
		
			// Render/Unrender blocks
			for (int i = 0; i < blocks.GetLength (0); ++i) {
				for (int j = 0; j < blocks.GetLength (1); ++j) {
					for (int k = 0; k < blocks.GetLength (2); ++k) {
						if (blocks [i, j, k] != null) {
							blocks [i, j, k].Render (renderThese.Contains (new Tuple3<int> (i, j, k)));
						}
					}
				}
			}
		}
	}

	public void MazeReady(Maze maze) {
		_maze = maze;
		_mazeReady = true;
	}
}
