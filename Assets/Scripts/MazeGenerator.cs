﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour {

	public GameObject block;
	public int mazeX;
	public int mazeY;
	public int mazeZ;

	private int _width = 1;

	//private Maze maze;

	// Use this for initialization
	void Start () {
		GenerateMaze ();

		// Instantiate Blocks in maze
//		GameObject _block = block;
//		int _width = 1;
//		GameObject[, ,] maze = new GameObject[mazeX, mazeY, mazeZ];
//
//		for (int i = 0; i < mazeX; ++i) {
//			for (int j = 0; j < mazeY; ++j) {
//				for (int k = 0; k < mazeZ; ++k) {
//					maze[i, j, k] = Instantiate (_block, new Vector3(i * _width, j * _width, k * _width), Quaternion.identity) as GameObject;
//				}
//			}
//		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	// Method to Generate Maze
	void GenerateMaze() {
		//maze = new Maze (block, mazeX, mazeY, mazeZ);

		// Growing Tree algorithm
		bool[, ,] blockMap = new bool[mazeX, mazeY, mazeZ];
		GameObject[, ,] maze = new GameObject[mazeX, mazeY, mazeZ];

		//Initialize to false
		for (int i = 0; i < mazeX; ++i) {
			for (int j = 0; j < mazeY; ++j) {
				for (int k = 0; k < mazeZ; ++k) {
					blockMap [i, j, k] = false;
				}
			}
		}

		// Get random starting cell
		int x = Random.Range (0, mazeX);
		int y = Random.Range (0, mazeY);
		int z = Random.Range (0, mazeZ);

 		List<Tuple3> cellList = new List<Tuple3>();
		Tuple3 currentCell = new Tuple3 (x, y, z);
		cellList.Add (currentCell);

		while (true) {
			// Terminate if cellList is empty
			if (cellList.Count == 0) {
				break;
			}
				
			x = currentCell.first;
			y = currentCell.second;
			z = currentCell.third;

			// Define true as empty space, false default as block
			blockMap [x, y, z] = true;

			// Get Unvisited Neighbours
			List<Tuple3> neighbours = new List<Tuple3>();

			// Top
			if (y + 1 < mazeY && !blockMap [x, y + 1, z]) {
				neighbours.Add (new Tuple3(x, y + 1, z));
			}

			// Front
			if (z + 1 < mazeZ && !blockMap [x, y, z + 1]) {
				neighbours.Add (new Tuple3(x, y, z + 1));
			}

			// Bottom
			if (y - 1 >= 0 && !blockMap [x, y - 1, z]) {
				neighbours.Add (new Tuple3(x, y - 1, z));
			}

			// Left
			if (x - 1 >= 0 && !blockMap [x - 1, y, z]) {
				neighbours.Add (new Tuple3(x - 1, y, z));
			}

			// Right
			if (x + 1 < mazeX && !blockMap [x + 1, y, z]) {
				neighbours.Add (new Tuple3(x + 1, y, z));
			}

			// Back
			if (z - 1 >= 0 && !blockMap [x, y, z - 1]) {
				neighbours.Add (new Tuple3(x, y, z - 1));
			}

			// Bottom
			if (y - 1 >= 0 && !blockMap [x, y - 1, z]) {
				neighbours.Add (new Tuple3(x, y - 1, z));
			}

			// If no neighbours, remove cell
			if (neighbours.Count == 0) {
				//cellList.Remove (currentCell);
				for (int i = 0; i < cellList.Count; ++i) {
					if (cellList [i].first == currentCell.first && cellList[i].second == currentCell.second && cellList[i].third == currentCell.third) {
						cellList.RemoveAt (i);
						break;
					}
				}

				if (cellList.Count == 0) {
					break;
				}

				currentCell = cellList [cellList.Count - 1];
				continue;
			}

			// Pick a (random) neighbour and add to cellList
			currentCell = neighbours[Random.Range(0, neighbours.Count)];
			cellList.Add (currentCell);
		}	

		// Instantiate Blocks in maze
		for (int i = 0; i < mazeX; ++i) {
			for (int j = 0; j < mazeY; ++j) {
				for (int k = 0; k < mazeZ; ++k) {
					if(!blockMap[i, j, k]) {
						maze[i, j, k] = Instantiate (block, new Vector3(i * _width, j * _width, k * _width), Quaternion.identity) as GameObject;
					}
				}
			}
		}

	}

	// Generic Tuple3 Implementation
	public class Tuple3 {
		public int first { get; set; }
		public int second { get; set; }
		public int third { get; set; }

		public Tuple3(int a, int b, int c) {
			first = a;
			second = b;
			third = c;
		}

		public override bool Equals (object obj) {
			// Check for null values and compare run-time types.
			if (obj == null || GetType() != obj.GetType()) 
				return false;

			Tuple3 t = (Tuple3)obj;
			return (first == t.first) && (second == t.second) && (third == t.third);
		}

		public override int GetHashCode() {
			int hash = 17;
			hash = hash * 23 + first.GetHashCode();
			hash = hash * 23 + second.GetHashCode();
			hash = hash * 23 + third.GetHashCode();
			return hash;
		}
	}

	// Maze Class
	public class Maze {
		private int _x, _y, _z;
		private Block[, ,] _blocks;
		private GameObject _block;
		private float _width;

		public Maze(GameObject block, int x, int y, int z) {
			_x = x;
			_y = y;
			_z = z;
			_block = block;

			_blocks = new Block[x, y, z];
		}

		public bool createBlock(int x, int y, int z) {
			if (x > _x || y > _y || z > _z) {
				return false;
			}

			_blocks [x, y, z] = new Block (_block, x, y, z);
			return true;
		}

		public bool placeBlock(Block b, int x, int y, int z) {
			if (x > _x || y > _y || z > _z) {
				return false;
			}

			_blocks [x, y, z] = b;
			return true;
		}
	}

	public class Block {
		public int x { get; set; }
		public int y { get; set; }
		public int z { get; set; }

		private GameObject _block;
		private int _width;

		public Block(GameObject block, int x, int y, int z) {
			this.x = x;
			this.y = y;
			this.z = z;
			_block = block;
			_width = 2;

			_block = Instantiate (_block, new Vector3(x * _width, y * _width, z * _width), Quaternion.identity) as GameObject;
		}
			
	}
		
}
