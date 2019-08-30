﻿using System;
using Towel.DataStructures;
using Towel.Mathematics;

namespace Towel.Algorithms
{
	/// <summary>Static class taht constains algorithms for searching.</summary>
	public static class Search
	{
		#region Binary

		/// <summary>Performs a binary search to find the index where a specific value fits in indexed, sorted items.</summary>
		/// <param name="get">Indexer delegate.</param>
		/// <param name="length">The number of indexed items.</param>
		/// <param name="compare">Comparison delegate.</param>
		/// <returns>The index where the specific value fits into the index, sorted items.</returns>
		public static int Binary<T>(GetIndex<T> get, int length, CompareToKnownValue<T> compare)
		{
			if (get is null)
			{
				throw new ArgumentNullException(nameof(get));
			}
			if (compare is null)
			{
				throw new ArgumentNullException(nameof(compare));
			}
			if (length <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(length), length, "!(" + nameof(length) + " > 0)");
			}
			return Binary(get, 0, length, compare);
		}

		private static int Binary<T>(GetIndex<T> get, int index, int length, CompareToKnownValue<T> compare)
		{
			int low = index;
			int hi = index + length - 1;
			while (low <= hi)
			{
				int median = low + (hi - low >> 1);
				switch (compare(get(median)))
				{
					case CompareResult.Equal:
						return median;
					case CompareResult.Less:
						low = median + 1;
						break;
					case CompareResult.Greater:
						hi = median - 1;
						break;
					default:
						throw new NotImplementedException();
				}
			}
			return ~low;
		}

		#endregion

		#region Graph Algorithms

		#region Delegates

		/// <summary>Step function for all neigbors of a given node.</summary>
		/// <param name="current">The node to step through all the neighbors of.</param>
		/// <param name="neighbors">Step function to perform on all neighbors.</param>
		public delegate void Neighbors<NODE>(NODE current, Step<NODE> neighbors);
		/// <summary>Computes the heuristic value of a given node in a graph (smaller values mean closer to goal node).</summary>
		/// <param name="node">The node to compute the heuristic value of.</param>
		/// <returns>The computed heuristic value for this node.</returns>
		public delegate NUMERIC Heuristic<NODE, NUMERIC>(NODE node);
		/// <summary>Computes the cost of moving from the current node to a specific neighbor.</summary>
		/// <param name="current">The current (starting) node.</param>
		/// <param name="neighbor">The node to compute the cost of movign to.</param>
		/// <returns>The computed cost value of movign from current to neighbor.</returns>
		public delegate NUMERIC Cost<NODE, NUMERIC>(NODE current, NODE neighbor);
		/// <summary>Predicate for determining if we have reached the goal node.</summary>
		/// <param name="current">The current node.</param>
		/// <returns>True if the current node is a/the goal node; False if not.</returns>
		public delegate bool Goal<NODE>(NODE current);

		#endregion

		#region Classes

		internal class GreedyNode<NODE, NUMERIC>
		{
			internal readonly GreedyNode<NODE, NUMERIC> Previous;
			internal readonly NODE Value;
			internal readonly NUMERIC Priority;

			internal GreedyNode(GreedyNode<NODE, NUMERIC> previous, NODE value, NUMERIC priority)
			{
				this.Previous = previous;
				this.Value = value;
				this.Priority = priority;
			}
		}

		internal class AstarNode<NODE, NUMERIC>
		{
			internal readonly AstarNode<NODE, NUMERIC> Previous;
			internal readonly NODE Value;
			internal readonly NUMERIC Priority;
			internal readonly NUMERIC Cost;

			internal AstarNode(AstarNode<NODE, NUMERIC> previous, NODE value, NUMERIC priority, NUMERIC cost)
			{
				this.Previous = previous;
				this.Value = value;
				this.Priority = priority;
				this.Cost = cost;
			}
		}

		internal class PathNode<NODE>
		{
			internal readonly NODE Value;
			internal PathNode<NODE> Next;

			internal PathNode(NODE value)
			{
				this.Value = value;
			}
		}

		#endregion

		#region Internal Fuctions

		internal static Stepper<NODE> BuildPath<NODE, NUMERIC>(GreedyNode<NODE, NUMERIC> node)
		{
			PathNode<NODE> start = BuildPath(node, out PathNode<NODE> end);
			return (Step<NODE> step) =>
			{
				PathNode<NODE> current = start;
				while (current != null)
				{
					step(current.Value);
					current = current.Next;
				}
			};
		}

