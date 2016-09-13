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
	private enum MazeAlgorithmMode {GrowingTree};
	private enum Direction {Up, Down, Left, Right, Clockwise, CounterClockwise};
	private enum BlockFace {Left, Right, Back, Front, Bottom, Top};

	private List<Maze> _mazeList;
	private float _rotationTime;

	// Use this for initialization
	void Start () {
		// Generate maze
		_mazeList = new List<Maze> ();
		_mazeList.Add(GenerateMaze (MazeAlgorithmMode.GrowingTree));
	}

	// Update is called once per frame
	void Update () {

		// Check for rotations and apply them
		foreach (Maze m in _mazeList) {
			if (m.rotating) {
				m.ApplyRotation ();
			} else {
				if (Input.GetKeyDown (KeyCode.UpArrow)) {
					m.Rotate (Direction.Up);
				} else if (Input.GetKeyDown (KeyCode.DownArrow)) {
					m.Rotate (Direction.Down);
				} else if (Input.GetKeyDown (KeyCode.LeftArrow)) {
					m.Rotate (Direction.Left);
				} else if (Input.GetKeyDown (KeyCode.RightArrow)) {
					m.Rotate (Direction.Right);
				} else if (Input.GetKeyDown (KeyCode.Comma)) {
					m.Rotate (Direction.Clockwise);
				} else if (Input.GetKeyDown (KeyCode.Period)) {
					m.Rotate (Direction.CounterClockwise);
				}
			}
		}
	}

	// Method to Generate Maze
	private Maze GenerateMaze(MazeAlgorithmMode mazeGenType) {
		switch(mazeGenType) {
		case MazeAlgorithmMode.GrowingTree:
			Maze maze;

			// Get starting cell for algorithm
			int x = (int)Mathf.Ceil (mazeX / 2);
			int y = (int)Mathf.Ceil (2 * mazeY / 3);
			int z = 0;

			// Growing Tree algorithm
			maze = new Maze (blockTypes, new Tuple3 (mazeX, mazeY, mazeZ), new Tuple3 (x, y, z), MazeAlgorithmMode.GrowingTree);

			// Calculate Maze
			maze.CalculateMaze ();

			// Choose an Exit
			if (!maze.ChooseExit ()) {
				Debug.Log ("Failed to choose an exit");
			}

			// Set Parent
			maze.parent = Instantiate(empty, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;

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

	// Shuffle Maze, mazeType parameter for type of shuffling algorithm
//	private Maze ShuffleMaze(Maze maze, MazeType mazeType) {
//		// Generate a maze positioning with the mazeType
//
//		// On another thread, A* calculate the moves to sliding puzzle into the maze
//
//		// On another thread execute those moves in sequence
//	}


	#region Maze Algorithm Class Definition
	private class MazeAlgorithm {
		public static void GrowingTree(Maze m) { 
			List<Tuple3> blockList = new List<Tuple3>();
			blockList.Add (new Tuple3 (m.startingPosition.first, m.startingPosition.second, m.startingPosition.third));

			Tuple3 currentCell;
			int x, y, z;

			while (blockList.Count > 0) {
				currentCell = blockList [blockList.Count - 1];
				x = currentCell.first;
				y = currentCell.second;
				z = currentCell.third;

				// Define true as empty space, false default as block
				m.Carve(x, y, z);

				// Get Unvisited Valid Neighbours
				List<Tuple3> neighbours = m.GetPotentialNeighbours(currentCell);

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
				m.CarveTo(currentCell, newCell);

				// Add it to cell list
				blockList.Add (newCell);

			}	
		}
	}
	#endregion

	#region Maze Class Definition
	private class Maze {
		public delegate void AlgorithmDelegate(Maze m);	// Delegate for Maze algorithm

		public bool rotating { get; set; }				// Rotation status
		public float rotationTime { get; set; }			// Time spent rotating
		public float rotationSeconds { get; set; }		// Speed of rotation
		public Vector3 rotationDirection { get; set; }	// Direction of rotation
		public Quaternion initialQ { get; set; }		// Initial Rotation Quaternion
		public Quaternion targetQ { get; set; }			// Target Rotation Quaternion
		public GameObject parent { get; set; }			// Empty Parent object containing all the maze block gameobjects
		public Tuple3 startingPosition { get; set; } 	// Coordinates for startingPosition/entrance
		public Tuple3 exitPosition { get; set; }		// Coordinates for exitPosition

		protected Tuple3 _mazeDimensions;		// Maze Dimensions
		protected BlockFace _exitDirection;		// The side of the cube that the exit protrudes from
		protected Block[, ,] _blockMaze;		// 3D array of actual blocks of the maze
		protected bool[, ,] _blockMap;			// 3D array of which block locations, false = block, true = empty space
		protected GameObject[] _blockTypes;		// The blocktypes

		private AlgorithmDelegate mazeAlgorithm;		// Maze Algorithm set

		// Constructor
		public Maze(GameObject[] blockTypes, Tuple3 dimensions, Tuple3 startingPosition, MazeAlgorithmMode mazeAlgo = MazeAlgorithmMode.GrowingTree) {
			rotating = false;
			_mazeDimensions = dimensions;
			this.startingPosition = startingPosition;
			this.exitPosition = null;
			_blockTypes = blockTypes;

			_blockMaze = new Block[_mazeDimensions.first, _mazeDimensions.second, _mazeDimensions.third];
			_blockMap = new bool[_mazeDimensions.first, _mazeDimensions.second, _mazeDimensions.third];

			SetAlgorithm(mazeAlgo);
		}

		// Sets the maze generation algorithm for use during calculatemaze
		public void SetAlgorithm(MazeAlgorithmMode mazeAlgo) {
			switch (mazeAlgo) {
			case MazeAlgorithmMode.GrowingTree:
				mazeAlgorithm = new AlgorithmDelegate (MazeAlgorithm.GrowingTree);
				break;
			}
		}

		// Sets the rotate variables for the maze via the parent object
		public void Rotate(Direction d, float seconds = 1f) {
			rotating = true;	
			rotationTime = 0;
			rotationSeconds = seconds;
			initialQ = parent.transform.rotation;

			// Map intuitive directions to the appropriate axis rotations
			switch (d) {
			case Direction.Up:
				rotationDirection = Vector3.right;
				break;
			case Direction.Down:
				rotationDirection = Vector3.left;
				break;
			case Direction.Left:
				rotationDirection = Vector3.down;
				break;
			case Direction.Right:
				rotationDirection = Vector3.up;
				break;
			case Direction.Clockwise:
				rotationDirection = Vector3.forward;
				break;
			case Direction.CounterClockwise:
				rotationDirection = Vector3.back;
				break;
			}

			Vector3 correctedAxis = Quaternion.Inverse(initialQ) * rotationDirection;
			Quaternion axisRotation = Quaternion.AngleAxis(90, correctedAxis);
			targetQ = initialQ * axisRotation;
		}

		// Applies the rotation to the maze
		public bool ApplyRotation() {
			if (rotating) {
				rotationTime += Time.deltaTime / rotationSeconds;

				parent.transform.rotation = Quaternion.Lerp (initialQ, targetQ, rotationTime);

				// Check if rotation is done
				if (parent.transform.rotation == targetQ) {
					rotating = false;
				}
				return true;
			} else {
				return false;
			}
		}

		// Generates platform to entrance
		public virtual void GenerateEntrancePath(GameObject path, float length) {
			float width = _blockTypes[0].transform.lossyScale.x;
			for (int i = 0; i < length; ++i) {
				Instantiate (path, new Vector3 (startingPosition.first * width, startingPosition.second * width - width + 1.25f, startingPosition.third * width - i * width - 6.5f), Quaternion.identity);
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
					Instantiate (path, new Vector3 (exitPosition.first * width - i * width - 6.5f, exitPosition.second * width - width + 1.25f, exitPosition.third * width + i * width - 6.5f), rotation);
				}
				break;
			case BlockFace.Right:
				rotation = Quaternion.Euler (0, 90, 0);
				for (int i = 0; i < length; ++i) {
					Instantiate (path, new Vector3 (exitPosition.first * width + i * width - 6.5f, exitPosition.second * width - width + 1.25f, exitPosition.third * width), rotation);
				}
				break;
			case BlockFace.Back:
				rotation = Quaternion.identity;
				for (int i = 0; i < length; ++i) {
					Instantiate (path, new Vector3 (exitPosition.first * width, exitPosition.second * width - width + 1.25f, exitPosition.third * width + i * width - 6.5f), rotation);
				}
				break;
			}
		}

		// Returns entrance coordinates
		public Tuple3 GetEntrance() {
			return startingPosition;
		}

		// Returns exit coordinates, must be called after ChooseExit
		public Tuple3 GetExit() {
			return exitPosition;
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
				exitPosition = candidateExits [Random.Range (0, candidateExits.Count - 1)];
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
				exitPosition = candidateExits [Random.Range (0, candidateExits.Count - 1)];
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
				exitPosition = candidateExits [Random.Range (0, candidateExits.Count - 1)];
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
							_blockMaze [i, j, k] = new Block (_blockTypes[(int)BlockType.Metal], parent, i, j, k);
						}
					}
				}
			}
		}

		// Calculate Maze based on set algorithm
		public void CalculateMaze() {
			mazeAlgorithm (this);
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

		// Get all potential neighbours to carve to, ie. it has no neighbouring cells that are already carved
		public List<Tuple3> GetPotentialNeighbours(Tuple3 from) {
			List<Tuple3> neighbours = GetBlockNeighbours (from);

			// Check if neighbours are valid, ie. it has no neighbouring cells that are already carved
			for (int i = 0; i < neighbours.Count; ++i) {
				Tuple3 n = neighbours [i];
				if (HasSpaceNeighbours (from, n)) {
					neighbours.RemoveAt (i--);
				}
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
