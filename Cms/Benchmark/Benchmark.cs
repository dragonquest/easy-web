using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Cms.Benchmark
{
    // Entry defines a Benchmark Action
    public struct Entry
    {
        public string Name;
        public Action Function;
    }

    public class Runner
    {
        private List<Entry> _list;
        public Runner()
        {
           _list = new List<Entry>();
        }

        public void AddFunc(string name, Action func)
        {
            var entry = new Entry();
            entry.Name = name;
            entry.Function = func;
            _list.Add(entry);
        }

        public List<Measurement> Run(int num)
        {
            var report = new List<Measurement>();

            foreach (var entry in _list)
            {
                report.Add(RunAction(num, entry.Name, entry.Function));
            }
            return report;
        }

        public Measurement RunAction(int num, string name, Action action)
        {
            var results = new Measurement();

            results.Name = name;
            var timer = new Stopwatch();
            for (int i = 0; i < num; i++)
            {
                timer.Reset();
                timer.Start();
                action.Invoke();
                timer.Stop();

                results.Values.Add(timer.Elapsed.TotalMilliseconds);
            }

            return results;
        }
    }

    public class Measurement
    {
        public string Name { get; set; }
        public List<double> Values { get; set; }

        public Measurement()
        {
            Values = new List<double>();
        }

        public int Count {
            get {
                return Values.Count;
            }
        }

        public void Merge(Measurement other)
        {
            if (Name != other.Name)
            {
                return;
            }

            foreach (var entry in other.Values)
            {
                Values.Add(entry);
            }
        }

        public void MergeList(List<Measurement> other)
        {
            foreach(var measurement in other)
            {
                Merge(measurement);
            }
        }
    }

    public class Statistics
    {
        public double Average(Measurement m)
        {
            double res = 0;
            foreach(var val in m.Values)
            {
                res += val;
            }
            return res / (double) m.Count;
        }

        public double Median(Measurement m)
        {
            var sorted = m.Values.OrderBy(x => x).ToArray();
            int mid = sorted.Length / 2;

            if (sorted.Length % 2 == 0)
                return (sorted[mid] + sorted[mid -1]) / 2;

            return sorted[mid];
        }
    }

    public class SimpleConsolePrinter
    {
        public Dictionary<string, Measurement> GroupByName(List<Measurement> measurements)
        {
            var result = new Dictionary<string, Measurement>();

            foreach(var m in measurements)
            {
                if (!result.ContainsKey(m.Name))
                {
                    result[m.Name] = m;
                    continue;
                }

                foreach (var val in m.Values)
                {
                    result[m.Name].Values.Add(val);
                }
            }

            return result;
        }

        public void Print(List<Measurement> measurements)
        {
            var stats = new Statistics();
            var grouped = GroupByName(measurements);

            Console.WriteLine("Iters\t\tAvg\t\t\tMedian\t\t\t\tName");
            foreach (var entry in grouped)
            {
                var iters = entry.Value.Values.Count;
                var avg = stats.Average(entry.Value);
                var median = stats.Median(entry.Value);
                Console.WriteLine("{0:0000}\t\t{1:00.00000} ms/avg\t\t{2:00.00000} ms/median\t\t{3}", iters, avg, median, entry.Key);
            }
            Console.WriteLine(" ");
        }
    }

}
