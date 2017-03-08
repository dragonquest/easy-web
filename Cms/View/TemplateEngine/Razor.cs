using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;

using RazorEngine;
using RazorEngine.Templating;

namespace Cms.View.TemplateEngine
{
    public class Razor : ITemplate
    {
        protected RazorEngine.Templating.IRazorEngineService _service;

        public Razor()
        {
            _service = Engine.Razor;
        }

        public void LoadFromPath(string path, string extension = "*.cshtml")
        {
            string[] files = Directory.GetFiles(path, extension, SearchOption.AllDirectories);

            var templates = new List<string>();
            foreach (string file in files)
            {
                var name = file.Replace(path, "");
                _service.AddTemplate(name, File.ReadAllText(file));
                templates.Add(name);
            }

            foreach (string name in templates)
            {
                _service.Compile(name);
                Console.WriteLine("Compiled {0}", name);
            }
        }

        public string Render(string name, object data)
        {
            return _service.Run(name, null, data);
        }
    }
}
