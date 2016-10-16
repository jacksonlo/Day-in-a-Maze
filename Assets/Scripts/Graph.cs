using System;
using System.Collections.Generic;

public class Graph<T> {

	public Dictionary<Node<T>, Node<T>> nodes;

	public Graph () {
		this.nodes = new Dictionary<Node<T>, Node<T>>();
	}

	public void Add(Node<T> node) {
		nodes.Add (node, node);
	}

	public void AddRange(List<Node<T>> nodeList) {
		for (int i = 0; i < nodeList.Count; ++i) {
			Add (nodeList [i]);
		}
	}

	public void Remove(Node<T> node) {
		nodes.Remove (node);
	}
}


