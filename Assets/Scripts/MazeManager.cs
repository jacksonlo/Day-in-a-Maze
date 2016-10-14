using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MazeManager : MonoBehaviour {

	public Camera character;
	public GameObject[] blockTypes;
	public GameObject path;
	public GameObject empty;

	public int mazeX;
	public int mazeY;
	public int mazeZ;

	private List<Maze> _mazeList;
	private float _rotationTime;

	private List<Block> _activeBlocks;
	private Stack<Block> _pendingBlocks;

	private bool _shuffle;

	// Use this for initialization
	void Start () {
		// Generate maze
		_mazeList = new List<Maze> ();
		_mazeList.Add(GenerateMaze (MazeAlgorithmMode.GrowingTree));

		_shuffle = false;
		_activeBlocks = new List<Block> ();
		_pendingBlocks = new Stack<Block> ();
	}

	// Update is called once per frame
	void Update () {

		// Shuffle Maze
		if (Input.GetKeyDown (KeyCode.Backslash)) {
			_mazeList [0].ShuffleMaze (MazeAlgorithmMode.GrowingTree);
			_shuffle = true;
		}

		if (Input.GetKeyDown (KeyCode.Z)) {
			_mazeList [0].Heartbeat ();
		}

		// Place trigger ready blocks into action queues
		if (_shuffle) {
			List<Block> readyBlocks = _mazeList [0].GetTriggerReadyBlocks ();
			for(int i = readyBlocks.Count - 1; i >= 0; --i) {
				_pendingBlocks.Push (readyBlocks [i]);
			}
			_shuffle = false;
		}

		// Action Queues
		if (_activeBlocks.Count > 0) {
			for (int i = 0; i < _activeBlocks.Count; ++i) {
				if (!_activeBlocks [i].IsMoving ()) {
					_activeBlocks [i].Move ();

					// Bump target spot's block to front of queue if exists
					Vector3 target = _activeBlocks[i]._bb.GetTarget();
					if (_mazeList [0].HasBlock((int)target.x, (int)target.y, (int)target.z)) {
						_pendingBlocks.Push (_mazeList [0].GetBlock (Util.VectorToTuple (target)));
					}

				} else if(!_activeBlocks[i].OffAnchor()) {
					_activeBlocks.RemoveAt (i--);
				}
			}
		} else if(_pendingBlocks.Count > 0) {
			_activeBlocks.Add(_pendingBlocks.Pop ());
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
