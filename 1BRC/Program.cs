using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using _1BRC;using BenchmarkDotNet.Running;

string measurementFile = "./measurements.txt";
// string measurementFile = "./measurements-1_000_000-sample.txt";
//
// new CreateMeasurements().CreateMesurementsFile(measurementFile, 1_000_000_000);
// return;

ConcurrentDictionary<string, Measures> measures = new ConcurrentDictionary<string, Measures>();

// BenchmarkRunner.Run<MeasuresWorker>();
// return;

var sw = new Stopwatch();
var worker = new MeasuresWorker(measurementFile);

sw.Start();
await worker.Process(measures);


foreach (var measure in measures)
{
    var val = measure.Value;
    Console.WriteLine($"{measure.Key}={val.Min}/{val.Sum/val.Count}/{val.Max}");
}

sw.Stop();
Console.WriteLine(sw.Elapsed);

public record Measures(decimal Min, long Sum, decimal Max, int Count);