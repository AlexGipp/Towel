﻿using BenchmarkDotNet.Attributes;
using Towel.DataStructures;

namespace Towel_Benchmarking.DataStructures
{
	[Benchmarks(Tag.DataStructures, Tag.FirstInLastOutArray)]
	public class FirstInLastOutArray_Benchmarks
	{
		[ParamsSource(nameof(PushCounts))]
		public int PushCount { get; set; }

		public int[] PushCounts => BenchmarkSettings.DataStructures.InsertionCounts;

		[Benchmark]
		public void Push()
		{
			IStack<int> stack = new StackArray<int>();
			int pushCount = PushCount;
			for (int i = 0; i < pushCount; i++)
			{
				stack.Push(i);
			}
		}

		[Benchmark]
		public void PushWithCapacity()
		{
			IStack<int> stack = new StackArray<int>(PushCount);
			int pushCount = PushCount;
			for (int i = 0; i < pushCount; i++)
			{
				stack.Push(i);
			}
		}
	}

	[Benchmarks(Tag.DataStructures, Tag.FirstInLastOutLinked)]
	public class FirstInLastOutLinked_Benchmarks
	{
		[ParamsSource(nameof(PushCounts))]
		public int PushCount { get; set; }

		public int[] PushCounts => BenchmarkSettings.DataStructures.InsertionCounts;

		[Benchmark]
		public void Push()
		{
			IStack<int> stack = new StackLinked<int>();
			int pushCount = PushCount;
			for (int i = 0; i < pushCount; i++)
			{
				stack.Push(i);
			}
		}
	}
}
