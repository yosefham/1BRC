using System.Diagnostics;
using _1BRC;
using BenchmarkDotNet.Running;

string measurementFile = "../../measurements.txt";
// string measurementFile = "./measurements-1_000_000-sample.txt";

// await new CreateMeasurements().CreateMesurementsFile(measurementFile, 1_000_000_000);
// return;

// BenchmarkRunner.Run<FileReader>();
// BenchmarkRunner.Run<MeasuresWorker>();
// return;

var sw = new Stopwatch();
var worker = new MeasuresWorker
{
    MeasurementFile = measurementFile
};

sw.Start();
await worker.Process();
sw.Stop();

Console.WriteLine(sw.Elapsed);
