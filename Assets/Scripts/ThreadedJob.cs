using System;
using System.Collections;

public class ThreadedJob {
	private bool complete = false;
	private object completeLock = new object();
	private System.Threading.Thread thread = null;

	public bool Completed {
		get {
			bool tmp;
			lock (completeLock) {
				tmp = complete;
			}
			return tmp;
		} 
		set {
			lock (completeLock) {
				complete = value;
			}
		}
	}

	public virtual void Start() {
		thread = new System.Threading.Thread(Run);
		thread.Start();
	}
	public virtual void Abort() {
		thread.Abort();
	}

	protected virtual void ThreadFunction() { }

	protected virtual void OnFinished() { }

	public virtual bool Update() {
		if (Completed) {
			OnFinished();
			return true;
		}
		return false;
	}
	public IEnumerator WaitFor() {
		while(!Update()) {
			yield return null;
		}
	}
	private void Run() {
		ThreadFunction();
		Completed = true;
	}
}