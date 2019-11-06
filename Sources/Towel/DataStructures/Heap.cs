﻿using System;
using System.Collections.Generic;
using static Towel.Syntax;

namespace Towel.DataStructures
{
	/// <summary>Stores items based on priorities and allows access to the highest priority item.</summary>
	/// <typeparam name="T">The generic type to be stored within the heap.</typeparam>
	public interface IHeap<T> : IDataStructure<T>,
		// Structure Properties
		DataStructure.ICountable,
		DataStructure.IClearable,
		DataStructure.IComparing<T>
	{
		#region Methods

		/// <summary>Enqueues an item into the heap.</summary>
		/// <param name="addition"></param>
		void Enqueue(T addition);
		/// <summary>Removes and returns the highest priority item.</summary>
		/// <returns>The highest priority item from the queue.</returns>
		T Dequeue();
		/// <summary>Returns the highest priority item.</summary>
		/// <returns>The highest priority item in the queue.</returns>
		T Peek();

		#endregion
	}

	/// <summary>Implements a priority heap with static priorities using an array.</summary>
	/// <typeparam name="T">The type of item to be stored in this priority heap.</typeparam>
	/// <citation>
	/// This heap imlpementation was originally developed by 
	/// Rodney Howell of Kansas State University. However, it has 
	/// been modified since its addition into the Towel framework.
	/// </citation>
	public class HeapArray<T> : IHeap<T>
	{
		internal readonly Compare<T> _compare;
		internal T[] _heap;
		internal int _minimumCapacity;
		internal int _count;
		internal const int _root = 1; // The root index of the heap.

		#region Constructors

		/// <summary>Generates a priority queue with a capacity of the parameter. Runtime O(1).</summary>
		/// <param name="compare">Delegate determining the comparison technique used for sorting.</param>
		/// <runtime>θ(1)</runtime>
		public HeapArray(Compare<T> compare)
		{
			_compare = compare;
			_heap = new T[2];
			_minimumCapacity = 1;
			_count = 0;
		}

		/// <summary>Generates a priority queue with a capacity of the parameter. Runtime O(1).</summary>
		/// <param name="compare">Delegate determining the comparison technique used for sorting.</param>
		/// <param name="minimumCapacity">The capacity you want this priority queue to have.</param>
		/// <runtime>θ(1)</runtime>
		public HeapArray(Compare<T> compare, int minimumCapacity)
		{
			_compare = compare;
			_heap = new T[minimumCapacity + 1];
			_minimumCapacity = minimumCapacity;
			_count = 0;
		}

		/// <summary>Generates a priority queue with a capacity of the parameter. Runtime O(1).</summary>
		/// <runtime>θ(1)</runtime>
		public HeapArray() : this(Towel.Compare.Default) { }

		#endregion

		#region Properties

		/// <summary>Delegate determining the comparison technique used for sorting.</summary>
		public Compare<T> Compare => _compare;

		/// <summary>The maximum items the queue can hold.</summary>
		/// <runtime>θ(1)</runtime>
		public int CurrentCapacity => _heap.Length - 1;

		/// <summary>The minumum capacity of this queue to limit low-level resizing.</summary>
		/// <runtime>θ(1)</runtime>
		public int MinimumCapacity => _minimumCapacity;

		/// <summary>The number of items in the queue.</summary>
		/// <runtime>O(1)</runtime>
		public int Count => _count;

		#endregion

		#region Methods

		/// <summary>Gets the index of the left child of the provided item.</summary>
		/// <param name="parent">The item to find the left child of.</param>
		/// <returns>The index of the left child of the provided item.</returns>
		internal static int LeftChild(int parent) => parent * 2;

		/// <summary>Gets the index of the right child of the provided item.</summary>
		/// <param name="parent">The item to find the right child of.</param>
		/// <returns>The index of the right child of the provided item.</returns>
		internal static int RightChild(int parent) => parent * 2 + 1;

		/// <summary>Gets the index of the parent of the provided item.</summary>
		/// <param name="child">The item to find the parent of.</param>
		/// <returns>The index of the parent of the provided item.</returns>
		internal static int Parent(int child) => child / 2;

		/// <summary>Enqueue an item into the priority queue and let it works its magic.</summary>
		/// <param name="addition">The item to be added.</param>
		/// <runtime>O(ln(n)), Ω(1), ε(ln(n))</runtime>
		public void Enqueue(T addition)
		{
			if (!(_count + 1 < _heap.Length))
			{
				if (_heap.Length * 2 > int.MaxValue)
				{
					throw new InvalidOperationException("this heap has become too large");
				}
				T[] _newHeap = new T[_heap.Length * 2];
				for (int i = 1; i <= _count; i++)
				{
					_newHeap[i] = _heap[i];
				}
				_heap = _newHeap;
			}
			_count++;
			_heap[_count] = addition;
			ShiftUp(_count);
		}

		/// <summary>Dequeues the item with the highest priority.</summary>
		/// <returns>The item of the highest priority.</returns>
		/// <runtime>O(ln(n))</runtime>
		public T Dequeue()
		{
			if (_count > 0)
			{
				T removal = _heap[_root];
				ArraySwap(_root, _count);
				_count--;
				ShiftDown(_root);
				return removal;
			}
			throw new InvalidOperationException("Attempting to remove from an empty priority queue.");
		}

		/// <summary>Requeues an item after a change has occured.</summary>
		/// <param name="item">The item to requeue.</param>
		/// <runtime>O(n)</runtime>
		public void Requeue(T item)
		{
			int i;
			for (i = 1; i <= _count; i++)
			{
				if (_compare(item, _heap[i]) == Equal)
				{
					break;
				}
			}
			if (i > _count)
			{
				throw new InvalidOperationException("Attempting to re-queue an item that is not in the heap.");
			}
			ShiftUp(i);
			ShiftDown(i);
		}

		/// <summary>This lets you peek at the top priority WITHOUT REMOVING it.</summary>
		/// <runtime>O(1)</runtime>
		public T Peek()
		{
			if (_count > 0)
			{
				return _heap[_root];
			}
			throw new InvalidOperationException("Attempting to peek at an empty priority queue.");
		}

