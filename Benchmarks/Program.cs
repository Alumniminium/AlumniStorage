using System;
using BenchmarkDotNet.Running;

namespace Benchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<AllocTests>();
            Console.WriteLine(summary);
        }
    }
}
