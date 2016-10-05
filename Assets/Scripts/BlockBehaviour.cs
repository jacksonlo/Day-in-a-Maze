using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class BlockBehaviour : MonoBehaviour {

	public Block block = null;
	public float blockWidth;

	private bool _magnet;
	private bool _moving;
	private Vector3 _attractionPoint;
	private Vector3 _movementVector;
	private Rigidbody _rb;
	private Vector3 _targetPoint;

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
		// Moving towards target
		if (_moving) {
			if (_magnet) {
				// Magnetizing back to attraction point
				if (Util.CheckBeforeTarget (_movementVector, transform.position, _attractionPoint)) {
					_rb.MovePosition (transform.position + _movementVector * Time.deltaTime);
				} else {
					// Snap position and then turn magnet back on
					transform.position = _attractionPoint;
					_magnet = true;
				}
			} else {
				if (Util.CheckBeforeTarget (_movementVector, transform.position, _targetPoint)) {
					_rb.MovePosition (transform.position + _movementVector * Time.deltaTime);
				} else {
					// Snap position and then get ready for magnetizing back to attractionpoint
					transform.position = _targetPoint;
					_moving = false;
					_movementVector = _movementVector * -1;
				}
			}
		}
	}

	public void Move(Vector3 v) {
		_targetPoint = v;
		_moving = true;
		_magnet = false;
	}

	void OnCollisionEnter(Collision collision) {
		if (collision.gameObject.name.Contains("Block") && _magnet) {
			// If it was hit, set attractionPoint, targetPoint and move
			if (!_moving) {
				_moving = true;
				_magnet = false;
				_attractionPoint = transform.position;

				// Set movementVector based on where collision occurred
				SetMovementVector(GetBlockFaceHit(collision.gameObject, this.gameObject));

				// Set the target point after the movement vector
				_targetPoint = transform.position + _movementVector;
			}
		}
	}

	private void SetMovementVector(BlockFace face) {
		// Check available neighbours
		List<Vector3> available = new List<Vector3>();
		foreach (BlockFace bf in Enum.GetValues(typeof(BlockFace))) {
			if(bf == BlockFace.None || block.neighbours[bf] != null) {
				continue;
			}

			Vector3 move = Util.GetBlockFaceVector (bf, transform) * blockWidth;
			available.Add (move);
		}

		if (available.Count == 0) {
			_movementVector = Util.GetBlockFaceVector (Util.RandomBlockFaceExcept (face), transform);
		} else {
			System.Random random = new System.Random ();
			_movementVector = available [random.Next (0, available.Count)];
		}
	}

	private BlockFace GetBlockFaceHit( GameObject Object, GameObject ObjectHit ){

		BlockFace faceHit = BlockFace.None;
		RaycastHit MyRayHit;
		Vector3 direction = (Object.transform.position - ObjectHit.transform.position).normalized;
		Ray MyRay = new Ray(ObjectHit.transform.position, direction);

		if (Physics.Raycast(MyRay, out MyRayHit)) {
			if (MyRayHit.collider != null) {
				Vector3 MyNormal = MyRayHit.normal;
				MyNormal = MyRayHit.transform.TransformDirection( MyNormal );

				if( MyNormal == MyRayHit.transform.up ) { 
					faceHit = BlockFace.Top; 
				} else if( MyNormal == -MyRayHit.transform.up ) { 
					faceHit = BlockFace.Bottom; 
				} else if( MyNormal == MyRayHit.transform.forward ) { 
					faceHit = BlockFace.Front; 
				} else if( MyNormal == -MyRayHit.transform.forward ) { 
					faceHit = BlockFace.Back; 
				} else if( MyNormal == MyRayHit.transform.right ) {
					faceHit = BlockFace.Right; 
				} else if( MyNormal == -MyRayHit.transform.right ) {
					faceHit = BlockFace.Left; 
				}
			}    
		}
		return faceHit;
	}

}