		/// <summary>Standard priority queue algorithm for up sifting.</summary>
		/// <param name="index">The index to be up sifted.</param>
		/// <runtime>O(ln(n)), Ω(1)</runtime>
		internal void ShiftUp(int index)
		{
			int parent;
			while ((parent = Parent(index)) > 0 && _compare(_heap[index], _heap[parent]) == Greater)
			{
				ArraySwap(index, parent);
				index = parent;
			}
		}

		/// <summary>Standard priority queue algorithm for sifting down.</summary>
		/// <param name="index">The index to be down sifted.</param>
		/// <runtime>O(ln(n)), Ω(1)</runtime>
		internal void ShiftDown(int index)
		{
			int leftChild, rightChild;
			while ((leftChild = LeftChild(index)) <= _count)
			{
				int down = leftChild;
				if ((rightChild = RightChild(index)) <= _count && _compare(_heap[rightChild], _heap[leftChild]) == Greater)
				{
					down = rightChild;
				}
				if (_compare(_heap[down], _heap[index]) == Less)
				{
					break;
				}
				ArraySwap(index, down);
				index = down;
			}
		}

		/// <summary>Standard array swap method.</summary>
		/// <param name="indexOne">The first index of the swap.</param>
		/// <param name="indexTwo">The second index of the swap.</param>
		/// <runtime>O(1)</runtime>
		internal void ArraySwap(int indexOne, int indexTwo)
		{
			T temp = _heap[indexTwo];
			_heap[indexTwo] = _heap[indexOne];
			_heap[indexOne] = temp;
		}

		/// <summary>Returns this queue to an empty state.</summary>
		/// <runtime>O(1)</runtime>
		public void Clear()
		{
			_count = 0;
		}

		/// <summary>Converts the heap into an array using pre-order traversal (WARNING: items are not ordered).</summary>
		/// <returns>The array of priority-sorted items.</returns>
		public T[] ToArray()
		{
			T[] array = new T[_count];
			for (int i = 1; i <= _count; i++)
			{
				array[i] = _heap[i];
			}
			return array;
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			for (int i = 0; i <= _count; i++)
			{
				yield return _heap[i];
			}
		}

		/// <summary>Gets the enumerator of the heap.</summary>
		/// <returns>The enumerator of the heap.</returns>
		public IEnumerator<T> GetEnumerator()
		{
			for (int i = 0; i <= _count; i++)
			{
				yield return _heap[i];
			}
		}

		/// <summary>Invokes a delegate for each entry in the data structure.</summary>
		/// <param name="step">The delegate to invoke on each item in the structure.</param>
		public void Stepper(Step<T> step) => _heap.Stepper(step, 1, _count + 1);

		/// <summary>Invokes a delegate for each entry in the data structure.</summary>
		/// <param name="step">The delegate to invoke on each item in the structure.</param>
		public void Stepper(StepRef<T> step) => _heap.Stepper(step, 1, _count + 1);

		/// <summary>Invokes a delegate for each entry in the data structure.</summary>
		/// <param name="step">The delegate to invoke on each item in the structure.</param>
		/// <returns>The resulting status of the iteration.</returns>
		public StepStatus Stepper(StepBreak<T> step) => _heap.Stepper(step, 1, _count + 1);

		/// <summary>Invokes a delegate for each entry in the data structure.</summary>
		/// <param name="step">The delegate to invoke on each item in the structure.</param>
		/// <returns>The resulting status of the iteration.</returns>
		public StepStatus Stepper(StepRefBreak<T> step) => _heap.Stepper(step, 1, _count + 1);

		/// <summary>Creates a shallow clone of this data structure.</summary>
		/// <returns>A shallow clone of this data structure.</returns>
		public IDataStructure<T> Clone()
		{
			HeapArray<T> clone = new HeapArray<T>(_compare);
			T[] cloneItems = new T[_heap.Length];
			for (int i = 1; i <= _count; i++)
			{
				cloneItems[i] = _heap[i];
			}
			clone._heap = cloneItems;
			clone._count = _count;
			clone._minimumCapacity = _minimumCapacity;
			return clone;
		}

		#endregion
	}

	#region In Development

