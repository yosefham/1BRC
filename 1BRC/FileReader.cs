using BenchmarkDotNet.Attributes;
using Microsoft.Win32.SafeHandles;

namespace _1BRC;

public class FileReader
{
    
    private readonly string _measurementFile = "./measurements-1_000_000-sample.txt";

    [Benchmark]
    public async Task StreamReader()
    {
        byte[] buffer = new byte[512 * 512];
        await using var file = File.OpenRead(_measurementFile);
        while (await file.ReadAsync(buffer) > 0)
        {
                
        }
    }

    [Benchmark]
    public void FileHandle()
    {
        
        byte[] buffer = new byte[512 * 512];
        using SafeFileHandle file = File.OpenHandle(_measurementFile);
        Span<byte> buffer2 = Span<byte>.Empty;
        while (RandomAccess.Read(file, buffer2, buffer.Length) > 0)
        {
                
        }
    }
}