using System.Collections.Concurrent;
using System.Text;
using BenchmarkDotNet.Attributes;
using Microsoft.Win32.SafeHandles;

namespace _1BRC;

[SimpleJob(launchCount:0, warmupCount:0, iterationCount:5)]
public  class MeasuresWorker
{
    // private readonly int _bufferLength;
    [Params(128 * 128, 128 * 256, 256 * 256, 256 * 512, 512 * 512)]
    public int BufferLength { get; set; } = 256 * 265;

    public string MeasurementFile { get; set; } = "C:/source/Personal/1BRC/1BRC/bin/measurements.txt";
    ConcurrentDictionary<string, Measures> measures = new ();


    [Benchmark]
    public async Task Process()
    {
        await foreach (Memory<byte> s in ReadBuffer(MeasurementFile).ConfigureAwait(false))
        {
            (Memory<byte> byteName, decimal measure) = await ProcessLines(s);
            string name = Encoding.UTF8.GetString(byteName.Span);
            measures.AddOrUpdate(name,
                _ => new Measures(measure, (long)measure, measure, 1),
                (_, m) =>
                {
                    var min = measure < m.Min ? measure : m.Min;
                    var max = measure > m.Max ? measure : m.Max;
                    var sum = m.Sum + measure;
                    var count = m.Count + 1;
                    return new Measures(min, (long)sum, max, count);
                });
        }

        foreach (var measure in measures)
        {
            var val = measure.Value;
            Console.WriteLine($"{measure.Key}={val.Min}/{val.Sum / val.Count}/{val.Max}");
        }
    }

        async IAsyncEnumerable<Memory<byte>> ReadBuffer(string filePath)
        {
            byte[] buffer = new byte[BufferLength];
            using SafeFileHandle file = File.OpenHandle(MeasurementFile);
            long index = 0;
            while (RandomAccess.Read(file, buffer, index) > 0)
            {
                var lastIndexOf = buffer.AsSpan().LastIndexOf((byte)'\n') + 1; 
                index += lastIndexOf;
                yield return new Memory<byte>(buffer[..lastIndexOf]);
            }
        }
    

    async Task<(Memory<byte>, decimal)> ProcessLines(Memory<byte> s)
    {
        await foreach (var line in GetSubstring(s,(byte)'\n').ConfigureAwait(false))
        {
            var indexOfSplitter = line.Span.LastIndexOf((byte)';');
            decimal.TryParse(line.Span[(indexOfSplitter+1)..], out var measure);
            return (line[..indexOfSplitter], measure);
        }

        return (Memory<byte>.Empty, 0);
    }


    async IAsyncEnumerable<Memory<byte>> GetSubstring(Memory<byte> s, byte splitter)
    {
        int lastIndex = 0;
        int nextIndex = 0; 
        for (int i = 0; i < s.Span.Length; i++)
        {
            if (s.Span[i] == splitter)
            {
                lastIndex = nextIndex;
                nextIndex = i + 1;
                yield return s[lastIndex..i];
            }
        }
    }

}
public record Measures(decimal Min, long Sum, decimal Max, int Count);
