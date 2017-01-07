using System;
using System.Collections.Generic;

namespace Cms.Net.Http
{
	public interface IUrlParams
	{
		string Get(string key);
	}

	class UrlParamsEmpty : IUrlParams
	{
		public string Get(string key)
		{
			return "";
		}
	}

	class UrlParamsBag : IUrlParams
	{
		protected Dictionary<string, string> params_;

		public UrlParamsBag()
		{
			params_ = new Dictionary<string, string>();
		}

		public void Add(string key, string val)
		{
			params_.Add(key, val);
		}

        public bool Merge(ref UrlParamsBag other)
        {
            if (other == null) return true;

            foreach (KeyValuePair<string, string> pair in other.Raw())
            {
                params_[pair.Key] = pair.Value;
            }
            return true;
        }

		public string Get(string key)
		{
			if(!params_.ContainsKey(key))
			{
				return "";
			}
			return params_[key];
		}

        public int Count()
        {
            return params_.Count;
        }

        public Dictionary<string, string> Raw()
        {
            return params_;
        }
	}
}