	#region HeapLinkedStatic
	/*
	/// <summary>Implements a mutable priority heap with static priorities using an array.</summary>
	/// <typeparam name="Type">The type of item to be stored in this priority heap.</typeparam>
	public class HeapLinkedStatic<T> : Heap<T>
	{
		#region HeapLinkedStaticNode

		/// <summary>This is just a storage class, it stores an entry in the priority heap and its priority.</summary>
		internal class HeapLinkedStaticNode
		{
			internal HeapLinkedStaticNode _parent;
			internal HeapLinkedStaticNode _leftChild;
			internal HeapLinkedStaticNode _rightChild;
			internal int _priority;
			internal Type _value;

			internal HeapLinkedStaticNode Parent { get { return _parent; } set { _parent = value; } }
			internal HeapLinkedStaticNode LeftChild { get { return _leftChild; } set { _leftChild = value; } }
			internal HeapLinkedStaticNode RightChild { get { return _rightChild; } set { _rightChild = value; } }
			internal int Priority { get { return _priority; } set { _priority = value; } }
			internal Type Value { get { return _value; } set { _value = value; } }

			internal HeapLinkedStaticNode(HeapLinkedStaticNode parent, int left, Type right)
			{
				_parent = parent;
				_priority = left;
				_value = right;
			}
		}

		#endregion

		internal HeapLinkedStaticNode[] _heapArray;
		internal int _count;

		internal object _lock;
		internal int _readers;
		internal int _writers;

		/// <summary>The maximum items the queue can hold.</summary>
		/// <runtime>O(1)</runtime>
		public int Capacity { get { return _heapArray.Length - 1; } }

		/// <summary>The number of items in the queue.</summary
		/// <runtime>O(1)</runtime>
		public int Count { get { return _count; } }

		/// <summary>True if full, false if there is still room.</summary>
		/// <runtime>O(1)</runtime>
		public bool IsFull { get { return _count == _heapArray.Length - 1; } }

		/// <summary>Generates a priority queue with a capacity of the parameter. Runtime O(1).</summary>
		/// <param name="capacity">The capacity you want this priority queue to have.</param>
		/// <runtime>Θ(capacity)</runtime>
		public HeapLinkedStatic(int capacity)
		{
			_heapArray = new HeapLinkedStaticNode[capacity + 1];
			_heapArray[0] = new HeapLinkedStaticNode(int.MaxValue, default(Type));
			for (int i = 1; i < capacity; i++)
				_heapArray[i] = new HeapLinkedStaticNode(int.MinValue, default(Type));
			_count = 0;
			_lock = new object();
		}

		/// <summary>Enqueue an item into the priority queue and let it works its magic.</summary>
		/// <param name="addition">The item to be added.</param>
		/// <param name="priority">The priority of the addition. (LARGER PRIORITY -> HIGHER PRIORITY)</param>
		/// <runtime>O(ln(n)), Ω(1), ε(ln(n))</runtime>
		public void Enqueue(Type addition, int priority)
		{
			WriterLock();
			if (!(_count < _heapArray.Length - 1))
			{
				WriterUnlock();
				throw new HeapArrayStaticException("Attempting to add to a full priority queue.");
			}
			_count++;
			// Functionality Note: imutable or mutable (next three lines)
			//_queueArray[_count + 1] = new Link<int, Type>(priority, addition);
			_heapArray[_count].Priority = priority;
			_heapArray[_count].Value = addition;
			ShiftUp(_count);
			WriterUnlock();
		}

		/// <summary>Dequeues the item with the highest priority.</summary>
		/// <returns>The item of the highest priority.</returns>
		/// <runtime>Θ(ln(n))</runtime>
		public Type Dequeue()
		{
			WriterLock();
			if (_count > 0)
			{
				Type removal = _heapArray[1].Value;
				ArraySwap(1, _count);
				_count--;
				ShiftDown(1);
				WriterUnlock();
				return removal;
			}
			WriterUnlock();
			throw new HeapArrayStaticException("Attempting to remove from an empty priority queue.");
		}

		/// <summary>This lets you peek at the top priority WITHOUT REMOVING it.</summary>
		/// <runtime>O(1)</runtime>
		public Type Peek()
		{
			ReaderLock();
			if (_count > 0)
			{
				ReaderUnlock();
				return _heapArray[1].Value;
			}
			ReaderUnlock();
			throw new HeapArrayStaticException("Attempting to peek at an empty priority queue.");
		}

		/// <summary>Standard priority queue algorithm for up sifting.</summary>
		/// <param name="index">The index to be up sifted.</param>
		/// <runtime>O(ln(n)), Ω(1)</runtime>
		internal void ShiftUp(int index)
		{
			// NOTE: "index * 2" is the index of the leftchild of the item at location "index"
			while (_heapArray[index].Priority > _heapArray[index / 2].Priority)
			{
				ArraySwap(index, index / 2);
				index = index / 2;
			}
		}

		/// <summary>Standard priority queue algorithm for sifting down.</summary>
		/// <param name="index">The index to be down sifted.</param>
		/// <runtime>O(ln(n)), Ω(1)</runtime>
		internal void ShiftDown(int index)
		{
			// NOTE: "index * 2" is the index of the leftchild of the item at location "index"
			while ((index * 2) <= _count)
			{
				int index2 = index * 2;
				if (((index * 2) + 1) <= _count && _heapArray[(index * 2) + 1].Priority < _heapArray[index].Priority) index2++;
				// NOTE: "(index * 2) + 1" is the index of the rightchild of the item at location "index"
				if (_heapArray[index].Priority >= _heapArray[index2].Priority) break;
				ArraySwap(index, index2);
				index = index2;
			}
		}

		/// <summary>Standard array swap method.</summary>
		/// <param name="indexOne">The first index of the swap.</param>
		/// <param name="indexTwo">The second index of the swap.</param>
		/// <runtime>O(1)</runtime>
		internal void ArraySwap(int indexOne, int indexTwo)
		{
			HeapLinkedStaticNode swapStorage = _heapArray[indexTwo];
			_heapArray[indexTwo] = _heapArray[indexOne];
			_heapArray[indexOne] = swapStorage;
		}

		/// <summary>Returns this queue to an empty state.</summary>
		/// <runtime>O(1)</runtime>
		public void Clear() { WriterLock(); _count = 0; WriterUnlock(); }

		/// <summary>Traversal function for a heap. Following a pre-order traversal.</summary>
		/// <param name="traversalFunction">The function to perform per iteration.</param>
		/// <returns>A determining a break in the traversal. (true = continue, false = break)</returns>
		public bool TraverseBreakable(Func<Type, bool> traversalFunction) { return TraversalPreOrderBreakable(traversalFunction); }

		/// <summary>Traversal function for a heap. Following a pre-order traversal.</summary>
		/// <param name="traversalFunction">The function to perform per iteration.</param>
		public void Traverse(Action<T> traversalFunction) { TraversalPreOrder(traversalFunction); }

		/// <summary>Implements an imperative traversal of the structure.</summary>
		/// <param name="traversalFunction">The function to perform per node in the traversal.</param>
		/// <runtime>O(n * traversalFunction)</runtime>
		public bool TraversalPreOrderBreakable(Func<Type, bool> traversalFunction)
		{
			ReaderLock();
			for (int i = 0; i < _count; i++)
			{
				if (!traversalFunction(_heapArray[i].Value))
				{
					ReaderUnlock();
					return false;
				}
			}
			ReaderUnlock();
			return true;
		}

		/// <summary>Implements an imperative traversal of the structure.</summary>
		/// <param name="traversalAction">The action to perform per node in the traversal.</param>
		/// <runtime>O(n * traversalAction)</runtime>
		public void TraversalPreOrder(Action<T> traversalAction)
		{
			ReaderLock();
			for (int i = 0; i < _count; i++) traversalAction(_heapArray[i].Value);
			ReaderUnlock();
		}

		/// <summary>Converts the heap into an array using pre-order traversal (WARNING: items are not ordered).</summary>
		/// <returns>The array of priority-sorted items.</returns>
		public Type[] ToArray()
		{
			ReaderLock();
			Type[] array = new Type[_count];
			for (int i = 0; i < _count; i++) { array[i] = _heapArray[i].Value; }
			ReaderUnlock();
			return array;
		}

		/// <summary>Thread safe enterance for readers.</summary>
		internal void ReaderLock() { lock (_lock) { while (!(_writers == 0)) Monitor.Wait(_lock); _readers++; } }
		/// <summary>Thread safe exit for readers.</summary>
		internal void ReaderUnlock() { lock (_lock) { _readers--; Monitor.Pulse(_lock); } }
		/// <summary>Thread safe enterance for writers.</summary>
		internal void WriterLock() { lock (_lock) { while (!(_writers == 0) && !(_readers == 0)) Monitor.Wait(_lock); _writers++; } }
		/// <summary>Thread safe exit for readers.</summary>
		internal void WriterUnlock() { lock (_lock) { _writers--; Monitor.PulseAll(_lock); } }

		/// <summary>This is used for throwing imutable priority queue exceptions only to make debugging faster.</summary>
		internal class HeapArrayStaticException : Exception { public HeapArrayStaticException(string message) : base(message) { } }
	}

	*/
	#endregion

