using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace Cms.View
{
  public interface ITemplate 
  {
    string Render(string name, object data);
  }

  public class Template : ITemplate
  {
    protected static ReaderWriterLock lock_ = new ReaderWriterLock();

    protected Dictionary<string, string> files_;
    protected Mustache.FormatCompiler compiler_;

    public Template()
    {
      files_ = new Dictionary<string, string>();
      compiler_ = new Mustache.FormatCompiler();
    }

    public void LoadFromPath(string path)
    {
      string[] files = Directory.GetFiles(path, "*.tmpl", SearchOption.AllDirectories);

      foreach (string file in files)
      {
        var name = file.Replace(path, "");
        files_.Add(name, File.ReadAllText(file));
      }
    }

    public string Render(string name, object data)
    {
      if(!files_.ContainsKey(name))
      {
        return "";
      }

      lock_.AcquireReaderLock(Timeout.Infinite);
      try {
        var content = files_[name];
        Mustache.Generator generator = compiler_.Compile(content);
        return generator.Render(data);
      } finally {
        lock_.ReleaseReaderLock();
      }
    }
  }
}