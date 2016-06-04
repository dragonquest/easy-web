using System.Net;
using System.IO;

using Cms.Net.Http;

namespace AboutMe.Handler {
	public class WebsiteController
	{
		protected Cms.View.ITemplate template_;

		public WebsiteController(Cms.View.ITemplate template)
		{
			template_ = template;
		}

		public void IndexPage(HttpListenerResponse response, HttpListenerRequest request, IUrlParams urlParams)
		{
			var resp = new ResponseWriter(response);
			resp.WriteString(template_.Render("index.tmpl", null));
		}
	}

	public class Website : IHandler
	{
		public void ServeHttp(HttpListenerResponse response, HttpListenerRequest request, IUrlParams urlParams)
		{
			var resp = new ResponseWriter(response);
			resp.WriteString("<html><head><meta charset=\"UTF-8\"></head><body><h1>Hello "+urlParams.Get("name")+"</h1></body></html>");
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

		public void ServeHttp(HttpListenerResponse response, HttpListenerRequest request, IUrlParams urlParams)
		{
			lock(locker_) {
				var fileName = Path.Combine(assetsDir_, urlParams.Get("file"));

				response.Headers.Set("Cache-Control", "max-age=2592000"); // 30 days default

				var resp = new ResponseWriter(response);
				if (File.Exists(fileName))
				{
				BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open));
				byte[] allData = reader.ReadBytes(int.MaxValue);
				reader.Close();
				resp.Write(allData);
				return;
				}

				resp.WriteString("<html><head><meta charset=\"UTF-8\"></head><body><h1>File "+urlParams.Get("file")+"</h1></body></html>");
			} // lock(locker_)
		}
	}

	public class NotFound : IHandler
	{
		public void ServeHttp(HttpListenerResponse response, HttpListenerRequest request, IUrlParams urlParams)
		{
			var resp = new ResponseWriter(response);
			resp.WriteString("<html><body><h1>Website not found</h1></body></html>");
		}
	}
}
