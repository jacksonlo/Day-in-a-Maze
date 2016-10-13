using System;
using System.Collections.Generic;

public class ShuffleJob : ThreadedJob
{
	public Maze maze;
	public List<Tuple2<Tuple3<int>>> moves;

	protected override void ThreadFunction()
	{
		moves = maze.ShuffleMaze (MazeAlgorithmMode.GrowingTree);
	}
	protected override void OnFinished()
	{
		Console.Write("Shuffle Calculations Done!");
	}	
}
