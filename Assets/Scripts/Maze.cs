using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Direction {
	Up, 
	Down, 
	Left, 
	Right, 
	Clockwise, 
	CounterClockwise
};

public class Maze {
	public delegate void AlgorithmDelegate(Maze m);	// Delegate for Maze algorithm
	public delegate List<Tuple2<Tuple3<int> > > ShuffleDelegate(Maze m, HeuristicMode hm);	// Delegate for Shuffle Algorithm

	public bool rotating { get; set; }				// Rotation status
	public float rotationTime { get; set; }			// Time spent rotating
	public float rotationSeconds { get; set; }		// Speed of rotation
	public Vector3 rotationDirection { get; set; }	// Direction of rotation
	public Quaternion initialQ { get; set; }		// Initial Rotation Quaternion
	public Quaternion targetQ { get; set; }			// Target Rotation Quaternion
	public GameObject parent { get; set; }			// Empty Parent object containing all the maze block gameobjects
	public Tuple3<int> startingPosition { get; set; } 	// Coordinates for startingPosition/entrance
	public Tuple3<int> mazeDimensions;					// Maze Dimensions

	public BlockType defaultType = BlockType.Metal32; 	// Default block type

	private Block[, ,] _blockMaze;			// 3D array of actual blocks of the maze
	private bool[, ,] _blockMap;			// 3D array of which block locations for calculations, false = block, true = empty space
	private GameObject[] _blockTypes;		// The available blocktypes
	private AlgorithmDelegate _mazeAlgorithm;		// Maze Algorithm delgate set
	private MazeAlgorithmMode _mazeAlgorithmMode;	// Maze Algorithm Mode set
	private ShuffleDelegate _shuffleAlgorithm;		// Shuffle Algorithm delegate set
	private ShuffleAlgorithmMode _shuffleAlgorithmMode; // Shuffle Algorithm Mode set

	// Constructor
	public Maze(GameObject mazeParent, GameObject[] blockTypes, Tuple3<int> dimensions, Tuple3<int> startingPosition, 
		MazeAlgorithmMode mazeAlgo = MazeAlgorithmMode.GrowingTree, ShuffleAlgorithmMode shuffleAlgo = ShuffleAlgorithmMode.AStar) {

		parent = mazeParent;
		rotating = false;
		mazeDimensions = dimensions;
		this.startingPosition = startingPosition;
		_blockTypes = blockTypes;

		_blockMaze = new Block[mazeDimensions.first, mazeDimensions.second, mazeDimensions.third];
		_blockMap = new bool[mazeDimensions.first, mazeDimensions.second, mazeDimensions.third];

		SetAlgorithm (mazeAlgo);
		SetShuffle (shuffleAlgo);
	}

	// Moves a block
	public void MoveBlock(Tuple3<int> from, Tuple3<int> to) {
		_blockMaze [from.first, from.second, from.third].Move (Util.TupleToVector(to));
	}

	// Shuffle Maze, mazeType parameter for type of shuffling algorithm
	public List<Tuple2<Tuple3<int>>> ShuffleMaze(MazeAlgorithmMode mazeAlgo = MazeAlgorithmMode.GrowingTree) {
		MazeAlgorithmMode originalAlgo = _mazeAlgorithmMode;

		// Generate a maze positioning with the mazeType
		SetAlgorithm(mazeAlgo);

		int currentNumber = 0;
		int targetNumber = 1;

		while (currentNumber != targetNumber) {
			_blockMap = new bool[mazeDimensions.first, mazeDimensions.second, mazeDimensions.third];
			CalculateMaze ();

			// Number every block in initial
			currentNumber = 0;
			for (int i = 0; i < _blockMaze.GetLength (0); ++i) {
				for (int j = 0; j < _blockMaze.GetLength (1); ++j) {
					for (int k = 0; k < _blockMaze.GetLength (2); ++k) {
						// If it's a block, check if there is no block in target
						if (_blockMaze [i, j, k] != null) {
							++currentNumber;
						}	
					}
				}
			}

			// Number every block in target
			targetNumber = 0;
			for (int i = 0; i < _blockMap.GetLength (0); ++i) {
				for (int j = 0; j < _blockMap.GetLength (1); ++j) {
					for (int k = 0; k < _blockMap.GetLength (2); ++k) {
						// If it's a block, add
						if (!_blockMap [i, j, k]) {
							++targetNumber;
						}	
					}
				}
			}
		}

//		// Reconcile block counts
//		if (currentNumber < targetNumber) {
//			// Remove random blocks from target
//
//		} else if (currentNumber > targetNumber) {
//			// Move extra blocks out of cube range
//		}

		// Set algorithm back
		SetAlgorithm (originalAlgo);

		// On another thread, A* calculate the moves to sliding puzzle into the maze
		return _shuffleAlgorithm(this, HeuristicMode.None);
	}