	#region HeapArrayStatic

	///// <summary>Implements a mutable priority heap with static priorities using an array.</summary>
	///// <typeparam name="Type">The type of item to be stored in this priority heap.</typeparam>
	//[Serializable]
	//public class HeapArrayStatic<T> //: Heap<T>
	//{
	//	#region HeapArrayLink

	//	/// <summary>This is just a storage class, it stores an entry in the priority heap and its priority.</summary>
	//	internal class HeapArrayLink
	//	{
	//		internal int _priority;
	//		internal Type _value;

	//		internal int Priority { get { return _priority; } set { _priority = value; } }
	//		internal Type Value { get { return _value; } set { _value = value; } }

	//		internal HeapArrayLink(int left, Type right)
	//		{
	//			_priority = left;
	//			_value = right;
	//		}
	//	}

	//	#endregion

	//	internal HeapArrayLink[] _heapArray;
	//	internal int _count;

	//	internal object _lock;
	//	internal int _readers;
	//	internal int _writers;

	//	/// <summary>The maximum items the queue can hold.</summary>
	//	/// <runtime>O(1)</runtime>
	//	public int Capacity { get { return _heapArray.Length - 1; } }

	//	/// <summary>The number of items in the queue.</summary
	//	/// <runtime>O(1)</runtime>
	//	public int Count { get { return _count; } }

	//	/// <summary>True if full, false if there is still room.</summary>
	//	/// <runtime>O(1)</runtime>
	//	public bool IsFull { get { return _count == _heapArray.Length - 1; } }

	//	/// <summary>Generates a priority queue with a capacity of the parameter. Runtime O(1).</summary>
	//	/// <param name="capacity">The capacity you want this priority queue to have.</param>
	//	/// <runtime>Θ(capacity)</runtime>
	//	public HeapArrayStatic(int capacity)
	//	{
	//		_heapArray = new HeapArrayLink[capacity + 1];
	//		_heapArray[0] = new HeapArrayLink(int.MaxValue, default(Type));
	//		for (int i = 1; i < capacity + 1; i++)
	//			_heapArray[i] = new HeapArrayLink(int.MinValue, default(Type));
	//		_count = 0;
	//		_lock = new object();
	//	}

	//	/// <summary>Enqueue an item into the priority queue and let it works its magic.</summary>
	//	/// <param name="addition">The item to be added.</param>
	//	/// <param name="priority">The priority of the addition. (LARGER PRIORITY -> HIGHER PRIORITY)</param>
	//	/// <runtime>O(ln(n)), Ω(1), ε(ln(n))</runtime>
	//	public void Enqueue(Type addition, int priority)
	//	{
	//		WriterLock();
	//		if (!(_count < _heapArray.Length - 1))
	//		{
	//			WriterUnlock();
	//			throw new HeapArrayStaticException("Attempting to add to a full priority queue.");
	//		}
	//		_count++;
	//		// Functionality Note: imutable or mutable (next three lines)
	//		//_queueArray[_count + 1] = new Link<int, Type>(priority, addition);
	//		_heapArray[_count].Priority = priority;
	//		_heapArray[_count].Value = addition;
	//		ShiftUp(_count);
	//		WriterUnlock();
	//	}

	//	/// <summary>Dequeues the item with the highest priority.</summary>
	//	/// <returns>The item of the highest priority.</returns>
	//	/// <runtime>Θ(ln(n))</runtime>
	//	public Type Dequeue()
	//	{
	//		WriterLock();
	//		if (_count > 0)
	//		{
	//			Type removal = _heapArray[1].Value;
	//			ArraySwap(1, _count);
	//			_count--;
	//			ShiftDown(1);
	//			WriterUnlock();
	//			return removal;
	//		}
	//		WriterUnlock();
	//		throw new HeapArrayStaticException("Attempting to remove from an empty priority queue.");
	//	}

	//	/// <summary>This lets you peek at the top priority WITHOUT REMOVING it.</summary>
	//	/// <runtime>O(1)</runtime>
	//	public Type Peek()
	//	{
	//		ReaderLock();
	//		if (_count > 0)
	//		{
	//			ReaderUnlock();
	//			return _heapArray[1].Value;
	//		}
	//		ReaderUnlock();
	//		throw new HeapArrayStaticException("Attempting to peek at an empty priority queue.");
	//	}

	//	/// <summary>Standard priority queue algorithm for up sifting.</summary>
	//	/// <param name="index">The index to be up sifted.</param>
	//	/// <runtime>O(ln(n)), Ω(1)</runtime>
	//	internal void ShiftUp(int index)
	//	{
	//		// NOTE: "index * 2" is the index of the leftchild of the item at location "index"
	//		while (_heapArray[index].Priority > _heapArray[index / 2].Priority)
	//		{
	//			ArraySwap(index, index / 2);
	//			index = index / 2;
	//		}
	//	}

