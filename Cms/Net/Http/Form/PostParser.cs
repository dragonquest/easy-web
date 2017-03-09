using System;
using System.Net;
using System.Web;
using System.Collections.Specialized;

using Cms.Net.Http;

namespace Cms.Net.Http.Form
{
    public class PostParser
    {
        public static NameValueCollection Parse(IRequest req)
        {
            var rawReq = req.GetRawRequest();

			if (!rawReq.HasEntityBody)
			{
				return new NameValueCollection();
			}

			System.IO.Stream body = rawReq.InputStream;

			System.Text.Encoding encoding = rawReq.ContentEncoding;
			System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);

            string s = reader.ReadToEnd();
			body.Close();
			reader.Close();

            return HttpUtility.ParseQueryString(s);
        }
    }
}
