using System;

public class ShuffleJob : ThreadedJob
{
	public Maze maze;

	protected override void ThreadFunction()
	{
		// Do your threaded task. DON'T use the Unity API here
		maze.ShuffleMaze (MazeAlgorithmMode.GrowingTree);
	}
	protected override void OnFinished()
	{
		// This is executed by the Unity main thread when the job is finished
		Console.Write("Shuffle Done!");
	}	
}
