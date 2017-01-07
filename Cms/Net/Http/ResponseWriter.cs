using System;
using System.Net;

namespace Cms.Net.Http
{
	class ResponseWriter
	{
		HttpListenerResponse response_;

		public ResponseWriter(HttpListenerResponse response)
		{
			response_ = response;
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
	}
}
