using System;
using System.Collections.Generic;

namespace Cms.Net.Http
{
    public interface IUrlParams
    {
        string Get(string key);
    }

    public class UrlParamsEmpty : IUrlParams
    {
        public string Get(string key)
        {
            return "";
        }
    }

    public class UrlParamsBag : IUrlParams
    {
        protected Dictionary<string, string> _params;

        public UrlParamsBag()
        {
            _params = new Dictionary<string, string>();
        }

        public void Add(string key, string val)
        {
            _params.Add(key, val);
        }

        public bool Merge(ref UrlParamsBag other)
        {
            if (other == null) return true;

            foreach (KeyValuePair<string, string> pair in other.Raw())
            {
                _params[pair.Key] = pair.Value;
            }
            return true;
        }

        public string Get(string key)
        {
            if(!_params.ContainsKey(key))
            {
                return "";
            }
            return _params[key];
        }

        public int Count()
        {
            return _params.Count;
        }

        public Dictionary<string, string> Raw()
        {
            return _params;
        }
    }
}
