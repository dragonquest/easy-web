using System.IO;
using System.Collections.Generic;

namespace Storage
{
    public class MemeTemplate
    {
        private List<string> _templates;

        public MemeTemplate()
        {
            _templates = new List<string>();
        }

        // Load loads the template filenames from a given path
        public bool Load(string path)
        {
            foreach (string filePath in Directory.GetFiles(path))
            {
                var file = Path.GetFileName(filePath);
                if (file[0] == '.') continue;

                _templates.Add(file);
            }
            return true;
        }

        public List<string> GetTemplates()
        {
            return _templates;
        }
    }
} // namespace Storage
