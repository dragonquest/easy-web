using System.Threading.Tasks;

namespace Cms.Log
{
    // Simple Logging interface without context data
    // since its sufficient for this little project.
    public interface ILogger
    {
        void Debug(string message);
        void Info(string message);
        void Warning(string message);
        void Error(string message);
        void Fatal(string message);
    }

    public class Null : ILogger
    {
        public void Debug(string message)
        {
        }

        public void Info(string message)
        {
        }

        public void Warning(string message)
        {
        }

        public void Error(string message)
        {
        }

        public void Fatal(string message)
        {
        }
    }

    public class Console : ILogger
    {
        public void Debug(string message)
        {
            Task.Run(() =>
                    {
                    System.Console.Error.WriteLine("[DEBUG] {0}", message);
                    });
        }

        public void Info(string message)
        {
            Task.Run(() =>
                    {
                    System.Console.Error.WriteLine("[INFO] {0}", message);
                    });
        }

        public void Warning(string message)
        {
            Task.Run(() =>
                    {
                    System.Console.Error.WriteLine("[WARNING] {0}", message);
                    });
        }

        public void Error(string message)
        {
            Task.Run(() =>
                    {
                    System.Console.Error.WriteLine("[ERROR] {0}", message);
                    });
        }

        public void Fatal(string message)
        {
            Task.Run(() =>
                    {
                    System.Console.Error.WriteLine("[FATAL] {0}", message);
                    });
        }
    }
}
