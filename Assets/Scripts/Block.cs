using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum BlockType {Metal = 0, White = 1};
public enum BlockFace {Left, Right, Back, Front, Bottom, Top};


public class Block {
	public int x { get; set; }
	public int y { get; set; }
	public int z { get; set; }

	private GameObject _blockType;
	private float _width;

	public Block(GameObject blockType, int x, int y, int z) {
		this.x = x;
		this.y = y;
		this.z = z;
		_blockType = blockType;
		_width = _blockType.transform.lossyScale.x;

		_blockType = GameObject.Instantiate (_blockType, new Vector3(x * _width, y * _width, z * _width), Quaternion.identity) as GameObject;
	}

	public Block(GameObject blockType, GameObject parent, int x, int y, int z) {
		this.x = x;
		this.y = y;
		this.z = z;
		_blockType = blockType;
		_width = _blockType.transform.lossyScale.x;

		_blockType = GameObject.Instantiate (_blockType, new Vector3(x * _width, y * _width, z * _width), Quaternion.identity) as GameObject;
		_blockType.transform.parent = parent.transform;
	}

}
