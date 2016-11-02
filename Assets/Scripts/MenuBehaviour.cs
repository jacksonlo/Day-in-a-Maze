using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;


public enum MenuButton {
	Play,
	Exit
}

public class MenuBehaviour : MonoBehaviour {

	public MenuButton button;
	public GameObject loadingImage;

	// Use this for initialization
	void Start () {
		loadingImage.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnMouseDown() {
		switch (button) {
		case MenuButton.Play:
			loadingImage.SetActive(true);
			SceneManager.LoadScene ("Maze");
			break;
		case MenuButton.Exit:
			break;
		}
	}
}
