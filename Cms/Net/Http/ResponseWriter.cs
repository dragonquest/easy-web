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
        HttpListenerResponse _response;

        public ResponseWriter(HttpListenerResponse response)
        {
            _response = response;
        }

        public WebHeaderCollection Headers()
        {
            return _response.Headers;
        }

        public void SetStatus(HttpStatusCode code)
        {
            _response.StatusCode = (int)code;
        }

        public void Redirect(string url)
        {
            _response.Redirect(url);
            _response.Close();
        }

        public void WriteString(string content)
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(content);
            Write(buffer);
        }

        public void Write(byte[] content)
        {
            _response.ContentLength64 = content.Length;
            System.IO.Stream output = _response.OutputStream;
            output.Write(content, 0, content.Length);
            output.Close();
            _response.Close();
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
            _response.AddHeader("Content-Encoding", "gzip");
            _response.ContentLength64 = content.Length;
            _response.OutputStream.Write(content, 0, content.Length);
        }

        public HttpListenerResponse GetRawResponse()
        {
            return _response;
        }
    }
}
