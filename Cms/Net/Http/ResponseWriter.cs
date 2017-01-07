using System;
using System.Net;

using System.IO;
using System.IO.Compression;

namespace Cms.Net.Http
{
    public interface IResponseWriter
    {
        void SetStatus(HttpStatusCode code);
        void Redirect(string url);
        void WriteString(string content);
        void Write(byte[] content);

        HttpListenerResponse GetRawResponse();
        WebHeaderCollection Headers();
    }

	class ResponseWriter : IResponseWriter
	{
		HttpListenerResponse response_;

		public ResponseWriter(HttpListenerResponse response)
		{
			response_ = response;
		}

        public WebHeaderCollection Headers()
        {
            return response_.Headers;
        }

		public void SetStatus(HttpStatusCode code)
		{
			response_.StatusCode = (int)code;
		}

		public void Redirect(string url)
		{
			response_.Redirect(url);
			response_.Close();
		}

		public void WriteString(string content)
		{
			byte[] buffer = System.Text.Encoding.UTF8.GetBytes(content);
			Write(buffer);
		}

		public void Write(byte[] content)
		{
			response_.ContentLength64 = content.Length;
			System.IO.Stream output = response_.OutputStream;
			output.Write(content, 0, content.Length);
			output.Close();
			response_.Close();
		}

        private void WriteGzip(byte[] content)
        {
            using (var ms = new MemoryStream())
            {
                using (var zip = new GZipStream(ms, CompressionMode.Compress, true))
                {
                    zip.Write(content, 0, content.Length);
                }
                content = ms.ToArray();
            }
            response_.AddHeader("Content-Encoding", "gzip");
            response_.ContentLength64 = content.Length;
            response_.OutputStream.Write(content, 0, content.Length);
        }

        public HttpListenerResponse GetRawResponse()
        {
            return response_;
        }
	}
}
