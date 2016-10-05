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
			if (_shuffleJob == null) {
				_shuffleJob = new ShuffleJob ();
				_shuffleJob.maze = _mazeList [0];
				_shuffleJob.Start ();
				return;
			} else {
				_shuffleJob.Abort ();
			}
		}

		// Check if shufflejob is done and set back to null
		if (_shuffleJob != null) {
			if (_shuffleJob.Update ()) {
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

		// Handle shuffle translations
		if (Input.GetKeyDown (KeyCode.Z)) {
			//test
			Block testBlock = (_mazeList[0].GetBlockMaze())[5, 0, 8];
			testBlock.Move (new Vector3 (8f, 0f, 8f));
			//Rigidbody body = (_mazeList[0].GetBlockMaze())[5, 0, 8].blockObject.GetComponent<Rigidbody>();
			//body.AddForce(5.0f, 0f, 0f, ForceMode.Impulse);
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