	//	/// <summary>Standard priority queue algorithm for sifting down.</summary>
	//	/// <param name="index">The index to be down sifted.</param>
	//	/// <runtime>O(ln(n)), Ω(1)</runtime>
	//	internal void ShiftDown(int index)
	//	{
	//		// NOTE: "index * 2" is the index of the leftchild of the item at location "index"
	//		while ((index * 2) <= _count)
	//		{
	//			int index2 = index * 2;
	//			if (((index * 2) + 1) <= _count && _heapArray[(index * 2) + 1].Priority < _heapArray[index].Priority) index2++;
	//			// NOTE: "(index * 2) + 1" is the index of the rightchild of the item at location "index"
	//			if (_heapArray[index].Priority >= _heapArray[index2].Priority) break;
	//			ArraySwap(index, index2);
	//			index = index2;
	//		}
	//	}

	//	/// <summary>Standard array swap method.</summary>
	//	/// <param name="indexOne">The first index of the swap.</param>
	//	/// <param name="indexTwo">The second index of the swap.</param>
	//	/// <runtime>O(1)</runtime>
	//	internal void ArraySwap(int indexOne, int indexTwo)
	//	{
	//		HeapArrayLink swapStorage = _heapArray[indexTwo];
	//		_heapArray[indexTwo] = _heapArray[indexOne];
	//		_heapArray[indexOne] = swapStorage;
	//	}

	//	/// <summary>Returns this queue to an empty state.</summary>
	//	/// <runtime>O(1)</runtime>
	//	public void Clear() { WriterLock(); _count = 0; WriterUnlock(); }

	//	/// <summary>Traversal function for a heap. Following a pre-order traversal.</summary>
	//	/// <param name="traversalFunction">The function to perform per iteration.</param>
	//	/// <returns>A determining a break in the traversal. (true = continue, false = break)</returns>
	//	public bool TraverseBreakable(Func<Type, bool> traversalFunction) { return TraversalPreOrderBreakable(traversalFunction); }

	//	/// <summary>Traversal function for a heap. Following a pre-order traversal.</summary>
	//	/// <param name="traversalFunction">The function to perform per iteration.</param>
	//	public void Traverse(Action<T> traversalFunction) { TraversalPreOrder(traversalFunction); }

	//	/// <summary>Implements an imperative traversal of the structure.</summary>
	//	/// <param name="traversalFunction">The function to perform per node in the traversal.</param>
	//	/// <runtime>O(n * traversalFunction)</runtime>
	//	public bool TraversalPreOrderBreakable(Func<Type, bool> traversalFunction)
	//	{
	//		ReaderLock();
	//		for (int i = 0; i < _count; i++)
	//		{
	//			if (!traversalFunction(_heapArray[i].Value))
	//			{
	//				ReaderUnlock();
	//				return false;
	//			}
	//		}
	//		ReaderUnlock();
	//		return true;
	//	}

	//	/// <summary>Implements an imperative traversal of the structure.</summary>
	//	/// <param name="traversalAction">The action to perform per node in the traversal.</param>
	//	/// <runtime>O(n * traversalAction)</runtime>
	//	public void TraversalPreOrder(Action<T> traversalAction)
	//	{
	//		ReaderLock();
	//		for (int i = 0; i < _count; i++) traversalAction(_heapArray[i].Value);
	//		ReaderUnlock();
	//	}

	//	/// <summary>Converts the heap into an array using pre-order traversal (WARNING: items are not ordered).</summary>
	//	/// <returns>The array of priority-sorted items.</returns>
	//	public Type[] ToArray()
	//	{
	//		ReaderLock();
	//		Type[] array = new Type[_count];
	//		for (int i = 0; i < _count; i++) { array[i] = _heapArray[i].Value; }
	//		ReaderUnlock();
	//		return array;
	//	}

	//	/// <summary>Thread safe enterance for readers.</summary>
	//	internal void ReaderLock() { lock (_lock) { while (!(_writers == 0)) Monitor.Wait(_lock); _readers++; } }
	//	/// <summary>Thread safe exit for readers.</summary>
	//	internal void ReaderUnlock() { lock (_lock) { _readers--; Monitor.Pulse(_lock); } }
	//	/// <summary>Thread safe enterance for writers.</summary>
	//	internal void WriterLock() { lock (_lock) { while (!(_writers == 0) && !(_readers == 0)) Monitor.Wait(_lock); _writers++; } }
	//	/// <summary>Thread safe exit for readers.</summary>
	//	internal void WriterUnlock() { lock (_lock) { _writers--; Monitor.PulseAll(_lock); } }

	//	#region Structure<T>

	//	#region .Net Framework Compatibility

	//	///// <summary>FOR COMPATIBILITY ONLY. AVOID IF POSSIBLE.</summary>
	//	//IEnumerator IEnumerable.GetEnumerator()
	//	//{
	//	//	throw new NotImplementedException();
	//	//}

	//	///// <summary>FOR COMPATIBILITY ONLY. AVOID IF POSSIBLE.</summary>
	//	//IEnumerator<T> IEnumerable<T>.GetEnumerator()
	//	//{
	//	//	throw new NotImplementedException();
	//	//}

	//	#endregion

	//	/// <summary>Pulls out all the values in the structure that are equivalent to the key.</summary>
	//	/// <typeparam name="Key">The type of the key to check for.</typeparam>
	//	/// <param name="key">The key to check for.</param>
	//	/// <param name="compare">Delegate representing comparison technique.</param>
	//	/// <returns>An array containing all the values matching the key or null if non were found.</returns>
	//	//Type[] GetValues<Key>(Key key, Compare<Type, Key> compare);

	//	/// <summary>Pulls out all the values in the structure that are equivalent to the key.</summary>
	//	/// <typeparam name="Key">The type of the key to check for.</typeparam>
	//	/// <param name="key">The key to check for.</param>
	//	/// <param name="compare">Delegate representing comparison technique.</param>
	//	/// <returns>An array containing all the values matching the key or null if non were found.</returns>
	//	/// <param name="values">The values that matched the given key.</param>
	//	/// <returns>true if 1 or more values were found; false if no values were found.</returns>
	//	//bool TryGetValues<Key>(Key key, Compare<Type, Key> compare, out Type[] values);

