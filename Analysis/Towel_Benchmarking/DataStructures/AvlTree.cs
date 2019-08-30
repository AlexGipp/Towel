﻿using BenchmarkDotNet.Attributes;
using Towel.DataStructures;

namespace Towel_Benchmarking.DataStructures
{
	[Benchmarks(Tag.DataStructures, Tag.AvlTreeLinked)]
	public class AvlTreeLinked_Benchmarks
	{
		[ParamsSource(nameof(AddCounts))]
		public int AddCount { get; set; }
		public int[] AddCounts => BenchmarkSettings.DataStructures.InsertionCounts;

		[Benchmark]
		public void Add()
		{
			IAvlTree<int> avlTree = new AvlTreeLinked<int>();
			int addCount = AddCount;
			for (int i = 0; i < addCount; i++)
			{
				avlTree.Add(i);
			}
		}
	}
}
