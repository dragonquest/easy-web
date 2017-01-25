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
            for(int x=0;x < 1000;x++) {
                (new Scope()).Bench("test1", () =>
                {
                    int res=0;
                    for(int i=0; i<100;i++)
                    {
                        res += i;
                    }
                });
                (new Scope()).Bench("test2", () =>
                {
                    int res=0;
                    for(int i=0; i<100;i++)
                    {
                        res += (i * 2 * 3) - i;
                    }
                });
            };
            response.WriteString(_template.Render("index.tmpl", null));
        }

        public void BenchPage(IResponseWriter response, IRequest request, IUrlParams urlParams)
        {
            var report = (new Scope()).GetMeasurements();
            (new SimpleConsolePrinter()).Print(report);
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
