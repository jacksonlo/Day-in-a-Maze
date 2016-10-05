using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class Util {

	public static bool[, ,] CopyMap(bool[, ,] given) {
		bool[, ,] ret = new bool[given.GetLength (0), given.GetLength (1), given.GetLength (2)];
		for (int i = 0; i < given.GetLength (0); ++i) {
			for (int j = 0; j < given.GetLength (1); ++j) {
				for (int k = 0; k < given.GetLength (2); ++k) {
					ret [i, j, k] = given [i, j, k];
				}
			}
		}
		return ret;
	}

	public static bool MazeMapEquals(bool[, ,] a, bool[, ,] b) {
		if (a.GetLength (0) != b.GetLength (0) || a.GetLength (1) != b.GetLength (1) || a.GetLength (2) != b.GetLength (2)) {
			return false;
		}

		for (int i = 0; i < a.GetLength (0); ++i) {
			for (int j = 0; j < a.GetLength (1); ++j) {
				for (int k = 0; k < a.GetLength (2); ++k) {
					if (a [i, j, k] != b [i, j, k]) {
						return false;
					}
				}
			}
		}
		return true;
	}

	public static bool Move(Tuple3<int> from, Tuple3<int> to, bool[, ,] map) {
		if (map [from.first, from.second, from.third] != true || map [to.first, to.second, to.third] != false) {
			Console.Write ("Error in boolean map moving");
			return false;
		}

		map [from.first, from.second, from.third] = true;
		map [to.first, to.second, to.third] = false;
		return true;
	}

	public static List<Tuple2<Tuple3<int>>> MakeSpace(Tuple3<int> from, Tuple3<int> to, Maze m, bool[, ,] map) {
		List<Tuple2<Tuple3<int>>> moves = new List<Tuple2<Tuple3<int>>> ();

		// Get space neighbours of offending block
		List<Tuple3<int>> spaceNeighbours = m.GetSpaceNeighbours(to, map);

		if (spaceNeighbours.Count > 0) {
			// Move offending block into space
			Util.Move (to, spaceNeighbours [0], map);
			moves.Add (new Tuple2<Tuple3<int>> (to, spaceNeighbours [0]));
			return moves;
		} else {
			// Recurse through block neighbours to get least amount of moves
			List<Tuple2<Tuple3<int>>> possibleMoves;
			int min = -1;
			List<Tuple3<int>> blockNeighbours = m.GetBlockNeighbours (to, map);

			for (int i = 0; i < blockNeighbours.Count; ++i) {
				possibleMoves = MakeSpace (to, blockNeighbours [i], m, map);
				if (possibleMoves.Count < min || min == -1) {
					min = possibleMoves.Count;
					moves = possibleMoves;
				}
			}

			return moves;
		}
	}

	public static List<Tuple2<Tuple3<int>>> ReverseMoves(List<Tuple2<Tuple3<int>>> moves) {
		List<Tuple2<Tuple3<int>>> reversed = new List<Tuple2<Tuple3<int>>> ();
		for (int i = moves.Count - 1; i >= 0; --i) {
			reversed.Add (new Tuple2<Tuple3<int>> (moves[i].second, moves[i].first));
		}
		return reversed;
	}

	public static bool CheckBeforeTarget(Vector3 m, Vector3 a, Vector3 b) {
		// Check which direction of vector
		if (m.x != 0) {
			return m.x > 0 ? a.x <= b.x : a.x >= b.x;
		} else if (m.y != 0) {
			return m.y > 0 ? a.y <= b.y : a.y >= b.y;
		} else {
			return m.z > 0 ? a.z <= b.z : a.z >= b.z;
		}
	}

	public static Vector3 GetBlockFaceVector(BlockFace bf, Transform transform) {
		Vector3 move = transform.up;
		switch (bf) {
		case BlockFace.Top:
			move = transform.up;
			break;
		case BlockFace.Bottom:
			move = -transform.up;
			break;
		case BlockFace.Left:
			move = -transform.right;
			break;
		case BlockFace.Right:
			move = transform.right;
			break;
		case BlockFace.Front:
			move = transform.forward;
			break;
		case BlockFace.Back:
			move = -transform.forward;
			break;
		}
		return move;
	}

	public static BlockFace RandomBlockFaceExcept(BlockFace bf) {
		System.Random rand = new System.Random ();
		BlockFace returnFace = bf;

		while(returnFace != bf) {
			switch (rand.Next (0, 6)) {
			case 0:
				returnFace = BlockFace.Top;
				break;
			case 1:
				returnFace = BlockFace.Bottom;
				break;
			case 2:
				returnFace = BlockFace.Left;
				break;
			case 3:
				returnFace = BlockFace.Right;
				break;
			case 4:
				returnFace = BlockFace.Front;
				break;
			case 5: 
				returnFace = BlockFace.Back;
				break;
			}
		}

		return returnFace;
	}
}
