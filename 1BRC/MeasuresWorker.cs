using System.Collections.Concurrent;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace _1BRC;

public class MeasuresWorker
{
    // private readonly int _bufferLength;
    // [Params(1024,128*128,128*256,256*256,256*512,512*512,1024*1024)]
    public int _bufferLength { get; set; }
    
    private readonly string _measurementFile = "./measurements-1_000_000-sample.txt";
    // ConcurrentDictionary<string, Measures> measures = new ();

    public MeasuresWorker(string measurementFile, int bufferLength = 128*128)
    {
        _measurementFile = measurementFile;
        _bufferLength = bufferLength;
    }
    
    // [Benchmark]
    public async Task Process(ConcurrentDictionary<string, Measures> measures)
    {
        await foreach (string s in ReadFile(_measurementFile))
        {
            (string name, decimal measure) = await ProcessLines2(s);
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
    }
    
    async IAsyncEnumerable<string> ReadFile(string filePath)
    {
        var file = File.OpenRead(filePath);
        byte[] buffer = new byte[_bufferLength];
        Memory<byte> copy = Memory<byte>.Empty;
    
        string pending = String.Empty;
        while (await file.ReadAsync(buffer) > 0)
        {
            copy = new Memory<byte>(buffer);
        
            var s = pending + Encoding.UTF8.GetString(copy.Span);
            var lastIndexOf = s.LastIndexOf('\n');
            lastIndexOf++;
            pending = s[lastIndexOf..];
            yield return s[..lastIndexOf];
        }
    
    }


    async Task<(string, decimal)> ProcessLines2(string s)
    {
        string[] words;
        await foreach (string line in GetSubstring(s, '\n'))
        {
            words = line.Split(";");
            decimal.TryParse(words[1], out var measure);
            return (words[0], measure);
        }

        return (string.Empty, 0);
    }


    async IAsyncEnumerable<string> GetSubstring(string s, char splitter)
    {
        int lastIndex = 0;
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] == splitter)
            {
                yield return s[lastIndex..i];
                lastIndex = i+1;
            }
        }
    }
    
    (string, decimal) ProcessLines(string s)
    {
        var lines = s.Split('\n');
        foreach (var line in lines)
        {
            var words = line.Split(";");
            decimal.TryParse(words[1], out var measure);
            return (words[0], measure);
        }

        return (String.Empty, 0);
    }


}
