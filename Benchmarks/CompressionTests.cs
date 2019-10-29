using System.Buffers;
using System.IO;
using System.IO.Compression;
using BenchmarkDotNet.Attributes;
using Universal.Packets;

namespace Benchmarks
{
    [MemoryDiagnoser]
    public class CompressionTests
    {
        public MsgLogin CachedMsg;
        public byte[] CachedArray;

        [GlobalSetup]
        public void Setup()
        {
            CachedMsg = MsgLogin.Create("user", "pass", true, Universal.Packets.Enums.MsgLoginType.Login);
            CachedArray = CachedMsg;
        }   
        [Benchmark]
        public void Brotli_Compress()
        {
            var outBuffer = ArrayPool<byte>.Shared.Rent(500);
            using(var ms = new MemoryStream())
            using(var br = new BrotliStream(ms,CompressionLevel.Fastest))
            {
                br.Write(outBuffer,0,outBuffer.Length);
            }
            ArrayPool<byte>.Shared.Return(outBuffer);      
        }        
        [Benchmark]
        public void Deflate_Compress()
        {
            var outBuffer = ArrayPool<byte>.Shared.Rent(500);
            using(var ms = new MemoryStream())
            using(var br = new DeflateStream(ms,CompressionLevel.Fastest))
            {
                br.Write(outBuffer,0,outBuffer.Length);
            }
            ArrayPool<byte>.Shared.Return(outBuffer);
        }       
        [Benchmark]
        public void Gzip_Compress()
        {
            var outBuffer = ArrayPool<byte>.Shared.Rent(500);
            using(var ms = new MemoryStream())
            using(var br = new GZipStream(ms,CompressionLevel.Fastest))
            {
                br.Write(outBuffer,0,outBuffer.Length);
            }
            ArrayPool<byte>.Shared.Return(outBuffer);
        }
    }
}
