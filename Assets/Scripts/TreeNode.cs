using System;
using System.Collections.Generic;

public class TreeNode<T> {

	public T value; 
	public List<TreeNode<T> > children = new List<TreeNode<T> >();
	public TreeNode<T> parent;
	public int level;

	public TreeNode(T value, TreeNode<T> parent = null) {
		this.value = value;
		this.parent = parent;
		this.level = parent == null ? 0 : parent.level + 1;
	}

	public TreeNode<T> AddChild(T value) {
		TreeNode<T> child = new TreeNode<T> (value, this);
		this.children.Add (child);
		return child;
	}

	public bool RemoveChild(TreeNode<T> node) {
		return children.Remove (node);
	}

	public void ApplyToChildren(Action<T> action) {
		foreach (TreeNode<T> child in children) {
			action (child);
		}
	}
		
}