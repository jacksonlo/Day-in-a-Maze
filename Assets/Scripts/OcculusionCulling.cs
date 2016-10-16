using UnityEngine;
using System.Collections;

public class OcculusionCulling : MonoBehaviour {

	//private GameObject _maze;
	private GameObject[] _blocks;
	private Camera _camera;
	private bool _mazeReady;

	// Use this for initialization
	void Start () {
		_mazeReady = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (_mazeReady) {
			// Render/Unrender each block depending on of it is in view of the camera by a margin
			foreach (GameObject block in _blocks) {
				Vector3 blockPoint = _camera.WorldToViewportPoint (block.transform.position);
				bool inView = blockPoint.z > 0 && blockPoint.x > 0 && blockPoint.x < 1 && blockPoint.y > 0 && blockPoint.y < 1;
				Renderer rend = block.GetComponent<Renderer> ();
				rend.enabled = inView;
			}
		}
	}

	void MazeReady() {
		_blocks = GameObject.FindGameObjectsWithTag ("Block");
		_camera = GetComponent<Camera> ();
		_mazeReady = true;
	}
}
