﻿using UnityEngine;
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
				} else if (Input.GetKeyDown (KeyCode.Backslash)) {
					m.ShuffleMaze ();
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


}