	//	/// <summary>Checks to see if a given object is in this data structure.</summary>
	//	/// <param name="item">The item to check for.</param>
	//	/// <param name="compare">Delegate representing comparison technique.</param>
	//	/// <returns>true if the item is in this structure; false if not.</returns>
	//	public bool Contains(Type item, Compare<T> compare)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	/// <summary>Checks to see if a given object is in this data structure.</summary>
	//	/// <typeparam name="Key">The type of the key to check for.</typeparam>
	//	/// <param name="key">The key to check for.</param>
	//	/// <param name="compare">Delegate representing comparison technique.</param>
	//	/// <returns>true if the item is in this structure; false if not.</returns>
	//	public bool Contains<Key>(Key key, Compare<Type, Key> compare)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	/// <summary>Invokes a delegate for each entry in the data structure.</summary>
	//	/// <param name="function">The delegate to invoke on each item in the structure.</param>
	//	public void Stepper(Step<T> function)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	/// <summary>Invokes a delegate for each entry in the data structure.</summary>
	//	/// <param name="function">The delegate to invoke on each item in the structure.</param>
	//	public void Stepper(StepRef<T> function)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	/// <summary>Invokes a delegate for each entry in the data structure.</summary>
	//	/// <param name="function">The delegate to invoke on each item in the structure.</param>
	//	/// <returns>The resulting status of the iteration.</returns>
	//	public StepStatus Stepper(StepBreak<T> function)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	/// <summary>Invokes a delegate for each entry in the data structure.</summary>
	//	/// <param name="function">The delegate to invoke on each item in the structure.</param>
	//	/// <returns>The resulting status of the iteration.</returns>
	//	public StepStatus Stepper(StepRefBreak<T> function)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	/// <summary>Creates a shallow clone of this data structure.</summary>
	//	/// <returns>A shallow clone of this data structure.</returns>
	//	public Structure<T> Clone()
	//	{
	//		throw new NotImplementedException();
	//	}

	//	///// <summary>Converts the structure into an array.</summary>
	//	///// <returns>An array containing all the item in the structure.</returns>
	//	//public Type[] ToArray()
	//	//{
	//	//	throw new NotImplementedException();
	//	//}

	//	#endregion

	//	/// <summary>This is used for throwing imutable priority queue exceptions only to make debugging faster.</summary>
	//	internal class HeapArrayStaticException : System.Exception
	//	{
	//		public HeapArrayStaticException(string message) : base(message) { }
	//	}
	//}

	#endregion

	#region HeapArrayDynamic

	///// <summary>Implements a mutable priority heap with dynamic priorities using an array and a hash table.</summary>
	///// <typeparam name="Type">The type of item to be stored in this priority heap.</typeparam>
	//[Serializable]
	//public class HeapArrayDynamic<T> : Heap<T>
	//{
	//	#region HeapArrayDynamicLink

	//	/// <summary>This is just a storage class, it stores an entry in the priority heap and its priority.</summary>
	//	internal class HeapArrayDynamicLink
	//	{
	//		internal int _priority;
	//		internal Type _value;

	//		internal int Priority { get { return _priority; } set { _priority = value; } }
	//		internal Type Value { get { return _value; } set { _value = value; } }

	//		internal HeapArrayDynamicLink(int left, Type right)
	//		{
	//			_priority = left;
	//			_value = right;
	//		}
	//	}

	//	#endregion

	//	internal int _count;
	//	internal HeapArrayDynamicLink[] _heapArray;
	//	internal HashTableLinked<int, Type> _indexingReference;

	//	internal object _lock;
	//	internal int _readers;
	//	internal int _writers;

	//	/// <summary>The maximum items the queue can hold.</summary>
	//	/// <runtime>O(1)</runtime>
	//	public int Capacity { get { ReaderLock(); int capacity = _heapArray.Length - 1; ReaderUnlock(); return capacity; } }

	//	/// <summary>The number of items in the queue.</summary
	//	/// <runtime>O(1)</runtime>
	//	public int Count { get { ReaderLock(); int count = _count; ReaderUnlock(); return count; } }

	//	/// <summary>True if full, false if there is still room.</summary>
	//	/// <runtime>O(1)</runtime>
	//	public bool IsFull { get { ReaderLock(); bool isFull = _count == _heapArray.Length - 1; ReaderUnlock(); return isFull; } }

	//	/// <summary>Generates a priority queue with a capacity of the parameter.</summary>
	//	/// <param name="capacity">The capacity you want this priority queue to have.</param>
	//	/// <runtime>Θ(capacity)</runtime>
	//	public HeapArrayDynamic(int capacity)
	//	{
	//		_indexingReference = new HashTableLinked<int, Type>();
	//		_heapArray = new HeapArrayDynamicLink[capacity + 1];
	//		_heapArray[0] = new HeapArrayDynamicLink(int.MaxValue, default(Type));
	//		for (int i = 1; i < capacity; i++)
	//			_heapArray[0] = new HeapArrayDynamicLink(int.MinValue, default(Type));
	//		_count = 0;
	//		_lock = new object();
	//	}

	//	/// <summary>Enqueue an item into the priority queue and let it works its magic.</summary>
	//	/// <param name="addition">The item to be added.</param>
	//	/// <param name="priority">The priority of the addition (LARGER PRIORITY -> HIGHER PRIORITY).</param>
	//	/// <runtime>O(n), Ω(1), ε(ln(n))</runtime>
	//	public void Enqueue(Type addition, int priority)
	//	{
	//		WriterLock();
	//		if (!(_count < _heapArray.Length - 1))
	//		{
	//			WriterUnlock();
	//			throw new HeapArrayDynamicException("Attempting to add to a full priority queue.");
	//		}
	//		_count++;
	//		// Functionality Note: imutable or mutable (next three lines)
	//		//_queueArray[_count + 1] = new Link<int, Type>(priority, addition);
	//		_heapArray[_count].Priority = priority;
	//		_heapArray[_count].Value = addition;
	//		// Runtime Note: O(n) cause by hash table addition
	//		_indexingReference.Add(addition, _count);
	//		ShiftUp(_count);
	//		WriterUnlock();
	//	}

