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

	private List<Maze> _mazeList;
	private float _rotationTime;

	private Queue<Block> _activeAnimations;
	private Queue<Block> _pendingAnimations;

	private ShuffleJob _shuffleJob;
	private List<Tuple2<Tuple3<int>>> _moves;

	// Use this for initialization
	void Start () {
		// Generate maze
		_mazeList = new List<Maze> ();
		_mazeList.Add(GenerateMaze (MazeAlgorithmMode.GrowingTree));
	}

	// Update is called once per frame
	void Update () {

		// Shuffle Maze
		if (Input.GetKeyDown (KeyCode.Backslash)) {
			_moves = _mazeList [0].ShuffleMaze (MazeAlgorithmMode.GrowingTree);
			for (int i = 0; i < _moves.Count; ++i) {
				_mazeList [0].MoveBlock (_moves [i].first, _moves [i].second);
			}
		}

		// Check if shufflejob is done, get moves and set back to null
		if (_shuffleJob != null) {
			if (_shuffleJob.Update ()) {
				_moves = _shuffleJob.moves;
				_shuffleJob = null;
			}
		}

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
			maze = new Maze (empty, blockTypes, new Tuple3<int> (mazeX, mazeY, mazeZ), new Tuple3<int> (x, y, z), 
				MazeAlgorithmMode.GrowingTree, ShuffleAlgorithmMode.ForceDirected);

			// Calculate Maze
			maze.CalculateMaze ();

			// Instantiate Blocks in maze
			maze.InstantiateMaze ();

			break;
		}
		return maze;
	}


}
