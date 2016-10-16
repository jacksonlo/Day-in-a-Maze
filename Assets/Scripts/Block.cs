using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum BlockType {
	Metal08 = 0, 
	Metal10 = 1,
	Metal22 = 2,
	Metal24 = 3,
	Metal32 = 4
};
public enum BlockFace {Top, Bottom, Left, Right, Front, Back, None};

public class Block {
	public int x { get; set; }
	public int y { get; set; }
	public int z { get; set; }

	public GameObject blockObject;
	public Dictionary<BlockFace, Block> neighbours;

	private float _width;
	public BlockBehaviour _bb;

	public Block(GameObject blockType, int x, int y, int z) {
		this.x = x;
		this.y = y;
		this.z = z;
		this.blockObject = blockType;
		_width = this.blockObject.transform.lossyScale.x;

		this.blockObject = GameObject.Instantiate (this.blockObject, new Vector3(x * _width, y * _width, z * _width), Quaternion.identity) as GameObject;

		InitializeNeighbours ();
		_bb = blockObject.GetComponent<BlockBehaviour> ();
	}

	public Block(GameObject blockType, GameObject parent, int x, int y, int z) {
		this.x = x;
		this.y = y;
		this.z = z;
		this.blockObject = blockType;
		_width = this.blockObject.transform.lossyScale.x;

		this.blockObject = GameObject.Instantiate (this.blockObject, new Vector3(x * _width, y * _width, z * _width), Quaternion.identity) as GameObject;
		this.blockObject.transform.parent = parent.transform;

		InitializeNeighbours ();
		_bb = blockObject.GetComponent<BlockBehaviour> ();
	}

	private void InitializeNeighbours() {
		neighbours = new Dictionary<BlockFace, Block> ();
		neighbours.Add (BlockFace.Top, null);
		neighbours.Add (BlockFace.Bottom, null);
		neighbours.Add (BlockFace.Left, null);
		neighbours.Add (BlockFace.Right, null);
		neighbours.Add (BlockFace.Front, null);
		neighbours.Add (BlockFace.Back, null);
	}

	public void Render(bool b) {
		Renderer rend = blockObject.GetComponent<Renderer> ();
		if(rend.enabled != b) {
			rend.enabled = b;
		}
	}

	public bool IsRendered() {
		return blockObject.GetComponent<Renderer> ().enabled;
	}

	public void SetBlock() {
		_bb.block = this;
	}

	public void Move(Vector3 v, bool gridPoint = true) {
		_bb.Move (v, gridPoint);
	}

	public void Move() {
		_bb.Move ();
	}
		
	public void SetMoveTarget(Vector3 v, bool gridPoint = true) {
		_bb.SetMoveTarget (v, true);
	}
		
	public void Heartbeat() {
		if (!_bb.IsMoving () && _bb.OffAnchor()) {
			_bb.Magnetize ();
		}
	}

	public bool TriggerReady() {
		return _bb.TriggerReady();
	}

	public bool IsMoving() {
		return _bb.IsMoving ();
	}

	public bool OffAnchor() {
		return _bb.OffAnchor();
	}
}