	//	/// <summary>This lets you peek at the top priority WITHOUT REMOVING it.</summary>
	//	/// <runtime>O(1)</runtime>
	//	public Type Peek()
	//	{
	//		ReaderLock();
	//		if (_count > 0)
	//		{
	//			Type peek = _heapArray[1].Value;
	//			ReaderUnlock();
	//			return peek;
	//		}
	//		ReaderUnlock();
	//		throw new HeapArrayDynamicException("Attempting to peek at an empty priority queue.");
	//	}

	//	/// <summary>Dequeues the item with the highest priority.</summary>
	//	/// <returns>The item of the highest priority.</returns>
	//	/// <runtime>O(n), Ω(ln(n)), ε(ln(n))</runtime>
	//	public Type Dequeue()
	//	{
	//		WriterLock();
	//		if (_count > 0)
	//		{
	//			Type removal = _heapArray[1].Value;
	//			ArraySwap(1, _count);
	//			_count--;
	//			// Runtime Note: O(n) caused by has table removal
	//			_indexingReference.Remove(removal);
	//			ShiftDown(1);
	//			WriterUnlock();
	//			return removal;
	//		}
	//		WriterUnlock();
	//		throw new HeapArrayDynamicException("Attempting to dequeue from an empty priority queue.");
	//	}

	//	/// <summary>Increases the priority of an item in the queue.</summary>
	//	/// <param name="item">The item to have its priority increased.</param>
	//	/// <param name="priority">The ammount to increase the priority by (LARGER INT -> HIGHER PRIORITY).</param>
	//	/// <runtime>O(n), Ω(1), ε(ln(n))</runtime>
	//	public void IncreasePriority(Type item, int priority)
	//	{
	//		WriterLock();
	//		// Runtime Note: O(n) caused by hash table look-up.
	//		int index = _indexingReference[item];
	//		// Functionality Note: imutable or mutable (next two lines)
	//		//_queueArray[index] = new Link<int, Type>(_queueArray[index].Left + priority, item);
	//		_heapArray[index].Priority += priority;
	//		ShiftUp(index);
	//		WriterUnlock();
	//	}

	//	/// <summary>Decreases the priority of an item in the queue.</summary>
	//	/// <param name="item">The item to have its priority decreased.</param>
	//	/// <param name="priority">The ammount to decrease the priority by (LARGER INT -> HIGHER PRIORITY).</param>
	//	/// <runtime>O(n), Ω(1), ε(ln(n))</runtime>
	//	public void DecreasePriority(Type item, int priority)
	//	{
	//		WriterLock();
	//		// Runtime Note: O(n) caused by hash table look-up.
	//		int index = _indexingReference[item];
	//		// Functionality Note: imutable or mutable (next two lines)
	//		//_queueArray[index] = new Link<int, Type>(_queueArray[index].Left - priority, item);
	//		_heapArray[index].Priority -= priority;
	//		ShiftDown(index);
	//		WriterUnlock();
	//	}

	//	/// <summary>Standard priority queue algorithm for up sifting.</summary>
	//	/// <param name="index">The index to be up sifted.</param>
	//	/// <runtime>O(ln(n)), Ω(1)</runtime>
	//	internal void ShiftUp(int index)
	//	{
	//		// NOTE: "index / 2" is the index of the parent of the item at location "index"
	//		while (_heapArray[index].Priority > _heapArray[index / 2].Priority)
	//		{
	//			ArraySwap(index, index / 2);
	//			index = index / 2;
	//		}
	//	}

	//	/// <summary>Standard priority queue algorithm for sifting down.</summary>
	//	/// <param name="index">The index to be down sifted.</param>
	//	/// <runtime>O(ln(n)), Ω(1)</runtime>
	//	internal void ShiftDown(int index)
	//	{
	//		// NOTE: "index * 2" is the index of the leftchild of the item at location "index"
	//		while ((index * 2) <= _count)
	//		{
	//			int index2 = index * 2;
	//			if (((index * 2) + 1) <= _count && _heapArray[(index * 2) + 1].Priority < _heapArray[index].Priority) index2++;
	//			// NOTE: "(index * 2) + 1" is the index of the rightchild of the item at location "index"
	//			if (_heapArray[index].Priority >= _heapArray[index2].Priority) break;
	//			ArraySwap(index, index2);
	//			index = index2;
	//		}
	//	}

	//	/// <summary>Standard array swap method.</summary>
	//	/// <param name="indexOne">The first index of the swap.</param>
	//	/// <param name="indexTwo">The second index of the swap.</param>
	//	/// <runtime>O(1)</runtime>
	//	internal void ArraySwap(int indexOne, int indexTwo)
	//	{
	//		HeapArrayDynamicLink swapStorage = _heapArray[indexTwo];
	//		_heapArray[indexTwo] = _heapArray[indexOne];
	//		_heapArray[indexOne] = swapStorage;
	//		_indexingReference[_heapArray[indexOne].Value] = indexOne;
	//		_indexingReference[_heapArray[indexTwo].Value] = indexTwo;
	//	}

	//	/// <summary>Returns this queue to an empty state.</summary>
	//	/// <runtime>O(1)</runtime>
	//	public void Clear() { WriterLock(); _indexingReference.Clear(); _count = 0; WriterUnlock(); }

	//	/// <summary>Traversal function for a heap. Following a pre-order traversal.</summary>
	//	/// <param name="traversalFunction">The function to perform per iteration.</param>
	//	/// <returns>A determining a break in the traversal. (true = continue, false = break)</returns>
	//	public bool TraverseBreakable(Func<Type, bool> traversalFunction) { return TraversalPreOrderBreakable(traversalFunction); }

	//	/// <summary>Traversal function for a heap. Following a pre-order traversal.</summary>
	//	/// <param name="traversalFunction">The function to perform per iteration.</param>
	//	public void Traverse(Action<T> traversalFunction) { TraversalPreOrder(traversalFunction); }

