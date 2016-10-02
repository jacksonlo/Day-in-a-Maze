using UnityEngine;
using System.Collections;

public class BlockBehaviour : MonoBehaviour {

	public float blockWidth;
	private bool _magnet;
	private bool _moving;
	private Vector3 _attractionPoint;
	private Vector3 _movementVector;
	private Rigidbody _rb;


	// Use this for initialization
	void Start () {
		blockWidth = transform.lossyScale.x;

		_magnet = true;
		_moving = false;
		_rb = GetComponent<Rigidbody> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void FixedUpdate() {
		if(_moving) {
			_rb.MovePosition(transform.position + _movementVector * Time.deltaTime);
		}
	}

	void OnCollisionEnter(Collision collision) {
		if (collision.gameObject.name.Contains("Block") && _magnet) {
			// If it was hit, set attractionPoint and move
			if (!_moving) {
				_moving = true;
				_attractionPoint = transform.position;

				// Set _movementVector based on where collision occurred
				SetMovementVector();
			}
		}
	}

	private void SetMovementVector() {
		_movementVector = transform.forward * blockWidth;
	}

}