		internal static Stepper<NODE> BuildPath<NODE, NUMERIC>(AstarNode<NODE, NUMERIC> node)
		{
			PathNode<NODE> start = BuildPath(node, out PathNode<NODE> end);
			return (Step<NODE> step) =>
			{
				PathNode<NODE> current = start;
				while (current != null)
				{
					step(current.Value);
					current = current.Next;
				}
			};
		}

		internal static PathNode<NODE> BuildPath<NODE, NUMERIC>(GreedyNode<NODE, NUMERIC> currentNode, out PathNode<NODE> currentPathNode)
		{
			if (currentNode.Previous == null)
			{
				PathNode<NODE> start = new PathNode<NODE>(currentNode.Value);
				currentPathNode = start;
				return start;
			}
			else
			{
				PathNode<NODE> start = BuildPath(currentNode.Previous, out PathNode<NODE> previous);
				currentPathNode = new PathNode<NODE>(currentNode.Value);
				previous.Next = currentPathNode;
				return start;
			}
		}

		internal static PathNode<NODE> BuildPath<NODE, NUMERIC>(AstarNode<NODE, NUMERIC> currentNode, out PathNode<NODE> currentPathNode)
		{
			if (currentNode.Previous == null)
			{
				PathNode<NODE> start = new PathNode<NODE>(currentNode.Value);
				currentPathNode = start;
				return start;
			}
			else
			{
				PathNode<NODE> start = BuildPath(currentNode.Previous, out PathNode<NODE> previous);
				currentPathNode = new PathNode<NODE>(currentNode.Value);
				previous.Next = currentPathNode;
				return start;
			}
		}

		#endregion

		/// <summary>Runs the A* search algorithm algorithm on a graph.</summary>
		/// <param name="start">The node to start at.</param>
		/// <param name="neighbors">Step function for all neigbors of a given node.</param>
		/// <param name="heuristic">Computes the heuristic value of a given node in a graph.</param>
		/// <param name="cost">Computes the cost of moving from the current node to a specific neighbor.</param>
		/// <param name="goal">Predicate for determining if we have reached the goal node.</param>
		/// <returns>Stepper of the shortest path or null if no path exists.</returns>
		public static Stepper<NODE> Graph<NODE, NUMERIC>(NODE start, Neighbors<NODE> neighbors, Heuristic<NODE, NUMERIC> heuristic, Cost<NODE, NUMERIC> cost, Goal<NODE> goal)
		{
			// using a heap (aka priority queue) to store nodes based on their computed A* f(n) value
			IHeap<AstarNode<NODE, NUMERIC>> fringe = new HeapArray<AstarNode<NODE, NUMERIC>>(
				// NOTE: Typical A* implementations prioritize smaller values
				(a, b) => Compute.Compare(b.Priority, a.Priority));

			// push starting node
			fringe.Enqueue(
				new AstarNode<NODE, NUMERIC>(
					null,
					start,
					default(NUMERIC),
					Constant<NUMERIC>.Zero));

			// run the algorithm
			while (fringe.Count != 0)
			{
				AstarNode<NODE, NUMERIC> current = fringe.Dequeue();
				if (goal(current.Value))
				{
					return BuildPath(current);
				}
				else
				{
					neighbors(current.Value,
						(NODE neighbor) =>
						{
							NUMERIC costValue = Compute.Add(current.Cost, cost(current.Value, neighbor));
							fringe.Enqueue(
								new AstarNode<NODE, NUMERIC>(
									current,
									neighbor,
									Compute.Add(heuristic(neighbor), costValue),
									costValue));
						});
				}
			}
			return null; // goal node was not reached (no path exists)
		}

		#region A* overloads

		/// <summary>Runs the A* search algorithm algorithm on a graph.</summary>
		/// <param name="start">The node to start at.</param>
		/// <param name="graph">The graph to perform the search on.</param>
		/// <param name="heuristic">Computes the heuristic value of a given node in a graph.</param>
		/// <param name="cost">Computes the cost of moving from the current node to a specific neighbor.</param>
		/// <param name="goal">Predicate for determining if we have reached the goal node.</param>
		/// <returns>Stepper of the shortest path or null if no path exists.</returns>
		public static Stepper<NODE> Graph<NODE, NUMERIC>(NODE start, IGraph<NODE> graph, Heuristic<NODE, NUMERIC> heuristic, Cost<NODE, NUMERIC> cost, Goal<NODE> goal)
		{
			return Graph(start, graph.Neighbors, heuristic, cost, goal);
		}