	//	/// <summary>Implements an imperative traversal of the structure.</summary>
	//	/// <param name="traversalFunction">The function to perform per node in the traversal.</param>
	//	/// <runtime>O(n * traversalFunction)</runtime>
	//	public bool TraversalPreOrderBreakable(Func<Type, bool> traversalFunction)
	//	{
	//		ReaderLock();
	//		for (int i = 0; i < _count; i++)
	//		{
	//			if (!traversalFunction(_heapArray[i].Value))
	//			{
	//				ReaderUnlock();
	//				return false;
	//			}
	//		}
	//		ReaderUnlock();
	//		return true;
	//	}

	//	/// <summary>Implements an imperative traversal of the structure.</summary>
	//	/// <param name="traversalAction">The action to perform per node in the traversal.</param>
	//	/// <runtime>O(n * traversalAction)</runtime>
	//	public void TraversalPreOrder(Action<T> traversalAction)
	//	{
	//		ReaderLock();
	//		for (int i = 0; i < _count; i++) traversalAction(_heapArray[i].Value);
	//		ReaderUnlock();
	//	}

	//	/// <summary>Gets all the items in the heap. WARNING: the return items are NOT ordered.</summary>
	//	/// <returns>The items in the heap in random order.</returns>
	//	public Type[] ToArray()
	//	{
	//		ReaderLock();
	//		Type[] array = new Type[_count];
	//		for (int i = 0; i < _count; i++)
	//			array[i] = _heapArray[i].Value;
	//		ReaderUnlock();
	//		return array;
	//	}

	//	/// <summary>Thread safe enterance for readers.</summary>
	//	internal void ReaderLock() { lock (_lock) { while (!(_writers == 0)) Monitor.Wait(_lock); _readers++; } }
	//	/// <summary>Thread safe exit for readers.</summary>
	//	internal void ReaderUnlock() { lock (_lock) { _readers--; Monitor.Pulse(_lock); } }
	//	/// <summary>Thread safe enterance for writers.</summary>
	//	internal void WriterLock() { lock (_lock) { while (!(_writers == 0) && !(_readers == 0)) Monitor.Wait(_lock); _writers++; } }
	//	/// <summary>Thread safe exit for readers.</summary>
	//	internal void WriterUnlock() { lock (_lock) { _writers--; Monitor.PulseAll(_lock); } }

	//	#region Structure<T>

	//	#region .Net Framework Compatibility

	//	/// <summary>FOR COMPATIBILITY ONLY. AVOID IF POSSIBLE.</summary>
	//	IEnumerator IEnumerable.GetEnumerator()
	//	{
	//		throw new NotImplementedException();
	//	}

	//	/// <summary>FOR COMPATIBILITY ONLY. AVOID IF POSSIBLE.</summary>
	//	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	//	{
	//		throw new NotImplementedException();
	//	}

	//	#endregion

	//	/// <summary>Pulls out all the values in the structure that are equivalent to the key.</summary>
	//	/// <typeparam name="Key">The type of the key to check for.</typeparam>
	//	/// <param name="key">The key to check for.</param>
	//	/// <param name="compare">Delegate representing comparison technique.</param>
	//	/// <returns>An array containing all the values matching the key or null if non were found.</returns>
	//	//Type[] GetValues<Key>(Key key, Compare<Type, Key> compare);

	//	/// <summary>Pulls out all the values in the structure that are equivalent to the key.</summary>
	//	/// <typeparam name="Key">The type of the key to check for.</typeparam>
	//	/// <param name="key">The key to check for.</param>
	//	/// <param name="compare">Delegate representing comparison technique.</param>
	//	/// <returns>An array containing all the values matching the key or null if non were found.</returns>
	//	/// <param name="values">The values that matched the given key.</param>
	//	/// <returns>true if 1 or more values were found; false if no values were found.</returns>
	//	//bool TryGetValues<Key>(Key key, Compare<Type, Key> compare, out Type[] values);

	//	/// <summary>Checks to see if a given object is in this data structure.</summary>
	//	/// <param name="item">The item to check for.</param>
	//	/// <param name="compare">Delegate representing comparison technique.</param>
	//	/// <returns>true if the item is in this structure; false if not.</returns>
	//	public bool Contains(Type item, Compare<T> compare)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	/// <summary>Checks to see if a given object is in this data structure.</summary>
	//	/// <typeparam name="Key">The type of the key to check for.</typeparam>
	//	/// <param name="key">The key to check for.</param>
	//	/// <param name="compare">Delegate representing comparison technique.</param>
	//	/// <returns>true if the item is in this structure; false if not.</returns>
	//	public bool Contains<Key>(Key key, Compare<Type, Key> compare)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	/// <summary>Invokes a delegate for each entry in the data structure.</summary>
	//	/// <param name="function">The delegate to invoke on each item in the structure.</param>
	//	public void Stepper(Step<T> function)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	/// <summary>Invokes a delegate for each entry in the data structure.</summary>
	//	/// <param name="function">The delegate to invoke on each item in the structure.</param>
	//	public void Stepper(StepRef<T> function)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	/// <summary>Invokes a delegate for each entry in the data structure.</summary>
	//	/// <param name="function">The delegate to invoke on each item in the structure.</param>
	//	/// <returns>The resulting status of the iteration.</returns>
	//	public StepStatus Stepper(StepBreak<T> function)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	/// <summary>Invokes a delegate for each entry in the data structure.</summary>
	//	/// <param name="function">The delegate to invoke on each item in the structure.</param>
	//	/// <returns>The resulting status of the iteration.</returns>
	//	public StepStatus Stepper(StepRefBreak<T> function)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	/// <summary>Creates a shallow clone of this data structure.</summary>
	//	/// <returns>A shallow clone of this data structure.</returns>
	//	public Structure<T> Clone()
	//	{
	//		throw new NotImplementedException();
	//	}

	//	///// <summary>Converts the structure into an array.</summary>
	//	///// <returns>An array containing all the item in the structure.</returns>
	//	//public Type[] ToArray()
	//	//{
	//	//	throw new NotImplementedException();
	//	//}

	//	#endregion

	//	/// <summary>This is used for throwing mutable priority queue exceptions only to make debugging faster.</summary>
	//	internal class HeapArrayDynamicException : System.Exception
	//	{
	//		public HeapArrayDynamicException(string message) : base(message) { }
	//	}
	//}

	#endregion

	#endregion
}