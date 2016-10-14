using System;
using System.Collections.Generic;

public class ShuffleJob : ThreadedJob
{
	public Maze maze;

	protected override void ThreadFunction()
	{
		maze.ShuffleMaze (MazeAlgorithmMode.GrowingTree);
	}
	protected override void OnFinished()
	{
		Console.Write("Shuffle Calculations Done!");
	}	
}
