using System;
using System.Net;
using System.IO;

using Cms.Net.Http;
using Cms.Benchmark;

namespace AboutMe.Handler {
    public class WebsiteController
    {
        protected Cms.View.ITemplate template_;

        public WebsiteController(Cms.View.ITemplate template)
        {
            template_ = template;
        }

        public void IndexPage(IResponseWriter response, IRequest request, IUrlParams urlParams)
        {
            response.WriteString(template_.Render("index.tmpl", null));
        }
    }

    public class ExitApp : IHandler
    {
        public void ServeHttp(IResponseWriter response, IRequest request, IUrlParams urlParams)
        {
            var scoped = new Scope();
            foreach (var expr in scoped.GetAll())
            {
                var report = expr.GetReport();
                (new SimpleConsolePrinter()).Print(report);
            }
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
        static readonly object locker_ = new object();
        protected string assetsDir_;

        public AssetsHandler(string assetsDir)
        {
            assetsDir_ = assetsDir;
        }

        public void ServeHttp(IResponseWriter response, IRequest request, IUrlParams urlParams)
        {
            lock(locker_) {
                string fileName = assetsDir_ + request.Url.AbsolutePath;
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
            } // lock(locker_)
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
