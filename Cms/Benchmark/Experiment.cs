using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Cms.Benchmark
{
    /*
    public class Usage
    {
        public void Do()
        {
            int counter = 1;

            var exp = new Experiment();
            exp.AddFunc("for1", () => {
                for(int i = 0; i < 1000; i++)
                {
                    counter += i * 2;
                }
            });

            exp.AddFunc("for2", () => {
                for(int i = 0; i < 1000; i++)
                {
                    counter += i - 2;
                    counter /= 2;
                }
            });

            exp.AddFunc("for3", () => {
                for(int i = 0; i < 1000; i++)
                {
                    counter += i * 2;
                }
            });
            var lab = new Lab();
            lab.Add("route lookup", exp);
            lab.Run("route lookup");
        }

        public void Result()
        {
            var lab = new Lab();
            var report = lab.Get("route lookup").GetReport();
            (new SimpleConsolePrinter()).Print(report);
        }
    }
    */

    public class Scope
    {
        private static Lab _lab;
        private static ConcurrentDictionary<string, bool> _expr;

        public Scope()
        {
            if (_lab == null)
            {
                _lab = new Lab();
                _expr = new ConcurrentDictionary<string, bool>();
            }
        }

        public void Bench(string name, Action action)
        {
            var expr = new Experiment();
            if (!_expr.ContainsKey(name))
            {
                expr.AddFunc(name, action);
                _expr[name] = true;
                _lab.Add(name, expr);
            }

           Task.Run(() =>
           {
             _lab.Run(name);
           });
        }

        public Experiment Get(string name)
        {
            return _lab.Get(name);
        }

        public List<Experiment> GetAll()
        {
            return _lab.GetAll();
        }
    }

    public class Lab
    {
        private static Dictionary<string, Experiment> _experiments;
        private static readonly object _mu = new object();

        public Lab()
        {
            if (_experiments == null)
            {
                _experiments = new Dictionary<string, Experiment>();
            }
        }

        public void Add(string name, Experiment exp)
        {
            lock(_mu)
            {
                if (!_experiments.ContainsKey(name)) {
                    _experiments[name] = exp;
                }
            }
        }

        public void Run(string name)
        {
            lock(_mu)
            {
                if(!_experiments.ContainsKey(name)) return;
                _experiments[name].Run();
            }
        }

        public Experiment Get(string name)
        {
            lock(_mu)
            {
                if (!_experiments.ContainsKey(name))
                {
                    return new Experiment();
                }

                return _experiments[name];
            }
        }

        public List<Experiment> GetAll()
        {
            lock(_mu)
            {
                var exprs = new List<Experiment>();
                foreach (var expr in _experiments)
                {
                   exprs.Add(expr.Value);
                }
                return exprs;
            }
        }
    }

    public class Experiment
    {
        private Runner _runner;
        private List<Runner.ReportEntry> _reports;

        private object _mu = new object();

        public Experiment()
        {
            _runner = new Runner();
            _reports = new List<Runner.ReportEntry>();
        }

        public void AddFunc(string name, Action func)
        {
            lock(_mu)
            {
                _runner.AddFunc(name, func);
            }
        }

        public void Run()
        {
            lock(_mu)
            {
                var newReports = _runner.Run(1);

                if (_reports.Count == 0)
                {
                    _reports = newReports;
                    return;
                }

                for(int i = 0; i < _reports.Count; i++)
                {
                    _reports[i].Merge(newReports);
                }
            }
        }

        public List<Runner.ReportEntry> GetReport()
        {
            lock(_mu)
            {
                return _reports;
            }
        }
    }
}