	// Executes the list of moves
	public void ExecuteMoves(List<Tuple2<Tuple3<int>>> moves) {
		// The list is in reverse order so execute from the end
		for (int i = 0; i < moves.Count; ++i) {
			Debug.Log (moves [i].first + " -> " + moves [i].second);
			MoveBlock (moves [i].first, moves [i].second);
			moves.RemoveAt(moves.Count -1);
		}
	}

	// Returns the block map
	public bool[, ,] GetBlockMap() {
		return this._blockMap;
	}

	// Returns the block maze
	public Block[, ,] GetBlockMaze() {
		return this._blockMaze;
	}

	// Sets the shuffle algorithm, defaults to AStar
	public void SetShuffle(ShuffleAlgorithmMode shuffleMode) {
		switch (shuffleMode) {
		case ShuffleAlgorithmMode.AStar:
			_shuffleAlgorithm = new ShuffleDelegate (ShuffleAlgorithm.AStar);
			break;
		case ShuffleAlgorithmMode.Directed:
			_shuffleAlgorithm = new ShuffleDelegate (ShuffleAlgorithm.Directed);
			break;
		case ShuffleAlgorithmMode.ForceDirected:
			_shuffleAlgorithm = new ShuffleDelegate (ShuffleAlgorithm.ForceDirected);
			break;
		}
	}

