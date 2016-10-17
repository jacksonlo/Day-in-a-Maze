using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class BlockBehaviour : MonoBehaviour {

	public Block block = null;
	public float moveSpeed;
	private float _blockWidth;
	private bool _moving;
	private Vector3 _attractionPoint;
	private Vector3 _movementVector;
	private Rigidbody _rb;
	private Vector3 _targetPoint;

	// Use this for initialization
	void Start () {
		_blockWidth = transform.lossyScale.x;

		_moving = false;
		_rb = GetComponent<Rigidbody> ();
		_rb.constraints = RigidbodyConstraints.FreezeRotation;
		_attractionPoint = _rb.position;
		SetKinematic (true);
		moveSpeed = 1;
	}
	
	// Update is called once per frame
	void Update() {
		// Moving towards target
		if (_moving) {
			// Re-evaluate targetpoint incase direction in case misalignment
			//SetMoveTarget (_targetPoint, false);
			if (Util.CheckBeforeTarget (_movementVector, _rb.position, _targetPoint)) {
				_rb.MovePosition(_rb.position + _movementVector * moveSpeed * Time.deltaTime);
			} else {
				// Debug.Log ("Done Moving!");
				// Snap position and then get ready for magnetizing back to attractionpoint
				_rb.position = _targetPoint;
				_moving = false;
				//SetKinematic (false);
				_attractionPoint = _rb.position;
			}
		}
	}

	// Gridpoint refers to if it is a point reference on the maze grid rather than a realspace coordinate
	public void Move(Vector3 v, bool gridPoint = true) {
		SetMoveTarget (v, gridPoint);
		Move ();
	}

	public void Move() {
		_moving = true;
		SetKinematic (true);
	}

	public void SetKinematic(bool b) {
		_rb.isKinematic = b;
	}

	public void SetMoveTarget(Vector3 v, bool gridPoint = true) {
		_targetPoint = gridPoint ? v * _blockWidth : v;
		_movementVector = (_targetPoint - _rb.position).normalized;
	}

	// Returns true if current position is not on the attraction point
	public bool OffAnchor() {
		return _rb.position != _attractionPoint;
	}

	public void Magnetize() {
		Move (_attractionPoint, false);
	}

	public bool IsMoving() {
		return _moving;
	}

	public bool TriggerReady() {
		return !_moving && _rb.position != _targetPoint;
	}

	public Vector3 GetTarget(bool gridPoint = true) {
		return gridPoint ? _targetPoint / _blockWidth : _targetPoint;
	}

//	void OnCollisionEnter(Collision collision) {
//		if (collision.gameObject.name.Contains("Block") && _magnet) {
//			// If it was hit, set attractionPoint, targetPoint and move
//			if (!_moving) {
//				_moving = true;
//				_magnet = false;
//				_attractionPoint = transform.position;
//
//				// Set movementVector based on where collision occurred
//				SetMovementVector(GetBlockFaceHit(collision.gameObject, this.gameObject));
//
//				// Set the target point after the movement vector
//				_targetPoint = transform.position + _movementVector;
//			}
//		}
//	}

//	private void SetMovementVector(BlockFace face) {
//		// Check available neighbours
//		List<Vector3> available = new List<Vector3>();
//		foreach (BlockFace bf in Enum.GetValues(typeof(BlockFace))) {
//			if(bf == BlockFace.None || block.neighbours[bf] != null) {
//				continue;
//			}
//
//			Vector3 move = Util.GetBlockFaceVector (bf, transform) * _blockWidth;
//			available.Add (move);
//		}
//
//		if (available.Count == 0) {
//			_movementVector = Util.GetBlockFaceVector (Util.RandomBlockFaceExcept (face), transform);
//		} else {
//			System.Random random = new System.Random ();
//			_movementVector = available [random.Next (0, available.Count)];
//		}
//	}

//	private BlockFace GetBlockFaceHit( GameObject Object, GameObject ObjectHit ){
//
//		BlockFace faceHit = BlockFace.None;
//		RaycastHit MyRayHit;
//		Vector3 direction = (Object.transform.position - ObjectHit.transform.position).normalized;
//		Ray MyRay = new Ray(ObjectHit.transform.position, direction);
//
//		if (Physics.Raycast(MyRay, out MyRayHit)) {
//			if (MyRayHit.collider != null) {
//				Vector3 MyNormal = MyRayHit.normal;
//				MyNormal = MyRayHit.transform.TransformDirection( MyNormal );
//
//				if( MyNormal == MyRayHit.transform.up ) { 
//					faceHit = BlockFace.Top; 
//				} else if( MyNormal == -MyRayHit.transform.up ) { 
//					faceHit = BlockFace.Bottom; 
//				} else if( MyNormal == MyRayHit.transform.forward ) { 
//					faceHit = BlockFace.Front; 
//				} else if( MyNormal == -MyRayHit.transform.forward ) { 
//					faceHit = BlockFace.Back; 
//				} else if( MyNormal == MyRayHit.transform.right ) {
//					faceHit = BlockFace.Right; 
//				} else if( MyNormal == -MyRayHit.transform.right ) {
//					faceHit = BlockFace.Left; 
//				}
//			}    
//		}
//		return faceHit;
//	}

}
