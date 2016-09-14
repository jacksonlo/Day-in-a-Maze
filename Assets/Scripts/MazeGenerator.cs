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
		Maze maze = null;
		switch(mazeGenType) {
		case MazeAlgorithmMode.GrowingTree:
			// Get starting cell for algorithm
			int x = (int)Mathf.Ceil (mazeX / 2);
			int y = (int)Mathf.Ceil (2 * mazeY / 3);
			int z = 0;

			// Growing Tree algorithm
			maze = new Maze (empty, blockTypes, new Tuple3<int> (mazeX, mazeY, mazeZ), new Tuple3<int> (x, y, z), MazeAlgorithmMode.GrowingTree);

			// Calculate Maze
			maze.CalculateMaze ();

			// Choose an Exit
			if (!maze.ChooseExit ()) {
				Debug.Log ("Failed to choose an exit");
			}

			// Instantiate Blocks in maze
			maze.InstantiateMaze ();

			// Generate platforms to entrance and exit
//			maze.GenerateEntrancePath (path, 10f);
//			maze.GenerateExitPath (path, 5f);
//
//			// Move Character to entrance
//			Tuple3<int> exitPath = maze.GetExit();
//			character.transform.position = new Vector3(exitPath.first, exitPath.second, exitPath.third);

			break;
		}
		return maze;
	}

	#region Maze Algorithm Class Definition
	private class MazeAlgorithm {
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
		public Tuple3<int> startingPosition { get; set; } 	// Coordinates for startingPosition/entrance
		public Tuple3<int> exitPosition { get; set; }		// Coordinates for exitPosition
		public Tuple3<int> mazeDimensions;					// Maze Dimensions

		protected BlockFace _exitDirection;		// The side of the cube that the exit protrudes from
		protected Block[, ,] _blockMaze;		// 3D array of actual blocks of the maze
		protected bool[, ,] _blockMap;			// 3D array of which block locations, false = block, true = empty space
		protected GameObject[] _blockTypes;		// The blocktypes

		private AlgorithmDelegate mazeAlgorithm;		// Maze Algorithm set

		// Constructor
		public Maze(GameObject mazeParent, GameObject[] blockTypes, Tuple3<int> dimensions, Tuple3<int> startingPosition, MazeAlgorithmMode mazeAlgo = MazeAlgorithmMode.GrowingTree) {
			parent = mazeParent;
			rotating = false;
			mazeDimensions = dimensions;
			this.startingPosition = startingPosition;
			this.exitPosition = null;
			_blockTypes = blockTypes;

			_blockMaze = new Block[mazeDimensions.first, mazeDimensions.second, mazeDimensions.third];
			_blockMap = new bool[mazeDimensions.first, mazeDimensions.second, mazeDimensions.third];

			SetAlgorithm(mazeAlgo);
		}

		// Shuffle Maze, mazeType parameter for type of shuffling algorithm
		public void ShuffleMaze(MazeAlgorithmMode mazeAlgo) {
			// Generate a maze positioning with the mazeType

			// On another thread, A* calculate the moves to sliding puzzle into the maze

			// On another thread execute those moves in sequence
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
		public Tuple3<int> GetEntrance() {
			return startingPosition;
		}

		// Returns exit coordinates, must be called after ChooseExit
		public Tuple3<int> GetExit() {
			return exitPosition;
		}

		// Chooses an exit
		public virtual bool ChooseExit() {
			List<Tuple3<int> > candidateExits = new List<Tuple3<int> > ();

			// Opposite Wall
			for (int i = 0; i < mazeDimensions.first; ++i) {
				for (int j = 0; j < mazeDimensions.second; ++j) {
					if (_blockMap [i, j, mazeDimensions.third - 1]) {
						candidateExits.Add (new Tuple3<int> (i, j, mazeDimensions.third - 1));
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
			for (int i = 0; i < mazeDimensions.second; ++i) {
				for (int j = 0; j < mazeDimensions.third; ++j) {
					if (_blockMap [0, i, j]) {
						candidateExits.Add (new Tuple3<int>(0, i, j));
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
			for (int i = 0; i < mazeDimensions.second; ++i) {
				for (int j = 0; j < mazeDimensions.third; ++j) {
					if (_blockMap [mazeDimensions.first - 1, i, j]) {
						candidateExits.Add (new Tuple3<int>(mazeDimensions.first - 1, i, j));
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
			// Instantiate Parent
			Tuple3<float> pivotCenter = new Tuple3<float> (_blockTypes [(int)BlockType.Metal].transform.lossyScale.x * mazeDimensions.first / 2,
				_blockTypes [(int)BlockType.Metal].transform.lossyScale.y * mazeDimensions.second / 2,
				_blockTypes [(int)BlockType.Metal].transform.lossyScale.z * mazeDimensions.third / 2);
			parent = Instantiate(parent, new Vector3(pivotCenter.first, pivotCenter.second, pivotCenter.third), Quaternion.identity) as GameObject;


			for (int i = 0; i < mazeDimensions.first; ++i) {
				for (int j = 0; j < mazeDimensions.second; ++j) {
					for (int k = 0; k < mazeDimensions.third; ++k) {
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
		public List<Tuple3<int> > GetSpaceNeighbours(Tuple3<int> location) {
			int x = location.first;
			int y = location.second;
			int z = location.third;

			List<Tuple3<int> > neighbours = new List<Tuple3<int> > ();

			// Top
			if (y + 2 < mazeDimensions.second && _blockMap [x, y + 2, z]) {
				neighbours.Add (new Tuple3<int>(x, y + 2, z));
			}

			// Front
			if (z + 2 < mazeDimensions.third && _blockMap [x, y, z + 2]) {
				neighbours.Add (new Tuple3<int>(x, y, z + 2));
			}

			// Bottom
			if (y - 2 >= 0 && _blockMap [x, y - 2, z]) {
				neighbours.Add (new Tuple3<int>(x, y - 2, z));
			}

			// Left
			if (x - 2 >= 0 && _blockMap [x - 2, y, z]) {
				neighbours.Add (new Tuple3<int>(x - 2, y, z));
			}

			// Right
			if (x + 2 < mazeDimensions.first && _blockMap [x + 2, y, z]) {
				neighbours.Add (new Tuple3<int>(x + 2, y, z));
			}

			// Back
			if (z - 2 >= 0 && _blockMap [x, y, z - 2]) {
				neighbours.Add (new Tuple3<int>(x, y, z - 2));
			}

			return neighbours;
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
		public List<Tuple3<int> > GetBlockNeighbours(Tuple3<int> location) {
			int x = location.first;
			int y = location.second;
			int z = location.third;

			List<Tuple3<int> > neighbours = new List<Tuple3<int> > ();

			// Top
			if (y + 2 < mazeDimensions.second && !_blockMap [x, y + 2, z]) {
				neighbours.Add (new Tuple3<int>(x, y + 2, z));
			}

			// Front
			if (z + 2 < mazeDimensions.third && !_blockMap [x, y, z + 2]) {
				neighbours.Add (new Tuple3<int>(x, y, z + 2));
			}

			// Bottom
			if (y - 2 >= 0 && !_blockMap [x, y - 2, z]) {
				neighbours.Add (new Tuple3<int>(x, y - 2, z));
			}

			// Left
			if (x - 2 >= 0 && !_blockMap [x - 2, y, z]) {
				neighbours.Add (new Tuple3<int>(x - 2, y, z));
			}

			// Right
			if (x + 2 < mazeDimensions.first && !_blockMap [x + 2, y, z]) {
				neighbours.Add (new Tuple3<int>(x + 2, y, z));
			}

			// Back
			if (z - 2 >= 0 && !_blockMap [x, y, z - 2]) {
				neighbours.Add (new Tuple3<int>(x, y, z - 2));
			}

			return neighbours;
		}

		// Get all potential neighbours to carve to, ie. it has no neighbouring cells that are already carved
		public List<Tuple3<int> > GetPotentialNeighbours(Tuple3<int> from) {
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

		// Cuts faces that have not been carved up to the minHoles, returns an int of how many faces were carved
		public int CarveFullFaces(int minHoles = 0) {
			int carvedFaces = 0;

			// Check each side to carve
			int firstD = 0;
			int secondD = 0;
			foreach (BlockFace face in System.Enum.GetValues(typeof(BlockFace))) {
				bool exit = false;

				switch (face) {
				case BlockFace.Left:
				case BlockFace.Right:
					firstD = mazeDimensions.second;
					secondD = mazeDimensions.third;
					break;
				case BlockFace.Top:
				case BlockFace.Bottom:
					firstD = mazeDimensions.first;
					secondD = mazeDimensions.third;
					break;
				case BlockFace.Back:
				case BlockFace.Front:
					firstD = mazeDimensions.first;
					secondD = mazeDimensions.second;
					break;
				}

				firstD--;
				secondD--;

				int holeCount = 0;
				for (int i = 0; i <= firstD; ++i) {
					for (int j = 0; j <= secondD; ++j) {
						Tuple3<int> coord = null;

						switch (face) {
						case BlockFace.Left:
							coord = new Tuple3<int> (0, firstD, secondD);
							break;
						case BlockFace.Right:
							coord = new Tuple3<int> (mazeDimensions.first - 1, firstD, secondD);
							break;
						case BlockFace.Top:
							coord = new Tuple3<int> (firstD, mazeDimensions.second - 1, secondD);
							break;
						case BlockFace.Bottom:
							coord = new Tuple3<int> (firstD, 0, secondD);
							break;
						case BlockFace.Back:
							coord = new Tuple3<int> (firstD, secondD, mazeDimensions.third - 1);
							break;
						case BlockFace.Front:
							coord = new Tuple3<int> (firstD, secondD, 0);
							break;
						}

						if (!HasBlock (coord)) {
							holeCount++;
							if (holeCount > minHoles) {
								exit = true;
								break;
							}
						}
					}
					if (exit) {
						break;
					}
				}

				// Carve face
				if (!exit) {
					carvedFaces++;
					for (int i = 0; i < firstD; ++i) {
						for (int j = 0; j < secondD; ++j) {
							Tuple3<int> coord = null;

							switch (face) {
							case BlockFace.Left:
								coord = new Tuple3<int> (0, firstD, secondD);
								break;
							case BlockFace.Right:
								coord = new Tuple3<int> (mazeDimensions.first - 1, firstD, secondD);
								break;
							case BlockFace.Top:
								coord = new Tuple3<int> (firstD, mazeDimensions.second - 1, secondD);
								break;
							case BlockFace.Bottom:
								coord = new Tuple3<int> (firstD, 0, secondD);
								break;
							case BlockFace.Back:
								coord = new Tuple3<int> (firstD, secondD, mazeDimensions.third - 1);
								break;
							case BlockFace.Front:
								coord = new Tuple3<int> (firstD, secondD, 0);
								break;
							}
							Carve (coord);
						}
					}
				}
			}
			return carvedFaces;
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
	public class Tuple3<T> {
		public T first { get; set; }
		public T second { get; set; }
		public T third { get; set; }

		public Tuple3(T a, T b, T c) {
			first = a;
			second = b;
			third = c;
		}

		public override bool Equals (object obj) {
			// Check for null values and compare run-time types.
			if (obj == null || GetType() != obj.GetType()) 
				return false;

			Tuple3<T> t = (Tuple3<T>)obj;
			return EqualityComparer<T>.Default.Equals(first, t.first) && EqualityComparer<T>.Default.Equals(second, t.second) && EqualityComparer<T>.Default.Equals(third, t.third);
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
	public class Tuple2<T> {
		public T first { get; set; }
		public T second { get; set; }

		public Tuple2(T a, T b) {
			first = a;
			second = b;
		}

		public override bool Equals(object obj) {
			if (obj == null || GetType() != obj.GetType()) 
				return false;

			Tuple2<T> t = (Tuple2<T>)obj;
			return EqualityComparer<T>.Default.Equals(first, t.first) && EqualityComparer<T>.Default.Equals(second, t.second);
		}

		public override int GetHashCode() {
			//return first ^ second;
			int hash = 17;
			hash = hash * 31 + first.GetHashCode();
			hash = hash * 31 + second.GetHashCode();
			return hash;
		}
	}
	#endregion

}