		/// <summary>Runs the A* search algorithm algorithm on a graph.</summary>
		/// <param name="start">The node to start at.</param>
		/// <param name="neighbors">Delegate for gettign the neighbors of a node.</param>
		/// <param name="heuristic">Computes the heuristic value of a given node in a graph.</param>
		/// <param name="cost">Computes the cost of moving from the current node to a specific neighbor.</param>
		/// <param name="goal">The goal node.</param>
		/// <returns>Stepper of the shortest path or null if no path exists.</returns>
		public static Stepper<NODE> Graph<NODE, NUMERIC>(NODE start, Neighbors<NODE> neighbors, Heuristic<NODE, NUMERIC> heuristic, Cost<NODE, NUMERIC> cost, NODE goal)
		{
			return Graph(start, neighbors, heuristic, cost, goal, Equate.Default);
		}

		/// <summary>Runs the A* search algorithm algorithm on a graph.</summary>
		/// <param name="start">The node to start at.</param>
		/// <param name="neighbors">Delegate for gettign the neighbors of a node.</param>
		/// <param name="heuristic">Computes the heuristic value of a given node in a graph.</param>
		/// <param name="cost">Computes the cost of moving from the current node to a specific neighbor.</param>
		/// <param name="goal">The goal node.</param>
		/// <param name="equate">A delegate for checking for equality between two nodes.</param>
		/// <returns>Stepper of the shortest path or null if no path exists.</returns>
		public static Stepper<NODE> Graph<NODE, NUMERIC>(NODE start, Neighbors<NODE> neighbors, Heuristic<NODE, NUMERIC> heuristic, Cost<NODE, NUMERIC> cost, NODE goal, Equate<NODE> equate)
		{
			return Graph(start, neighbors, heuristic, cost, (NODE node) => { return equate(node, goal); });
		}

		/// <summary>Runs the A* search algorithm algorithm on a graph.</summary>
		/// <param name="start">The node to start at.</param>
		/// <param name="graph">The graph to perform the search on.</param>
		/// <param name="heuristic">Computes the heuristic value of a given node in a graph.</param>
		/// <param name="cost">Computes the cost of moving from the current node to a specific neighbor.</param>
		/// <param name="goal">The goal node.</param>
		/// <returns>Stepper of the shortest path or null if no path exists.</returns>
		public static Stepper<NODE> Graph<NODE, NUMERIC>(NODE start, IGraph<NODE> graph, Heuristic<NODE, NUMERIC> heuristic, Cost<NODE, NUMERIC> cost, NODE goal)
		{
			return Graph(start, graph, heuristic, cost, goal, Equate.Default);
		}

		/// <summary>Runs the A* search algorithm algorithm on a graph.</summary>
		/// <param name="start">The node to start at.</param>
		/// <param name="graph">The graph to perform the search on.</param>
		/// <param name="heuristic">Computes the heuristic value of a given node in a graph.</param>
		/// <param name="cost">Computes the cost of moving from the current node to a specific neighbor.</param>
		/// <param name="goal">The goal node.</param>
		/// <param name="equate">A delegate for checking for equality between two nodes.</param>
		/// <returns>Stepper of the shortest path or null if no path exists.</returns>
		public static Stepper<NODE> Graph<NODE, NUMERIC>(NODE start, IGraph<NODE> graph, Heuristic<NODE, NUMERIC> heuristic, Cost<NODE, NUMERIC> cost, NODE goal, Equate<NODE> equate)
		{
			return Graph(start, graph.Neighbors, heuristic, cost, (NODE node) => { return equate(node, goal); });
		}

		#endregion

		/// <summary>Runs the Greedy search algorithm algorithm on a graph.</summary>
		/// <param name="start">The node to start at.</param>
		/// <param name="neighbors">Step function for all neigbors of a given node.</param>
		/// <param name="heuristic">Computes the heuristic value of a given node in a graph.</param>
		/// <param name="goal">Predicate for determining if we have reached the goal node.</param>
		/// <returns>Stepper of the shortest path or null if no path exists.</returns>
		public static Stepper<NODE> Graph<NODE, NUMERIC>(NODE start, Neighbors<NODE> neighbors, Heuristic<NODE, NUMERIC> heuristic, Goal<NODE> goal)
		{
			// using a heap (aka priority queue) to store nodes based on their computed heuristic value
			IHeap<GreedyNode<NODE, NUMERIC>> fringe = new HeapArray<GreedyNode<NODE, NUMERIC>>(
				// NOTE: Typical graph search implementations prioritize smaller values
				(a, b) => Compute.Compare(b.Priority, a.Priority));

			// push starting node
			fringe.Enqueue(
				new GreedyNode<NODE, NUMERIC>(
					null,
					start,
					default(NUMERIC)));

			// run the algorithm
			while (fringe.Count != 0)
			{
				GreedyNode<NODE, NUMERIC> current = fringe.Dequeue();
				if (goal(current.Value))
				{
					return BuildPath(current);
				}
				else
				{
					neighbors(current.Value,
						(NODE neighbor) =>
						{
							fringe.Enqueue(
								new GreedyNode<NODE, NUMERIC>(
									current,
									neighbor,
									heuristic(neighbor)));
						});
				}
			}
			return null; // goal node was not reached (no path exists)
		}

