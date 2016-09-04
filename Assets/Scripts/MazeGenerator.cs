using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour {

	public GameObject block;
	public int mazeX;
	public int mazeY;
	public int mazeZ;

	private int _width = 2;

	//private Maze maze;

	// Use this for initialization
	void Start () {
		GenerateMaze ();
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
//		int x = Random.Range (0, mazeX);
//		int y = Random.Range (0, mazeY);
//		int z = Random.Range (0, mazeZ);

		// Choose bottom back left as starting cell
		int x = 0;
		int y = 0;
		int z = 0;

		List<Tuple3> blockList = new List<Tuple3>();
		blockList.Add (new Tuple3 (x, y, z));
		Tuple3 currentCell;

		while (blockList.Count > 0) {
			currentCell = blockList [blockList.Count - 1];
			x = currentCell.first;
			y = currentCell.second;
			z = currentCell.third;

			// Define true as empty space, false default as block
			blockMap [x, y, z] = true;

			// Get Unvisited Valid Neighbours
			List<Tuple3> neighbours = GetPotentialNeighbours (blockMap, x, y, z);

			// Check if neighbours are valid, ie. it has no neighbouring cells that are already carved
			for (int i = 0; i < neighbours.Count; ++i) {
				Tuple3 n = neighbours [i];
				List<Tuple3> nn = GetAllNeighbours (blockMap, n.first, n.second, n.third);
				if (nn.Count != 1 || nn [0] == n) {
					neighbours.RemoveAt (i);
					i -= 1;
				}
			}

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
			currentCell = neighbours[Random.Range(0, neighbours.Count)];

			// Carve to it
			int xdiff = currentCell.first - x;
			int ydiff = currentCell.second - y;
			int zdiff = currentCell.third - z;

			if (xdiff != 0) {
				if (xdiff > 0) { //To the right
					blockMap[x + 1, y, z] = true;
				} else { //To the left
					blockMap[x - 1, y, z] = true;
				}
			} else if (ydiff != 0) {
				if (ydiff > 0) { //Up
					blockMap[x, y + 1, z] = true;
				} else { //Down
					blockMap [x, y - 1, z] = true;
				}
			} else { //if (zdiff != 0) {
				if (zdiff > 0) { //Towards screen
					blockMap[x, y, z + 1] = true;
				} else { //Away 
					blockMap[x, y, z - 1] = true;
				}
			}

			// Add it to cell list
			blockList.Add (currentCell);
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

	public List<Tuple3> GetPotentialNeighbours(bool[, ,] blockMap, int x, int y, int z) {
		List<Tuple3> neighbours = new List<Tuple3> ();

		// Top
		if (y + 2 < mazeY && !blockMap [x, y + 2, z]) {
			neighbours.Add (new Tuple3(x, y + 2, z));
		}

		// Front
		if (z + 2 < mazeZ && !blockMap [x, y, z + 2]) {
			neighbours.Add (new Tuple3(x, y, z + 2));
		}

		// Bottom
		if (y - 2 >= 0 && !blockMap [x, y - 2, z]) {
			neighbours.Add (new Tuple3(x, y - 2, z));
		}

		// Left
		if (x - 2 >= 0 && !blockMap [x - 2, y, z]) {
			neighbours.Add (new Tuple3(x - 2, y, z));
		}

		// Right
		if (x + 2 < mazeX && !blockMap [x + 2, y, z]) {
			neighbours.Add (new Tuple3(x + 2, y, z));
		}

		// Back
		if (z - 2 >= 0 && !blockMap [x, y, z - 2]) {
			neighbours.Add (new Tuple3(x, y, z - 2));
		}

		return neighbours;
	}

	public List<Tuple3> GetAllNeighbours(bool[, ,] blockMap, int x, int y, int z) {
		List<Tuple3> neighbours = new List<Tuple3> ();

		// Top
		if (y + 2 < mazeY && blockMap [x, y + 2, z]) {
			neighbours.Add (new Tuple3(x, y + 2, z));
		}

		// Front
		if (z + 2 < mazeZ && blockMap [x, y, z + 2]) {
			neighbours.Add (new Tuple3(x, y, z + 2));
		}

		// Bottom
		if (y - 2 >= 0 && blockMap [x, y - 2, z]) {
			neighbours.Add (new Tuple3(x, y - 2, z));
		}

		// Left
		if (x - 2 >= 0 && blockMap [x - 2, y, z]) {
			neighbours.Add (new Tuple3(x - 2, y, z));
		}

		// Right
		if (x + 2 < mazeX && blockMap [x + 2, y, z]) {
			neighbours.Add (new Tuple3(x + 2, y, z));
		}

		// Back
		if (z - 2 >= 0 && blockMap [x, y, z - 2]) {
			neighbours.Add (new Tuple3(x, y, z - 2));
		}

		return neighbours;
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

	// Generic Tuple2 Implemention
	public class Tuple2 {
		public int first { get; set; }
		public int second { get; set; }

		public Tuple2(int a, int b) {
			first = a;
			second = b;
		}

		public override bool Equals(object obj) {
			if (obj == null || GetType() != obj.GetType()) 
				return false;

			Tuple2 t = (Tuple2)obj;
			return (first == t.first) && (second == t.second);
		}

		public override int GetHashCode() {
			return first ^ second;
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
