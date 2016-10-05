using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum BlockType {
	Metal = 0, 
	White = 1
};
public enum BlockFace {Top, Bottom, Left, Right, Front, Back, None};

public class Block {
	public int x { get; set; }
	public int y { get; set; }
	public int z { get; set; }

	public GameObject blockObject;
	public Dictionary<BlockFace, Block> neighbours;

	private float _width;

	public Block(GameObject blockType, int x, int y, int z) {
		this.x = x;
		this.y = y;
		this.z = z;
		this.blockObject = blockType;
		_width = this.blockObject.transform.lossyScale.x;

		this.blockObject = GameObject.Instantiate (this.blockObject, new Vector3(x * _width, y * _width, z * _width), Quaternion.identity) as GameObject;

		InitializeNeighbours ();
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

	public void SetBlock() {
		BlockBehaviour bb = blockObject.GetComponent<BlockBehaviour> ();
		bb.block = this;
	}

	public void Move(Vector3 v) {
		BlockBehaviour bb = blockObject.GetComponent<BlockBehaviour> ();
		bb.Move (v);
	}
}
