using System.IO;
using System.Collections.Generic;

namespace Storage
{
    public class MemeFileStorage
    {
        private string _path;

        public MemeFileStorage(string path)
        {
            _path = path;
        }

        // Load loads the template filenames from a given path
        public bool Save(string filename, byte[] data)
        {
            var storagePath = Path.Combine(_path, filename);
            File.WriteAllBytes(storagePath, data);
            return true;
        }

        public byte[] Load(string filename)
        {
            var storagePath = Path.Combine(_path, filename);
            return File.ReadAllBytes(storagePath);
        }

        public bool Exists(string filename)
        {
            var storagePath = Path.Combine(_path, filename);
            return File.Exists(storagePath);
        }

        public List<string> ListAll()
        {
            var memes = new List<string>();
            var files = Directory.GetFiles(_path);

            foreach (var file in files)
            {
                memes.Add(Path.GetFileName(file));
            }

            return memes;
        }
    }
} // namespace Storage
