namespace Benchmarks
{
    public unsafe struct MyStruct
    {
        public fixed byte FixedSource[100_000];
    }
}