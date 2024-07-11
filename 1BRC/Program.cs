using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using _1BRC;

// string measurementFile = "./measurements.txt";
string measurementFile = "./measurements-1_000_000-sample.txt";
//
// new CreateMeasurements().CreateMesurementsFile(measurementFile, 1_000_000_000);
// return;

ConcurrentDictionary<string, Measures> measures = new ConcurrentDictionary<string, Measures>();


var sw = new Stopwatch();
sw.Start();


async IAsyncEnumerable<string> ReadFile(string filePath)
{
    var file = File.OpenRead(measurementFile);
    byte[] buffer = new byte[1024*1024];
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


await foreach (string s in ReadFile(measurementFile))
{
    (string name, decimal measure) = ProcessLines(s);
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

// await Parallel.ForEachAsync(ReadFile(measurementFile), async (s, _) =>
// {
//     (string name, decimal measure) = ProcessLines(s);
//     measures.AddOrUpdate(name,
//         _ => new Measures(measure, (long)measure, measure, 1),
//         (_, m) =>
//         {
//             var min = measure < m.Min ? measure : m.Min;
//             var max = measure > m.Max ? measure : m.Max;
//             var sum = m.Sum + measure;
//             var count = m.Count + 1;
//             return new Measures(min, (long)sum, max, count);
//         });
// });


foreach (var measure in measures)
{
    var val = measure.Value;
    Console.WriteLine($"{measure.Key}={val.Min}/{val.Sum/val.Count}/{val.Max}");
}

sw.Stop();
Console.WriteLine(sw.Elapsed);

public record Measures(decimal Min, long Sum, decimal Max, int Count);