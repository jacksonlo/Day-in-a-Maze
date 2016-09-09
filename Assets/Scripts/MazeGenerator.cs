using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour {

	public Camera character;
	public GameObject[] blockTypes;
	public GameObject path;
	public GameObject empty;

	public int mazeX;
	public int mazeY;
	public int mazeZ;

	private enum BlockType {Metal = 0, White = 1};
	private enum MazeType {GrowingTree};

	private List<Maze> mazeList;
	private List<GameObject> mazeContainers;

	// Use this for initialization
	void Start () {
		// Generate maze
		mazeList = new List<Maze> ();
		mazeList.Add(GenerateMaze (MazeType.GrowingTree));
	}

	// Update is called once per frame
	void Update () {

	}

	// Method to Generate Maze
	private Maze GenerateMaze(MazeType mazeType) {
		switch(mazeType) {
		case MazeType.GrowingTree:
			GrowingTreeMaze maze;

			// Get starting cell for algorithm
			int x = (int)Mathf.Ceil (mazeX / 2);
			int y = (int)Mathf.Ceil (2 * mazeY / 3);
			int z = 0;

			// Growing Tree algorithm
			maze = new GrowingTreeMaze (blockTypes, new Tuple3 (mazeX, mazeY, mazeZ), new Tuple3 (x, y, z));

			// Calculate Maze
			maze.CalculateMaze ();

			// Choose an Exit
			if (!maze.ChooseExit ()) {
				Debug.Log ("Failed to choose an exit");
			}

			// Set Parent
			GameObject parent = Instantiate(empty, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
			mazeContainers.Add(parent);
			maze.SetParent(mazeContainers[mazeContainers.Count - 1]);

			// Instantiate Blocks in maze
			maze.InstantiateMaze ();

			// Generate platforms to entrance and exit
//			maze.GenerateEntrancePath (path, 10f);
//			maze.GenerateExitPath (path, 5f);
//
//			// Move Character to entrance
//			Tuple3 exitPath = maze.GetExit();
//			character.transform.position = new Vector3(exitPath.first, exitPath.second, exitPath.third);

			return maze;
			break;
		}
		return null;
	}

	#region Growing Tree Maze
	public class GrowingTreeMaze : Maze {
		// Constructor
		public GrowingTreeMaze(GameObject[] blockTypes, Tuple3 dimensions, Tuple3 bottomFarLeft) 
			: base(blockTypes, dimensions, bottomFarLeft) {}

		// Get all potential neighbours to carve to, ie. it has no neighbouring cells that are already carved
		public List<Tuple3> GetPotentialNeighbours(Tuple3 from) {
			List<Tuple3> neighbours = base.GetBlockNeighbours (from);

			// Check if neighbours are valid, ie. it has no neighbouring cells that are already carved
			for (int i = 0; i < neighbours.Count; ++i) {
				Tuple3 n = neighbours [i];
				if (base.HasSpaceNeighbours (from, n)) {
					neighbours.RemoveAt (i--);
				}
			}

			return neighbours;
		}

		// Growing tree algorithm
		public override void CalculateMaze() {
			List<Tuple3> blockList = new List<Tuple3>();
			blockList.Add (new Tuple3 (base._startingPosition.first, base._startingPosition.second, base._startingPosition.third));

			Tuple3 currentCell;
			int x, y, z;

			while (blockList.Count > 0) {
				currentCell = blockList [blockList.Count - 1];
				x = currentCell.first;
				y = currentCell.second;
				z = currentCell.third;

				// Define true as empty space, false default as block
				Carve(x, y, z);

				// Get Unvisited Valid Neighbours
				List<Tuple3> neighbours = GetPotentialNeighbours(currentCell);

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
				CarveTo(currentCell, newCell);

				// Add it to cell list
				blockList.Add (newCell);
			}	
		
		}	
			
	}
	#endregion

	#region Maze Class Definition
	public class Maze {
		protected Tuple3 _mazeDimensions;		// Maze Dimensions
		protected Tuple3 _startingPosition; 	// Coordinates for startingPosition/entrance
		protected Tuple3 _exitPosition;
		protected BlockFace _exitDirection;
		protected Block[, ,] _blockMaze;		// 3D array of actual blocks of the maze
		protected bool[, ,] _blockMap;		// 3D array of which block locations, false = block, true = empty space
		protected GameObject[] _blockTypes;
		protected GameObject _parent;

		protected enum BlockFace {Left, Right, Back, Front, Bottom, Top};

		// Constructor
		public Maze(GameObject[] blockTypes, Tuple3 dimensions, Tuple3 startingPosition) {
			_mazeDimensions = dimensions;
			_startingPosition = startingPosition;
			_exitPosition = null;
			_blockTypes = blockTypes;

			_blockMaze = new Block[_mazeDimensions.first, _mazeDimensions.second, _mazeDimensions.third];
			_blockMap = new bool[_mazeDimensions.first, _mazeDimensions.second, _mazeDimensions.third];
		}

		// Sets parent game object for use later
		public void SetParent(GameObject parent) {
			_parent = parent;
		}

		// Generates platform to entrance
		public virtual void GenerateEntrancePath(GameObject path, float length) {
			float width = _blockTypes[0].transform.lossyScale.x;
			for (int i = 0; i < length; ++i) {
				Instantiate (path, new Vector3 (_startingPosition.first * width, _startingPosition.second * width - width + 1.25f, _startingPosition.third * width - i * width - 6.5f), Quaternion.identity);
			}
		}

		// Generates platform out of exit
		public virtual void GenerateExitPath(GameObject path, float length) {
			float width = _blockTypes[0].transform.lossyScale.x;
			Quaternion rotation = Quaternion.identity;
			switch (_exitDirection) {
			case BlockFace.Left:
				rotation = Quaternion.Euler (0, 270, 0);
				for (int i = 0; i < length; ++i) {
					Instantiate (path, new Vector3 (_exitPosition.first * width - i * width - 6.5f, _exitPosition.second * width - width + 1.25f, _exitPosition.third * width + i * width - 6.5f), rotation);
				}
				break;
			case BlockFace.Right:
				rotation = Quaternion.Euler (0, 90, 0);
				for (int i = 0; i < length; ++i) {
					Instantiate (path, new Vector3 (_exitPosition.first * width + i * width - 6.5f, _exitPosition.second * width - width + 1.25f, _exitPosition.third * width), rotation);
				}
				break;
			case BlockFace.Back:
				rotation = Quaternion.identity;
				for (int i = 0; i < length; ++i) {
					Instantiate (path, new Vector3 (_exitPosition.first * width, _exitPosition.second * width - width + 1.25f, _exitPosition.third * width + i * width - 6.5f), rotation);
				}
				break;
			}
		}

		// Returns entrance coordinates
		public Tuple3 GetEntrance() {
			return _startingPosition;
		}

		// Returns exit coordinates, must be called after ChooseExit
		public Tuple3 GetExit() {
			return _exitPosition;
		}

		// Chooses an exit
		public virtual bool ChooseExit() {
			List<Tuple3> candidateExits = new List<Tuple3> ();

			// Opposite Wall
			for (int i = 0; i < _mazeDimensions.first; ++i) {
				for (int j = 0; j < _mazeDimensions.second; ++j) {
					if (_blockMap [i, j, _mazeDimensions.third - 1]) {
						candidateExits.Add (new Tuple3(i, j, _mazeDimensions.third - 1));
					}
				}
			}

			if (candidateExits.Count > 0) {
				// Pick a random exit
				_exitPosition = candidateExits [Random.Range (0, candidateExits.Count - 1)];
				_exitDirection = BlockFace.Back;
				return true;
			}

			// Left Wall
			for (int i = 0; i < _mazeDimensions.second; ++i) {
				for (int j = 0; j < _mazeDimensions.third; ++j) {
					if (_blockMap [0, i, j]) {
						candidateExits.Add (new Tuple3(0, i, j));
					}
				}
			}

			if (candidateExits.Count > 0) {
				// Pick a random exit
				_exitPosition = candidateExits [Random.Range (0, candidateExits.Count - 1)];
				_exitDirection = BlockFace.Left;
				return true;
			}

			// Right Wall
			for (int i = 0; i < _mazeDimensions.second; ++i) {
				for (int j = 0; j < _mazeDimensions.third; ++j) {
					if (_blockMap [_mazeDimensions.first - 1, i, j]) {
						candidateExits.Add (new Tuple3(_mazeDimensions.first - 1, i, j));
					}
				}
			}

			if (candidateExits.Count > 0) {
				// Pick a random exit
				_exitPosition = candidateExits [Random.Range (0, candidateExits.Count - 1)];
				_exitDirection = BlockFace.Right;
				return true;
			}

			// Bottom ??

			return false;
		}

		// Instantiate Maze Block GameObjects into the world
		public void InstantiateMaze() {
			for (int i = 0; i < _mazeDimensions.first; ++i) {
				for (int j = 0; j < _mazeDimensions.second; ++j) {
					for (int k = 0; k < _mazeDimensions.third; ++k) {
						if(!_blockMap[i, j, k]) {
							_blockMaze [i, j, k] = new Block (_blockTypes[(int)BlockType.Metal], _parent, i, j, k);
						}
					}
				}
			}
		}

		// Calculate Maze, this default just sets everything to false
		public virtual void CalculateMaze() {
			for (int i = 0; i < _mazeDimensions.first; ++i) {
				for (int j = 0; j < _mazeDimensions.second; ++j) {
					for (int k = 0; k < _mazeDimensions.third; ++k) {
						_blockMap [i, j, k] = false;
					}
				}
			}
		}

		// Marks a space on the blockMap as empty (carve)
		public bool Carve(int x, int y, int z) {
			if(x >= 0 && x < _mazeDimensions.first && y >= 0 && y < _mazeDimensions.second && z >= 0 && z < _mazeDimensions.third) {
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
			if (x > _mazeDimensions.first || y > _mazeDimensions.second || z > _mazeDimensions.third) {
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
			if (x > _mazeDimensions.first || y > _mazeDimensions.second || z > _mazeDimensions.third) {
				return false;
			}

			_blockMaze [x, y, z] = b;
			return true;
		}
		public bool PlaceBlock(Block b, Tuple3 location) {
			return PlaceBlock (b, location.first, location.second, location.third);
		}

		// Get all neighbours that are spaces
		public List<Tuple3> GetSpaceNeighbours(Tuple3 location) {
			int x = location.first;
			int y = location.second;
			int z = location.third;

			List<Tuple3> neighbours = new List<Tuple3> ();

			// Top
			if (y + 2 < _mazeDimensions.second && _blockMap [x, y + 2, z]) {
				neighbours.Add (new Tuple3(x, y + 2, z));
			}

			// Front
			if (z + 2 < _mazeDimensions.third && _blockMap [x, y, z + 2]) {
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
			if (x + 2 < _mazeDimensions.first && _blockMap [x + 2, y, z]) {
				neighbours.Add (new Tuple3(x + 2, y, z));
			}

			// Back
			if (z - 2 >= 0 && _blockMap [x, y, z - 2]) {
				neighbours.Add (new Tuple3(x, y, z - 2));
			}

			return neighbours;
		}

		// Returns true if the location has neighbours that are spaces
		public bool HasSpaceNeighbours(Tuple3 from, Tuple3 location) {
			int x = location.first;
			int y = location.second;
			int z = location.third;

			// Top
			if (y + 2 < _mazeDimensions.second && _blockMap [x, y + 2, z] && !from.Equals(new Tuple3(x, y + 2, z))) {
				return true;
			}

			// Front
			if (z + 2 < _mazeDimensions.third && _blockMap [x, y, z + 2] && !from.Equals(new Tuple3(x, y, z + 2)))  {
				return true;
			}

			// Bottom
			if (y - 2 >= 0 && _blockMap [x, y - 2, z] && !from.Equals(new Tuple3(x, y - 2, z)))  {
				return true;
			}

			// Left
			if (x - 2 >= 0 && _blockMap [x - 2, y, z] && !from.Equals(new Tuple3(x - 2, y, z)))  {
				return true;
			}

			// Right
			if (x + 2 < _mazeDimensions.first && _blockMap [x + 2, y, z] && !from.Equals(new Tuple3(x + 2, y, z)))  {
				return true;
			}

			// Back
			if (z - 2 >= 0 && _blockMap [x, y, z - 2] && !from.Equals(new Tuple3(x, y, z - 2)))  {
				return true;
			}

			return false;
		}

		// Get's all neighbours that are blocks
		public List<Tuple3> GetBlockNeighbours(Tuple3 location) {
			int x = location.first;
			int y = location.second;
			int z = location.third;

			List<Tuple3> neighbours = new List<Tuple3> ();

			// Top
			if (y + 2 < _mazeDimensions.second && !_blockMap [x, y + 2, z]) {
				neighbours.Add (new Tuple3(x, y + 2, z));
			}

			// Front
			if (z + 2 < _mazeDimensions.third && !_blockMap [x, y, z + 2]) {
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
			if (x + 2 < _mazeDimensions.first && !_blockMap [x + 2, y, z]) {
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
			_width = _blockType.transform.lossyScale.x;

			_blockType = Instantiate (_blockType, new Vector3(x * _width, y * _width, z * _width), Quaternion.identity) as GameObject;
		}

		public Block(GameObject blockType, GameObject parent, int x, int y, int z) {
			this.x = x;
			this.y = y;
			this.z = z;
			_blockType = blockType;
			_width = _blockType.transform.lossyScale.x;

			_blockType = Instantiate (_blockType, new Vector3(x * _width, y * _width, z * _width), Quaternion.identity) as GameObject;
			_blockType.transform.parent = parent.transform;
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