	// Sets the maze generation algorithm for use during calculatemaze
	public void SetAlgorithm(MazeAlgorithmMode mazeAlgo) {
		_mazeAlgorithmMode = mazeAlgo;
		switch (mazeAlgo) {
		case MazeAlgorithmMode.GrowingTree:
			_mazeAlgorithm = new AlgorithmDelegate (MazeAlgorithm.GrowingTree);
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
			rotationDirection = Vector3.up;
			break;
		case Direction.Right:
			rotationDirection = Vector3.down;
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
		
	// Instantiate Maze Block GameObjects into the world
	public void InstantiateMaze() {
		// Instantiate Parent
		Tuple3<float> pivotCenter = new Tuple3<float> (_blockTypes [(int)defaultType].transform.lossyScale.x * mazeDimensions.first / 2,
		_blockTypes [(int)defaultType].transform.lossyScale.y * mazeDimensions.second / 2,
		_blockTypes [(int)defaultType].transform.lossyScale.z * mazeDimensions.third / 2);
		parent = GameObject.Instantiate(parent, new Vector3(pivotCenter.first, pivotCenter.second, pivotCenter.third), Quaternion.identity) as GameObject;

		// Instantiate Blocks
		for (int i = 0; i < mazeDimensions.first; ++i) {
			for (int j = 0; j < mazeDimensions.second; ++j) {
				for (int k = 0; k < mazeDimensions.third; ++k) {
					if (!_blockMap [i, j, k]) {
						_blockMaze [i, j, k] = new Block (_blockTypes [(int)defaultType], parent, i, j, k);
					} else {
						_blockMaze [i, j, k] = null;
					}
				}
			}
		}

		// Set Block neighbours
		for (int i = 0; i < mazeDimensions.first; ++i) {
			for (int j = 0; j < mazeDimensions.second; ++j) {
				for (int k = 0; k < mazeDimensions.third; ++k) {
					if (_blockMaze [i, j, k] != null) {
						// Top
						int yCheck = j + 1 < mazeDimensions.second ? j + 1 : -1;
						_blockMaze [i, j, k].neighbours [BlockFace.Top] = yCheck != -1 ? _blockMaze [i, yCheck, k] : null;

						// Bottom
						yCheck = j - 1 >= 0 ? j - 1 : -1;
						_blockMaze [i, j, k].neighbours [BlockFace.Bottom] = yCheck != -1 ? _blockMaze [i, yCheck, k] : null;

						// Left
						int xCheck = i - 1 >= 0 ? i - 1 : -1;
						_blockMaze [i, j, k].neighbours [BlockFace.Left] = xCheck != -1 ? _blockMaze [xCheck, j, k] : null;

						// Right
						xCheck = i + 1 < mazeDimensions.first ? i + 1 : -1;
						_blockMaze [i, j, k].neighbours [BlockFace.Right] = xCheck != -1 ? _blockMaze [xCheck, j, k] : null;

						// Front
						int zCheck = k - 1 >= 0 ? k - 1 : -1;
						_blockMaze [i, j, k].neighbours [BlockFace.Front] = zCheck != -1 ? _blockMaze [i, j, zCheck] : null;

						// Back
						zCheck = k + 1 < mazeDimensions.third ? k + 1 : -1;
						_blockMaze [i, j, k].neighbours [BlockFace.Back] = zCheck != -1 ? _blockMaze [i, j, zCheck] : null;
					}
				}
			}
		}
	}

	// Calculate Maze based on set algorithm
	public void CalculateMaze() {
		_mazeAlgorithm (this);
	}

	// Marks a space on the blockMap as empty (carve)
	public bool Carve(int x, int y, int z) {
		if(x >= 0 && x < mazeDimensions.first && y >= 0 && y < mazeDimensions.second && z >= 0 && z < mazeDimensions.third) {
			_blockMap [x, y, z] = true;
			return true;
		}
		return false;
	}
	public bool Carve(Tuple3<int> location) {
		return Carve (location.first, location.second, location.third);
	}

	// Carve to a location (Since 1 block thickness between each carveable block)
	public bool CarveTo(Tuple3<int> from, Tuple3<int> to) {
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
		_blockMap [to.first, to.second, to.third] = true;
		return true;
	}

	// Create a block at the given coordinates, given a block type
	public bool CreateBlock(int blockType, int x, int y, int z) {
		if (x > mazeDimensions.first || y > mazeDimensions.second || z > mazeDimensions.third) {
			return false;
		}

		_blockMaze [x, y, z] = new Block (_blockTypes[blockType], x, y, z);
		return true;
	}
	public bool CreateBlock(int blockType, Tuple3<int> location) {
		return CreateBlock (blockType, location.first, location.second, location.third);
	}

	// Place a given block type at the given coordinates
	public bool PlaceBlock(Block b, int x, int y, int z) {
		if (x > mazeDimensions.first || y > mazeDimensions.second || z > mazeDimensions.third) {
			return false;
		}

		_blockMaze [x, y, z] = b;
		return true;
	}
	public bool PlaceBlock(Block b, Tuple3<int> location) {
		return PlaceBlock (b, location.first, location.second, location.third);
	}

	// Get all neighbours that are spaces
	public List<Tuple3<int>> GetSpaceNeighbours(Tuple3<int> location, bool[, ,] givenMap = null) {
		bool[, ,] checkMap;
		if (givenMap != null) {
			checkMap = givenMap;	
		} else {
			checkMap = _blockMap;
		}

		int x = location.first;
		int y = location.second;
		int z = location.third;

		List<Tuple3<int> > neighbours = new List<Tuple3<int> > ();

		// Top
		if (y + 2 < mazeDimensions.second && checkMap [x, y + 2, z]) {
			neighbours.Add (new Tuple3<int>(x, y + 2, z));
		}

		// Front
		if (z + 2 < mazeDimensions.third && checkMap [x, y, z + 2]) {
			neighbours.Add (new Tuple3<int>(x, y, z + 2));
		}

		// Bottom
		if (y - 2 >= 0 && checkMap [x, y - 2, z]) {
			neighbours.Add (new Tuple3<int>(x, y - 2, z));
		}

		// Left
		if (x - 2 >= 0 && checkMap [x - 2, y, z]) {
			neighbours.Add (new Tuple3<int>(x - 2, y, z));
		}

		// Right
		if (x + 2 < mazeDimensions.first && checkMap [x + 2, y, z]) {
			neighbours.Add (new Tuple3<int>(x + 2, y, z));
		}

		// Back
		if (z - 2 >= 0 && checkMap [x, y, z - 2]) {
			neighbours.Add (new Tuple3<int>(x, y, z - 2));
		}

		return neighbours;
	}
	public List<Tuple3<int>> GetSpaceNeighbours(int x, int y, int z, bool[, ,] givenMap = null) {
		return GetSpaceNeighbours (new Tuple3<int> (x, y, z), givenMap);
	}
		
	// Returns true if the location has neighbours that are spaces
	public bool HasSpaceNeighbours(Tuple3<int> from, Tuple3<int> location) {
		int x = location.first;
		int y = location.second;
		int z = location.third;

		// Top
		if (y + 2 < mazeDimensions.second && _blockMap [x, y + 2, z] && !from.Equals(new Tuple3<int>(x, y + 2, z))) {
			return true;
		}

		// Front
		if (z + 2 < mazeDimensions.third && _blockMap [x, y, z + 2] && !from.Equals(new Tuple3<int>(x, y, z + 2)))  {
			return true;
		}

		// Bottom
		if (y - 2 >= 0 && _blockMap [x, y - 2, z] && !from.Equals(new Tuple3<int>(x, y - 2, z)))  {
			return true;
		}

		// Left
		if (x - 2 >= 0 && _blockMap [x - 2, y, z] && !from.Equals(new Tuple3<int>(x - 2, y, z)))  {
			return true;
		}

		// Right
		if (x + 2 < mazeDimensions.first && _blockMap [x + 2, y, z] && !from.Equals(new Tuple3<int>(x + 2, y, z)))  {
			return true;
		}

		// Back
		if (z - 2 >= 0 && _blockMap [x, y, z - 2] && !from.Equals(new Tuple3<int>(x, y, z - 2)))  {
			return true;
		}

		return false;
	}

	// Get's all neighbours that are blocks
	public List<Tuple3<int>> GetBlockNeighbours(Tuple3<int> location, bool[, ,] givenMap = null) {
		bool[, ,] checkMap;
		if (givenMap != null) {
			checkMap = givenMap;	
		} else {
			checkMap = _blockMap;
		}

		int x = location.first;
		int y = location.second;
		int z = location.third;

		List<Tuple3<int> > neighbours = new List<Tuple3<int> > ();

		// Top
		if (y + 2 < mazeDimensions.second && !checkMap [x, y + 2, z]) {
			neighbours.Add (new Tuple3<int>(x, y + 2, z));
		}

		// Front
		if (z + 2 < mazeDimensions.third && !checkMap [x, y, z + 2]) {
			neighbours.Add (new Tuple3<int>(x, y, z + 2));
		}

		// Bottom
		if (y - 2 >= 0 && !checkMap [x, y - 2, z]) {
			neighbours.Add (new Tuple3<int>(x, y - 2, z));
		}

		// Left
		if (x - 2 >= 0 && !checkMap [x - 2, y, z]) {
			neighbours.Add (new Tuple3<int>(x - 2, y, z));
		}

		// Right
		if (x + 2 < mazeDimensions.first && !checkMap [x + 2, y, z]) {
			neighbours.Add (new Tuple3<int>(x + 2, y, z));
		}

		// Back
		if (z - 2 >= 0 && !checkMap [x, y, z - 2]) {
			neighbours.Add (new Tuple3<int>(x, y, z - 2));
		}

		return neighbours;
	}
	public List<Tuple3<int>> GetBlockNeighbours(int x, int y, int z, bool[, ,] givenMap = null) {
		return GetBlockNeighbours (new Tuple3<int> (x, y, z), givenMap);
	}

	// Get all potential neighbours to carve to, ie. it has no neighbouring cells that are already carved
	public List<Tuple3<int>> GetPotentialNeighbours(Tuple3<int> from) {
		List<Tuple3<int> > neighbours = GetBlockNeighbours (from);

		// Check if neighbours are valid, ie. it has no neighbouring cells that are already carved
		for (int i = 0; i < neighbours.Count; ++i) {
			Tuple3<int> n = neighbours [i];
			if (HasSpaceNeighbours (from, n)) {
				neighbours.RemoveAt (i--);
			}
		}

		return neighbours;
	}

	// Check's if a block exists at the given coordinates, 0 for no block, 1 for block, -1 for out of range
	public bool HasBlock(int x, int y, int z) {
		// Check for in range
		if (x < 0 || x >= mazeDimensions.first || y < 0 || y >= mazeDimensions.second || z < 0 || z >= mazeDimensions.third) {
			return false;
		}

		if (_blockMap [x, y, z]) {
			return false;
		} else {
			return true;
		}
	}
	public bool HasBlock(Tuple3<int> t) {
		return HasBlock (t.first, t.second, t.third);
	}
		
}
