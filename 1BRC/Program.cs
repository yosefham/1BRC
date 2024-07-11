using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using _1BRC;

string measurementFile = "./measurements.txt";
//
// new CreateMeasurements().CreateMesurementsFile(measurementFile, 1_000_000_000);
// return;

var sw = new Stopwatch();
sw.Start();
var file = File.OpenRead(measurementFile);
int index = 0;
byte[] buffer = new byte[1024*1024];
ConcurrentDictionary<string, Measures> measures = new ConcurrentDictionary<string, Measures>();

var pending = string.Empty;
while ((await file.ReadAsync(buffer)) > 0)
{
    var s = pending + Encoding.UTF8.GetString(buffer);
    var lines = s.Split('\n');
    
    foreach (var line in lines) 
    {
        var words = line.Split(";");
        if (words?.Length < 2)
            pending = line;
        else
        {
            if (!decimal.TryParse(words[1], out var measure))
                pending = line;
            else
            {
                index++;
                var name = words[0];

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
    }
}

foreach (var measure in measures)
{
    var val = measure.Value;
    Console.WriteLine($"{measure.Key}={val.Min}/{val.Sum/val.Count}/{val.Max}");
}

sw.Stop();
Console.WriteLine(sw.Elapsed);
Console.WriteLine($"Lines processed: {index}");

public record Measures(decimal Min, long Sum, decimal Max, int Count);