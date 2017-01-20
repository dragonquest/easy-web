using System;
using System.Collections.Generic;
using System.Diagnostics;

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

        public List<ReportEntry> Run(int num)
        {
            var report = new List<ReportEntry>();

            foreach (var entry in _list)
            {
                report.Add(RunAction(num, entry.Name, entry.Function));
            }
            return report;
        }

        public ReportEntry RunAction(int num, string name, Action action)
        {
            var allocBytes = GetMemoryBytes();
            var handleCount = GetHandleCount();

            var timer = new Stopwatch();
            for (int i = 0; i < num; i++)
            {
                timer.Start();
                action.Invoke();
                timer.Stop();
            }
            var allocBytesEnd = GetMemoryBytes();
            var handleCountEnd = GetHandleCount();

            var reportEntry = new ReportEntry
            {
                Name = name,
                Average = ((decimal)timer.Elapsed.TotalMilliseconds) / (decimal)num,

                AllocBytes = (allocBytesEnd - allocBytes) / (decimal)num,
                HandleCount = (handleCountEnd - handleCount) / num,

                Iterations = num,
            };

            return reportEntry;
        }

        public class ReportEntry
        {
            public string Name { get; set; }
            public decimal Average { get; set; }

			public decimal AllocBytes { get; set; }
			public int HandleCount { get; set; }

			public int Iterations { get; set; }

            public void Merge(List<ReportEntry> others)
            {
                for(int i = 0; i < others.Count; i++)
                {
                    if(Name != others[i].Name) continue;

                    Average = (Average + others[i].Average) / (decimal)2.0;
                    AllocBytes = (AllocBytes + others[i].AllocBytes) / (decimal)2.0;
                    HandleCount = (HandleCount + others[i].HandleCount) / 2;
                    Iterations += others[i].Iterations;
                }
            }
        }

		public decimal GetMemoryBytes()
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			return (decimal)GC.GetTotalMemory(true);
		}

		public int GetHandleCount()
		{
			return System.Diagnostics.Process.GetCurrentProcess().HandleCount;
		}
    }

    public class SimpleConsolePrinter
    {
        public void Print(List<Runner.ReportEntry> entries)
        {
            if (entries.Count < 1)
            {
                Console.Write("Error: At least one ReportEntry is needed.");
                return;
            }

            decimal totalMs = 0;
            foreach (var entry in entries)
            {
                totalMs += entry.Average;
            }

            Console.WriteLine(" ");
            Console.WriteLine("{0} Iterations: ", entries[0].Iterations);
            Console.WriteLine("{0}", new String('*', 80));
            foreach (var entry in entries)
            {
                decimal perc = (100.0m / totalMs) * entry.Average;
				var allocBytes = entry.AllocBytes;
				if (allocBytes <= 0) 
				{
					allocBytes = 0;
				}
                Console.WriteLine("{0:00.00000} ms/avg\t\t{1:00.00} %\t\t{2:0000.000.000} bytes\t\t{3}", entry.Average, perc, allocBytes, entry.Name);
            }
            Console.WriteLine(" ");
        }
    }

}
