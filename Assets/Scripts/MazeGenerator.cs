using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour {

	public GameObject[] blockTypes;
	public int mazeX;
	public int mazeY;
	public int mazeZ;

	private float _width;

	enum BlockType {Metal, White};

	private GrowingTreeMaze maze;

	// Use this for initialization
	void Start () {
		_width = blockTypes[0].transform.lossyScale.x;
		GenerateMaze ();
	}

	// Update is called once per frame
	void Update () {

	}

	// Method to Generate Maze
	void GenerateMaze() {
		// Get random starting cell
		int x = Random.Range (0, mazeX);
		int y = Random.Range (0, mazeY);
		int z = Random.Range (0, mazeZ);

		// Growing Tree algorithm
		maze = new GrowingTreeMaze (blockTypes, new Tuple3(mazeX, mazeY, mazeZ), new Tuple3(x, y, z));

		List<Tuple3> blockList = new List<Tuple3>();
		blockList.Add (new Tuple3 (x, y, z));
		Tuple3 currentCell;

		while (blockList.Count > 0) {
			currentCell = blockList [blockList.Count - 1];
			x = currentCell.first;
			y = currentCell.second;
			z = currentCell.third;

			// Define true as empty space, false default as block
			maze.Carve(x, y, z);

			// Get Unvisited Valid Neighbours
			List<Tuple3> neighbours = maze.GetPotentialNeighbours(x, y, z);

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
			Tuple3 newCell = neighbours[Random.Range(0, neighbours.Count)];

			// Carve to it
			maze.CarveTo(currentCell, newCell);

			// Add it to cell list
			blockList.Add (newCell);
		}	

		// Instantiate Blocks in maze
		maze.InstantiateMaze();
	}

	#region Growing Tree Maze
	public class GrowingTreeMaze : Maze {
		public List<Tuple3> GetPotentialNeighbours(int x, int y, int z) {
			List<Tuple3> neighbours = base.GetBlockNeighbours (x, y, z);

			// Check if neighbours are valid, ie. it has no neighbouring cells that are already carved
			for (int i = 0; i < neighbours.Count; ++i) {
				Tuple3 n = neighbours [i];
				if (base.HasSpaceNeighbours (n.first, n.second, n.third)) {
					neighbours.RemoveAt (i--);
				}
			}

			return neighbours;
		}	
	}
	#endregion

	#region Maze Class Definition
	public class Maze {
		private Tuple3 _mazeDimensions;		// Maze Dimensions
		private Tuple3 _startingPosition; 	// Coordinates for bottom far left of the cube maze
		private Block[, ,] _blockMaze;		// 3D array of actual blocks of the maze
		private bool[, ,] _blockMap;		// 3D array of which block locations, false = block, true = empty space
		private GameObject[] _blockTypes;
		private float _width;

		// Constructor
		public Maze(GameObject[] blockTypes, Tuple3 dimensions, Tuple3 bottomFarLeft) {
			_mazeDimensions = dimensions;
			_startingPosition = bottomFarLeft;
			_blockTypes = blockTypes;

			_blockMaze = new Block[_mazeDimensions.first, _mazeDimensions.second, _mazeDimensions.third];
		}

		// Instantiate Maze Block GameObjects into the world
		public void InstantiateMaze() {
			for (int i = 0; i < mazeX; ++i) {
				for (int j = 0; j < mazeY; ++j) {
					for (int k = 0; k < mazeZ; ++k) {
						if(!_blockMap[i, j, k]) {
							_blockMaze[i, j, k] = Instantiate (_blockTypes[BlockType.Metal], new Vector3(i * _width, j * _width, k * _width), Quaternion.identity) as GameObject;
						}
					}
				}
			}
		}

		// Mark's a space on the blockMap as empty (carve)
		public bool Carve(int x, int y, int z) {
			if(x >= 0 && x < _mazeX && y >= 0 && y < _mazeY && z >= 0 && z < _mazeZ) {
				_blockMap [x, y, z] = true;
				return true;
			}
			return false;
		}
		public bool Carve(Tuple3 location) {
			return Carve (location.first, location.second, location.third);
		}

		// Carve to a location (Since 1 block thickness between each carveable block)
		public bool CarveTo(Tuple3 from, Tuple3 to) {
			int xdiff = to.first - from.first;
			int ydiff = to.second - from.second;
			int zdiff = to.third - from.third;

			if (xdiff != 0) {
				if (xdiff > 0) { //To the right
					_blockMap[from.first + 1, from.second, from.third] = true;
				} else { //To the left
					_blockMap[from.first - 1, from.second, from.third] = true;
				}
			} else if (ydiff != 0) {
				if (ydiff > 0) { //Up
					_blockMap[from.first, from.second + 1, from.third] = true;
				} else { //Down
					_blockMap [from.first, from.second - 1, from.third] = true;
				}
			} else { //if (zdiff != 0) {
				if (zdiff > 0) { //Towards screen
					_blockMap[from.first, from.second, from.third + 1] = true;
				} else { //Away 
					_blockMap[from.first, from.second, from.third - 1] = true;
				}
			}
			return true;
		}

		// Create a block at the given coordinates, given a block type
		public bool CreateBlock(int blockType, int x, int y, int z) {
			if (x > _mazeX || y > _mazeY || z > _mazeZ) {
				return false;
			}

			_blockMaze [x, y, z] = new Block (_blockTypes[blockType], x, y, z);
			return true;
		}
		public bool CreateBlock(int blockType, Tuple3 location) {
			return CreateBlock (blockType, location.first, location.second, location.third);
		}

		// Place a given block type at the given coordinates
		public bool PlaceBlock(Block b, int x, int y, int z) {
			if (x > _mazeX || y > _mazeY || z > _mazeZ) {
				return false;
			}

			_blockMaze [x, y, z] = b;
			return true;
		}

		// Get all neighbours that are spaces
		public List<Tuple3> GetSpaceNeighbours(int x, int y, int z) {
			List<Tuple3> neighbours = new List<Tuple3> ();

			// Top
			if (y + 2 < mazeY && _blockMap [x, y + 2, z]) {
				neighbours.Add (new Tuple3(x, y + 2, z));
			}

			// Front
			if (z + 2 < mazeZ && _blockMap [x, y, z + 2]) {
				neighbours.Add (new Tuple3(x, y, z + 2));
			}

			// Bottom
			if (y - 2 >= 0 && _blockMap [x, y - 2, z]) {
				neighbours.Add (new Tuple3(x, y - 2, z));
			}

			// Left
			if (x - 2 >= 0 && _blockMap [x - 2, y, z]) {
				neighbours.Add (new Tuple3(x - 2, y, z));
			}

			// Right
			if (x + 2 < mazeX && _blockMap [x + 2, y, z]) {
				neighbours.Add (new Tuple3(x + 2, y, z));
			}

			// Back
			if (z - 2 >= 0 && _blockMap [x, y, z - 2]) {
				neighbours.Add (new Tuple3(x, y, z - 2));
			}

			return neighbours;
		}

		// Returns true if the location has neighbours that are spaces
		public bool HasSpaceNeighbours(int x, int y, int z) {
			// Top
			if (y + 2 < mazeY && _blockMap [x, y + 2, z]) {
				return true;
			}

			// Front
			if (z + 2 < mazeZ && _blockMap [x, y, z + 2]) {
				return true;
			}

			// Bottom
			if (y - 2 >= 0 && _blockMap [x, y - 2, z]) {
				return true;
			}

			// Left
			if (x - 2 >= 0 && _blockMap [x - 2, y, z]) {
				return true;
			}

			// Right
			if (x + 2 < mazeX && _blockMap [x + 2, y, z]) {
				return true;
			}

			// Back
			if (z - 2 >= 0 && _blockMap [x, y, z - 2]) {
				return true;
			}

			return false;
		}

		// Get's all neighbours that are blocks
		public List<Tuple3> GetBlockNeighbours(int x, int y, int z) {
			List<Tuple3> neighbours = new List<Tuple3> ();

			// Top
			if (y + 2 < mazeY && !_blockMap [x, y + 2, z]) {
				neighbours.Add (new Tuple3(x, y + 2, z));
			}

			// Front
			if (z + 2 < mazeZ && !_blockMap [x, y, z + 2]) {
				neighbours.Add (new Tuple3(x, y, z + 2));
			}

			// Bottom
			if (y - 2 >= 0 && !_blockMap [x, y - 2, z]) {
				neighbours.Add (new Tuple3(x, y - 2, z));
			}

			// Left
			if (x - 2 >= 0 && !_blockMap [x - 2, y, z]) {
				neighbours.Add (new Tuple3(x - 2, y, z));
			}

			// Right
			if (x + 2 < mazeX && !_blockMap [x + 2, y, z]) {
				neighbours.Add (new Tuple3(x + 2, y, z));
			}

			// Back
			if (z - 2 >= 0 && !_blockMap [x, y, z - 2]) {
				neighbours.Add (new Tuple3(x, y, z - 2));
			}

			return neighbours;
		}
			
	}
	#endregion

	#region Block Class Definition
	public class Block {
		public int x { get; set; }
		public int y { get; set; }
		public int z { get; set; }

		private GameObject _blockType;
		private float _width;

		public Block(GameObject blockType, int x, int y, int z) {
			this.x = x;
			this.y = y;
			this.z = z;
			_blockType = blockType;
			_width = blockType.transform.lossyScale.x;

			_blockType = Instantiate (_blockType, new Vector3(x * _width, y * _width, z * _width), Quaternion.identity) as GameObject;
		}

	}
	#endregion

	#region Tuple3 Definition
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
	#endregion

	#region Tuple2 Definition
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
	#endregion

}
