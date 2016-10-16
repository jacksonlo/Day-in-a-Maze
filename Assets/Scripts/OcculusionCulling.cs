using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class OcculusionCulling : MonoBehaviour {

	//private GameObject _maze;
	private Maze _maze;
	private bool _mazeReady;
	private bool _mazeGraphReady;
	private Tuple3<int> _currentPos;

	// Use this for initialization
	void Start () {
		_mazeReady = false;
		_mazeGraphReady = false;
		_maze = null;
		_currentPos = new Tuple3<int> (0, 0, 0);
	}
	
	// Update is called once per frame
	void Update () {
		UpdatePosition ();
		Debug.Log (_mazeReady + "-" + _mazeGraphReady);
		if (_mazeReady && _mazeGraphReady) {
			Block[, ,] blocks = _maze.GetBlockMaze();

			// Traverse graph to render via BFS
			HashSet<Node<Tuple3<int>>> visited = new HashSet<Node<Tuple3<int>>> ();
			Queue<Node<Tuple3<int>>> q = new Queue<Node<Tuple3<int>>>();

			Node<Tuple3<int>> currentNode = _maze.mazeGraph.nodes[new Node<Tuple3<int>> (_currentPos)];
			currentNode.depth = 0;
			q.Enqueue(currentNode);
			visited.Add (currentNode);

			int renderRange = Math.Max(_maze.mazeDimensions.first, Math.Max(_maze.mazeDimensions.second, _maze.mazeDimensions.third));

			while (q.Count > 0) {
				currentNode = q.Dequeue ();

				// Get surrounding blocks from currentNode and apply render status
				List<Tuple3<int>> renderThese = _maze.GetBlockNeighbours(currentNode.value);
				for (int i = 0; i < renderThese.Count; ++i) {
					blocks [renderThese [i].first, renderThese [i].second, renderThese [i].third].Render (currentNode.depth < renderRange);
				}

				for (int i = 0; i < currentNode.neighbours.Count; ++i) {
					if (!visited.Contains (currentNode.neighbours [i])) {
						currentNode.neighbours [i].depth = currentNode.depth + 1;
						q.Enqueue (currentNode.neighbours [i]);
						visited.Add (currentNode.neighbours [i]);
					}
				}
			}
		}
	}

	private int NormalizeRange(int a, int max) {
		if (a > max) {
			a = max;
		} else if (a < 0) {
			a = 0;
		}
		return a;
	}
		
	public void MazeReady(Maze maze) {
		_maze = maze;
		_mazeReady = true;
	}

	public void MazeGraphReady(Maze maze) {
		_maze = maze;
		_mazeGraphReady = true;
	}

	public void UpdatePosition() {
		if (_mazeReady) {
			// Normalize currentPos to within bounds
//			int bw = 5;
//			_currentPos.first = NormalizeRange ((int)(Camera.main.transform.position.x / bw), _maze.mazeDimensions.first);
//			_currentPos.second = NormalizeRange ((int)(Camera.main.transform.position.y / bw), _maze.mazeDimensions.second);
//			_currentPos.third = NormalizeRange ((int)(Camera.main.transform.position.z / bw), _maze.mazeDimensions.third);
		}
	}
}
