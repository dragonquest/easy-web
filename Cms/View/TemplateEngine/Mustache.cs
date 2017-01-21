using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace Cms.View.TemplateEngine
{
    // XXX: MustacheTmpl is called with the prefix Tmpl
    //      because it would conflict with the dependency namespace Mustache.
    public class MustacheTmpl : ITemplate
    {
        protected static ReaderWriterLock _lock = new ReaderWriterLock();

        protected Dictionary<string, string> _files;
        protected Mustache.FormatCompiler _compiler;

        public MustacheTmpl()
        {
            _files = new Dictionary<string, string>();
            _compiler = new Mustache.FormatCompiler();
        }

        public void LoadFromPath(string path)
        {
            string[] files = Directory.GetFiles(path, "*.tmpl", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                var name = file.Replace(path, "");
                _files.Add(name, File.ReadAllText(file));
            }
        }

        public string Render(string name, object data)
        {
            if(!_files.ContainsKey(name))
            {
                return "";
            }

            _lock.AcquireReaderLock(Timeout.Infinite);
            try {
                var content = _files[name];
                Mustache.Generator generator = _compiler.Compile(content);
                return generator.Render(data);
            } finally {
                _lock.ReleaseReaderLock();
            }
        }
    }
}
