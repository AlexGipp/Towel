﻿using System;

namespace Towel.DataStructures
{
	public interface IDeque<T> : IDataStructure<T>,
		// Structure Properties
		DataStructure.ICountable,
		DataStructure.IClearable
	{
		#region Methods

		void EnqueueBack(T enqueue);
		T DequeueFront();
		T PeekFront();

		#endregion
	}

	/// <summary>Implements First-In-First-Out queue data structure.</summary>
	/// <remarks>The runtimes of each public member are included in the "remarks" xml tags.</remarks>
	[Serializable]
	public class DequeLinked<T> : IDeque<T>
	{
		private Node _head;
		private Node _tail;
		private int _count;

		#region Nested Types

		/// <summary>This class just holds the data for each individual node of the list.</summary>
		[Serializable]
		private class Node
		{
			public T Value;
			public Node Next;

			internal Node(T data)
			{
				Value = data;
			}
		}

		#endregion

		#region Constructors

		/// <summary>Creates an instance of a queue.</summary>
		/// <remarks>Runtime: O(1).</remarks>
		public DequeLinked()
		{
			_head = _tail = null;
			_count = 0;
		}

		#endregion

		#region Properties

		/// <summary>Returns the number of items in the queue.</summary>
		/// <remarks>Runtime: O(1).</remarks>
		public int Count { get { return _count; } }

		#endregion

		#region Methods

		/// <summary>Adds an item to the back of the queue.</summary>
		/// <param name="enqueue">The item to add to the queue.</param>
		/// <runtime>O(1)</runtime>
		public void EnqueueBack(T enqueue)
		{
			if (_tail == null)
				_head = _tail = new Node(enqueue);
			else
				_tail = _tail.Next = new Node(enqueue);
			_count++;
		}

		/// <summary>Removes the oldest item in the queue.</summary>
		/// <runtime>O(1)</runtime>
		public T DequeueFront()
		{
			if (_head == null)
				throw new InvalidOperationException("Attempting to remove a non-existing id value.");
			T value = _head.Value;
			if (_head == _tail)
				_tail = null;
			_head = null;
			_count--;
			return value;
		}

		/// <summary>Looks at the front-most value.</summary>
		/// <returns>The front-most value.</returns>
		public T PeekFront()
		{
			if (_head == null)
				throw new InvalidOperationException("Attempting to remove a non-existing id value.");
			T returnValue = _head.Value;
			return returnValue;
		}

		/// <summary>Resets the queue to an empty state.</summary>
		/// <runtime>O(1)</runtime>
		public void Clear()
		{
			_head = _tail = null;
			_count = 0;
		}

		/// <summary>Converts the list into a standard array.</summary>
		/// <returns>A standard array of all the items.</returns>
		/// <runtime>O(n)</runtime>
		public T[] ToArray()
		{
			if (_count == 0)
				return null;
			T[] array = new T[_count];
			Node looper = _head;
			for (int i = 0; i < _count; i++)
			{
				array[i] = looper.Value;
				looper = looper.Next;
			}
			return array;
		}

		System.Collections.IEnumerator
			System.Collections.IEnumerable.GetEnumerator()
		{
			Node current = this._head;
			while (current != null)
			{
				yield return current.Value;
				current = current.Next;
			}
		}

		System.Collections.Generic.IEnumerator<T>
			System.Collections.Generic.IEnumerable<T>.GetEnumerator()
		{
			Node current = this._head;
			while (current != null)
			{
				yield return current.Value;
				current = current.Next;
			}
		}

		/// <summary>Checks to see if a given object is in this data structure.</summary>
		/// <param name="item">The item to check for.</param>
		/// <param name="compare">Delegate representing comparison technique.</param>
		/// <returns>true if the item is in this structure; false if not.</returns>
		public bool Contains(T item, Compare<T> compare)
		{
			throw new NotImplementedException();
		}

		/// <summary>Checks to see if a given object is in this data structure.</summary>
		/// <typeparam name="Key">The type of the key to check for.</typeparam>
		/// <param name="key">The key to check for.</param>
		/// <param name="compare">Delegate representing comparison technique.</param>
		/// <returns>true if the item is in this structure; false if not.</returns>
		public bool Contains<Key>(Key key, Compare<T, Key> compare)
		{
			throw new NotImplementedException();
		}

		/// <summary>Invokes a delegate for each entry in the data structure.</summary>
		/// <param name="step">The delegate to invoke on each item in the structure.</param>
		public void Stepper(Step<T> step)
		{
			Node current = this._head;
			while (current != null)
			{
				step(current.Value);
				current = current.Next;
			}
		}

		/// <summary>Invokes a delegate for each entry in the data structure.</summary>
		/// <param name="step">The delegate to invoke on each item in the structure.</param>
		public void Stepper(StepRef<T> step)
		{
			throw new NotImplementedException();
		}

		/// <summary>Invokes a delegate for each entry in the data structure.</summary>
		/// <param name="step">The delegate to invoke on each item in the structure.</param>
		/// <returns>The resulting status of the iteration.</returns>
		public StepStatus Stepper(StepBreak<T> step)
		{
			throw new NotImplementedException();
		}

		/// <summary>Invokes a delegate for each entry in the data structure.</summary>
		/// <param name="step">The delegate to invoke on each item in the structure.</param>
		/// <returns>The resulting status of the iteration.</returns>
		public StepStatus Stepper(StepRefBreak<T> step)
		{
			throw new NotImplementedException();
		}

		/// <summary>Creates a shallow clone of this data structure.</summary>
		/// <returns>A shallow clone of this data structure.</returns>
		public IDataStructure<T> Clone()
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
