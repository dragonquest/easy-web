using System;
using System.Text;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.Linq;

using EasyWeb.Net.Http;
using EasyWeb.Net.Http.Form;

using System.Web;

using System.Collections.Generic;

namespace Controller {

	public class MemeGen
	{
		private EasyWeb.View.ITemplate _template;
		private Storage.MemeTemplate _memeTemplates;
		private Storage.MemeFileStorage _meme;
        private string _memeWebPath;

		public MemeGen(EasyWeb.View.ITemplate template, Storage.MemeTemplate memeTemplates, Storage.MemeFileStorage meme, string memeWebPath)
		{
			_template = template;
			_memeTemplates = memeTemplates;
            _meme = meme;
            _memeWebPath = memeWebPath;
		}

		public void IndexPage(IResponseWriter response, IRequest request, IUrlParams urlParams)
		{
			response.WriteString(_template.Render("index.cshtml", new
            {
                PublicPath = _memeWebPath,
                Memes = Enumerable.Reverse(_meme.ListAll())
            }));
		}

		public void SelectTemplatePage(IResponseWriter response, IRequest request, IUrlParams urlParams)
		{
			response.WriteString(_template.Render("select-template.cshtml", _memeTemplates.GetTemplates()));
		}

		public void CreateMemePage(IResponseWriter response, IRequest request, IUrlParams urlParams)
		{
			var queryString = HttpUtility.ParseQueryString(request.GetRawRequest().Url.Query);

			var template = queryString["template"];
			response.WriteString(_template.Render("create-meme.cshtml", new {
				template = template
			}));
		}

		public void SaveMemePage(IResponseWriter response, IRequest request, IUrlParams urlParams)
		{
            var form = PostParser.Parse(request);

            var image = Convert.FromBase64String(form["image"].Substring(form["image"].IndexOf(",")+1));

            var filename = createFilenameFromContent(image);
            _meme.Save(filename, image);

            response.WriteJson(new Dictionary<string,string>()
            {
                { "filename", _memeWebPath + filename }
            });
            return;

		}

        private string createFilenameFromContent(byte[] content)
        {
            var filename = new StringBuilder();

            using (MD5 md5Hash = MD5.Create())
            {
                filename.Append(DateTime.Now.ToString("yyyy-MM-dd_hh_mm_ss"));

                var hash = md5Hash.ComputeHash(content);
                for (int i = 0; i < hash.Length; i++)
                {
                    filename.Append(hash[i].ToString("x2"));
                }
                filename.Append(".jpg");
            }

            return filename.ToString();
        }
	}

	public class AssetsServer : IHandler
	{
		static readonly object _locker = new object();
		protected string _assetsDir;

		public AssetsServer(string assetsDir)
		{
			_assetsDir = assetsDir;
		}

		public void ServeHttp(IResponseWriter response, IRequest request, IUrlParams urlParams)
		{
			lock(_locker) {
				string fileName = _assetsDir + request.Url.AbsolutePath;
				//                response.Headers().Set("Cache-Control", "max-age=1"); //2592000 30 days default

				if (File.Exists(fileName))
				{
					string mimeType = MimeMapping.GetMimeMapping(fileName);
					response.GetRawResponse().ContentType = mimeType;

					byte[] allData = File.ReadAllBytes(fileName);
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
