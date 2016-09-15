using System.Collections;
using System.Collections.Generic;

public class Tuple2<T> {
	public T first { get; set; }
	public T second { get; set; }

	public Tuple2(T a, T b) {
		first = a;
		second = b;
	}

	public override bool Equals(object obj) {
		if (obj == null || GetType() != obj.GetType()) 
			return false;

		Tuple2<T> t = (Tuple2<T>)obj;
		return EqualityComparer<T>.Default.Equals(first, t.first) && EqualityComparer<T>.Default.Equals(second, t.second);
	}

	public override int GetHashCode() {
		//return first ^ second;
		int hash = 17;
		hash = hash * 31 + first.GetHashCode();
		hash = hash * 31 + second.GetHashCode();
		return hash;
	}
}

