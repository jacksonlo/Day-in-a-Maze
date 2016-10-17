using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.T)) {
			Debug.Log ("Restarting");
			RestartCurrentScene ();
		} else if (Input.GetKeyDown(KeyCode.X)) {
			Debug.Log ("Gravity Change!");
			Physics.gravity = new Vector3(0, 1.0F, 0);
		}
	}

	public void RestartCurrentScene() {
		Scene scene = SceneManager.GetActiveScene();
		SceneManager.LoadScene(scene.name);
	}
}
