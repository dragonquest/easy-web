using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;

namespace Cms.Net.Http
{
    interface IRouteCreator
    {
        IRoute Create();
    }

    struct RouteMatch
    {
        public bool IsMatch;
        public IRoute Route;
        public UrlParamsBag Params;
    }

    interface IRoute
    {
        string GetData();
        bool SetData(string data);

        RouteMatch TryMatch(string key);

        bool HasChild(string key);
        IRoute GetChild(string key);
        bool AddChild(string key, IRoute route);

        string Dump(int level = 0);
    }

    class RouteNotFound : IRoute
    {
        public RouteNotFound()
        {
        }

        public string GetData()
        {
            return "";
        }

        public bool SetData(string data)
        {
            return false;
        }

        public bool HasChild(string key)
        {
            return false;
        }

        public RouteMatch TryMatch(string key)
        {
            var r = new RouteMatch();
            r.IsMatch = false;
            return r;
        }

        public IRoute GetChild(string key)
        {
            return new RouteNotFound();
        }

        public bool AddChild(string key, IRoute route)
        {
            return false;
        }

        public string Dump(int level = 0)
        {
            return "";
        }
    }

    class StaticRoute : IRoute, IRouteCreator
    {
        protected string data_;
        protected Dictionary<string, IRoute> children_;

        public StaticRoute()
        {
            children_ = new Dictionary<string, IRoute>();
        }

        public IRoute Create()
        {
            return new StaticRoute();
        }

        public string GetData()
        {
            return data_;
        }

        public bool SetData(string data)
        {
            data_ = data;
            return true;
        }

        public bool HasChild(string key)
        {
            return children_.ContainsKey(key);
        }

        public RouteMatch TryMatch(string key)
        {
            var res = new RouteMatch();

            res.IsMatch = children_.ContainsKey(key);
            if (res.IsMatch)
            {
                res.Route = children_[key];
            }

            return res;
        }

        public IRoute GetChild(string key)
        {
            return children_[key];
        }

        public bool AddChild(string key, IRoute route)
        {
            children_[key] = route;
            return true;
        }

        public string Dump(int level = 0)
        {
            string padding = "";
            for(int i = 0; i < level; i++)
            {
                padding += " ";
            }
            string pretty = "";
            pretty += padding + "[data="+data_+"]";
            pretty += "\n";

            foreach (var item in children_)
            {
                pretty += padding + "Key: " + item.Key + "\n";
                pretty += padding + "Data: " + item.Value.GetData() + "\n";
                pretty += padding + "Children: " + item.Value.Dump(level + 1) + "\n";
                pretty += padding + "--------\n";
            }
            return pretty;
        }
    }

    class RegexRoute : IRoute, IRouteCreator
    {
        protected string data_;
        protected Dictionary<string, IRoute> children_;
		protected Dictionary<string, IRoute> regexChildren_;
		protected ConcurrentDictionary<string, Regex> regexes_;

        public RegexRoute()
        {
            children_ = new Dictionary<string, IRoute>();
            regexChildren_ = new Dictionary<string, IRoute>();
			regexes_ = new ConcurrentDictionary<string, Regex>();
        }

        public IRoute Create()
        {
            return new RegexRoute();
        }

        public string GetData()
        {
            return data_;
        }

        public bool SetData(string data)
        {
            data_ = data;
            return true;
        }

        public bool HasChild(string key)
        {
            return (children_.ContainsKey(key) ||
				    regexChildren_.ContainsKey(key));
        }

        public RouteMatch TryMatch(string key)
        {
            var res = new RouteMatch();

            res.IsMatch = false;

            // If the route is not a regex and can be
            // found then we just go grab that route
            // before we execute any slow regex parsing:
            if (children_.ContainsKey(key))
            {
                res.IsMatch = true;
                res.Route = children_[key];
                return res;
            }

            foreach (KeyValuePair<string, IRoute> child in regexChildren_)
            {
                Regex regex;

				if (regexes_.ContainsKey(child.Key))
				{
					regex = regexes_[child.Key];
				}
				else
				{
					regex = new Regex(child.Key, RegexOptions.Compiled);
					regexes_[child.Key] = regex;
				}

                var match = regex.Match(key);
                if (match.Success)
                {
                    res.IsMatch = true;
                    res.Route = child.Value;
                    res.Params = new UrlParamsBag();
                    foreach (string name in regex.GetGroupNames())
                    {
                        res.Params.Add(name.ToString(), match.Groups[name].Value);
                    }
                    return res;
                }

            }
            return res;
        }

