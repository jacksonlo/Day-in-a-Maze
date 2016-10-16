using System;
using System.Collections;
using System.Collections.Generic;

public class Node<T> {

	public T value;
	public List<Node<T>> neighbours;
	public int depth { get; set; }

	public Node (T value) {
		this.value = value;
		this.neighbours = new List<Node<T>>();
	}

	public void Add(T value) {
		neighbours.Add (new Node<T> (value));
	}

	public void AddRange(List<T> values) {
		for (int i = 0; i < values.Count; ++i) {
			Add (values [i]);
		}
	}
		
	public void Remove(Node<T> node) {
		neighbours.Remove (node);
	}

	public override bool Equals(Object obj) {
		// Check for null values and compare run-time types.
		if (obj == null || GetType () != obj.GetType ()) {
			return false;
		}

		Node<T> n = (Node<T>)obj;
		return EqualityComparer<T>.Default.Equals (this.value, n.value);
	}

	public override int GetHashCode() {
		return value.GetHashCode ();
	}
}