		#region Greedy Overloads

		/// <summary>Runs the Greedy search algorithm algorithm on a graph.</summary>
		/// <param name="start">The node to start at.</param>
		/// <param name="neighbors">Step function for all neigbors of a given node.</param>
		/// <param name="heuristic">Computes the heuristic value of a given node in a graph.</param>
		/// <param name="goal">Predicate for determining if we have reached the goal node.</param>
		/// <returns>Stepper of the shortest path or null if no path exists.</returns>
		public static Stepper<NODE> Graph<NODE, NUMERIC>(NODE start, Neighbors<NODE> neighbors, Heuristic<NODE, NUMERIC> heuristic, NODE goal)
		{
			return Graph(start, neighbors, heuristic, goal, Equate.Default);
		}

		/// <summary>Runs the Greedy search algorithm algorithm on a graph.</summary>
		/// <param name="start">The node to start at.</param>
		/// <param name="neighbors">Step function for all neigbors of a given node.</param>
		/// <param name="heuristic">Computes the heuristic value of a given node in a graph.</param>
		/// <param name="goal">Predicate for determining if we have reached the goal node.</param>
		/// <param name="equate">Delegate for checking for equality between two nodes.</param>
		/// <returns>Stepper of the shortest path or null if no path exists.</returns>
		public static Stepper<NODE> Graph<NODE, NUMERIC>(NODE start, Neighbors<NODE> neighbors, Heuristic<NODE, NUMERIC> heuristic, NODE goal, Equate<NODE> equate)
		{
			return Graph(start, neighbors, heuristic, (NODE node) => { return equate(node, goal); });
		}

		/// <summary>Runs the Greedy search algorithm algorithm on a graph.</summary>
		/// <param name="start">The node to start at.</param>
		/// <param name="graph">The graph to search against.</param>
		/// <param name="heuristic">Computes the heuristic value of a given node in a graph.</param>
		/// <param name="goal">Predicate for determining if we have reached the goal node.</param>
		/// <returns>Stepper of the shortest path or null if no path exists.</returns>
		public static Stepper<NODE> Graph<NODE, NUMERIC>(NODE start, IGraph<NODE> graph, Heuristic<NODE, NUMERIC> heuristic, NODE goal)
		{
			return Graph(start, graph, heuristic, goal, Equate.Default);
		}

		/// <summary>Runs the Greedy search algorithm algorithm on a graph.</summary>
		/// <param name="start">The node to start at.</param>
		/// <param name="graph">The graph to search against.</param>
		/// <param name="heuristic">Computes the heuristic value of a given node in a graph.</param>
		/// <param name="goal">Predicate for determining if we have reached the goal node.</param>
		/// <param name="equate">Delegate for checking for equality between two nodes.</param>
		/// <returns>Stepper of the shortest path or null if no path exists.</returns>
		public static Stepper<NODE> Graph<NODE, NUMERIC>(NODE start, IGraph<NODE> graph, Heuristic<NODE, NUMERIC> heuristic, NODE goal, Equate<NODE> equate)
		{
			return Graph(start, graph.Neighbors, heuristic, (NODE node) => { return equate(node, goal); });
		}

		/// <summary>Runs the Greedy search algorithm algorithm on a graph.</summary>
		/// <param name="start">The node to start at.</param>
		/// <param name="graph">The graph to search against.</param>
		/// <param name="heuristic">Computes the heuristic value of a given node in a graph.</param>
		/// <param name="goal">Predicate for determining if we have reached the goal node.</param>
		/// <returns>Stepper of the shortest path or null if no path exists.</returns>
		public static Stepper<NODE> Graph<NODE, NUMERIC>(NODE start, IGraph<NODE> graph, Heuristic<NODE, NUMERIC> heuristic, Goal<NODE> goal)
		{
			return Graph(start, graph.Neighbors, heuristic, goal);
		}

		#endregion

		#endregion
	}
}