        public IRoute GetChild(string key)
        {
			if (children_.ContainsKey(key))
			{
            	return children_[key];
			}

			return regexChildren_[key];
        }

        public bool AddChild(string key, IRoute route)
        {
			if (key.IndexOf('(') > 0)
			{
				regexChildren_[key] = route;
				return true;
			}

            children_[key] = route;
            return true;
        }

        public string Dump(int level = 0)
        {
            string padding = "";
            for(int i = 0; i < level; i++)
            {
                padding += " ";
            }
            string pretty = "";
            pretty += padding + "[data="+data_+"]";
            pretty += "\n";

            foreach (var item in children_)
            {
                pretty += padding + "Key: " + item.Key + "\n";
                pretty += padding + "Data: " + item.Value.GetData() + "\n";
                pretty += padding + "Children: " + item.Value.Dump(level + 1) + "\n";
                pretty += padding + "--------\n";
            }
            return pretty;
        }
    }

    interface ISegmenter
    {
        string[] Split(string url);
    }

    // The CachedSegmenter caches the segmented paths
    // CAUTION: A brute force lookupper could blow the
    // Cache. This was just a sample impl test of a stupid
    // cache & to benchmark the difference. DO NOT USE IT!
    class CachedSegmenter : ISegmenter
    {
        private Dictionary<string, string[]> cache_;
        private ISegmenter segmenter_;

        public CachedSegmenter(ISegmenter segmenter)
        {
            cache_ = new Dictionary<string, string[]>();
            segmenter_ = segmenter;
        }

        public string[] Split(string url)
        {
            if(!cache_.ContainsKey(url))
            {
                var parsed = segmenter_.Split(url);
                cache_[url] = parsed;
                return parsed;
            }
            return cache_[url];
        }
    }

    class DelimiterSegmenter : ISegmenter
    {
        private char delimiter_ = '/';

        public DelimiterSegmenter(char delimiter = '/')
        {
           delimiter_ = delimiter;
        }

        public string[] Split(string url)
        {
            return url.Split(delimiter_);
        }
    }

    class Router
    {
        protected IRoute root_;
        protected ISegmenter segmenter_;
        protected IRouteCreator routeCreator_;

        public Router(IRouteCreator routeCreator, ISegmenter segmenter)
        {
            segmenter_ = segmenter;
            routeCreator_ = routeCreator;
            root_ = routeCreator.Create();
        }

        public void Add(string url, string data)
        {
            string[] segments = segmenter_.Split(url);

            add(ref root_, segments, data);
        }

        private void add(ref IRoute route, string[] segments, string data)
        {
            if (segments.Length == 0)
            {
                route.SetData(data);
                return;
            }

            if (!route.HasChild(segments[0]))
            {
                IRoute newRoute = routeCreator_.Create();
                newRoute.SetData(data);
                route.AddChild(segments[0], newRoute);
            }

            var nextRoute = route.GetChild(segments[0]);
            add(ref nextRoute, pop(1, segments), data);
            return;
        }

        public KeyValuePair<IRoute, UrlParamsBag> Lookup(string url)
        {
            var parameters = new UrlParamsBag();
            string[] segments = segmenter_.Split(url);

            return lookup(ref root_, segments, ref parameters);
        }

        private KeyValuePair<IRoute, UrlParamsBag> lookup(ref IRoute route, string[] segments, ref UrlParamsBag parameters)
        {
            if (segments.Length == 0)
            {
                return new KeyValuePair<IRoute, UrlParamsBag>(new RouteNotFound(), parameters);
            }

            RouteMatch m = route.TryMatch(segments[0]);

            if (!m.IsMatch)
            {
                return new KeyValuePair<IRoute, UrlParamsBag>(new RouteNotFound(), parameters);
            }

            parameters.Merge(ref m.Params);

            if (segments.Length == 1)
            {
                return new KeyValuePair<IRoute, UrlParamsBag>(m.Route, parameters);
            }

            var nextRoute = m.Route;
            return lookup(ref nextRoute, pop(1, segments), ref parameters);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string[] pop(int n, string[] values)
        {
            var popped = new string[values.Length - n];
            int ptr=0;
            for(int i = n; i < values.Length; i++)
            {
                popped[ptr++] = values[i];
            }
            return popped;
        }
    }
}
