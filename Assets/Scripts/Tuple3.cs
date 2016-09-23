using System.Collections;
using System.Collections.Generic;

public class Tuple3<T> {
	public T first { get; set; }
	public T second { get; set; }
	public T third { get; set; }

	public Tuple3(T a, T b, T c) {
		first = a;
		second = b;
		third = c;
	}

	public override bool Equals (object obj) {
		// Check for null values and compare run-time types.
		if (obj == null || GetType() != obj.GetType()) 
			return false;

		Tuple3<T> t = (Tuple3<T>)obj;
		return EqualityComparer<T>.Default.Equals(first, t.first) && EqualityComparer<T>.Default.Equals(second, t.second) && EqualityComparer<T>.Default.Equals(third, t.third);
	}

	public override int GetHashCode() {
		int hash = 17;
		hash = hash * 23 + first.GetHashCode();
		hash = hash * 23 + second.GetHashCode();
		hash = hash * 23 + third.GetHashCode();
		return hash;
	}

	public static Tuple3<T> operator -(Tuple3<T> a, Tuple3<T> b)
	{
		return new Tuple3<T>(a.first - b.first, a.second - b.second, a.third - b.third);
	}
}
