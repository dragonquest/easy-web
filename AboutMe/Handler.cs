using System;
using System.Net;
using System.IO;

using Cms.Net.Http;
using Cms.Benchmark;

namespace AboutMe.Handler {
    public class WebsiteController
    {
        protected Cms.View.ITemplate _template;

        public WebsiteController(Cms.View.ITemplate template)
        {
            _template = template;
        }

        public void IndexPage(IResponseWriter response, IRequest request, IUrlParams urlParams)
        {
            response.WriteString(_template.Render("index.tmpl", null));
        }
    }

    public class ExitApp : IHandler
    {
        public void ServeHttp(IResponseWriter response, IRequest request, IUrlParams urlParams)
        {
            var scoped = new Scope();
            Console.WriteLine("Found Scoped Benchs: {0}", scoped.GetAll().Count);
            (new SimpleConsolePrinter()).Print(scoped.GetMeasurements());
            Environment.Exit(0);
        }
    }

    public class Website : IHandler
    {
        public void ServeHttp(IResponseWriter response, IRequest request, IUrlParams urlParams)
        {
            response.WriteString("<html><head><meta charset=\"UTF-8\"></head><body><h1>Hello "+urlParams.Get("name")+"</h1></body></html>");
        }
    }

    public class AssetsHandler : IHandler
    {
        static readonly object _locker = new object();
        protected string _assetsDir;

        public AssetsHandler(string assetsDir)
        {
            _assetsDir = assetsDir;
        }

        public void ServeHttp(IResponseWriter response, IRequest request, IUrlParams urlParams)
        {
            lock(_locker) {
                string fileName = _assetsDir + request.Url.AbsolutePath;
                response.Headers().Set("Cache-Control", "max-age=2592000"); // 30 days default

                if (File.Exists(fileName))
                {
                    BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open));
                    byte[] allData = reader.ReadBytes(int.MaxValue);
                    reader.Close();
                    response.Write(allData);
                    return;
                }

                response.WriteString("<html><head><meta charset=\"UTF-8\"></head><body><h1>File "+fileName+"</h1></body></html>");
            } // lock(_locker)
        }
    }

    public class NotFound : IHandler
    {
        public void ServeHttp(IResponseWriter response, IRequest request, IUrlParams urlParams)
        {
            response.WriteString("<html><body><h1>Website not found</h1></body></html>");
        }
    }
}
