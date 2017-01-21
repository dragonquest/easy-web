using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using System.Linq;

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

        // If Stop is set to true then the matched
        // route should be returned and the segments
        // should not be walked further.
        public bool Stop;
    }

    interface IRoute
    {
        IHandler GetData();
        bool SetData(IHandler data);

        RouteMatch TryMatch(string key);

        bool HasChild(string key);
        IRoute GetChild(string key);
        bool AddChild(string key, IRoute route);

        string Dump(int level = 0);
    }

    class StaticRoute : IRoute, IRouteCreator
    {
        protected IHandler _handler;
        protected Dictionary<string, IRoute> _children;

        public StaticRoute()
        {
            _children = new Dictionary<string, IRoute>();
        }

        public IRoute Create()
        {
            return new StaticRoute();
        }

        public IHandler GetData()
        {
            return _handler;
        }

        public bool SetData(IHandler handler)
        {
            _handler = handler;
            return true;
        }

        public bool HasChild(string key)
        {
            return _children.ContainsKey(key);
        }

        public RouteMatch TryMatch(string key)
        {
            var res = new RouteMatch();
            res.Stop = false;
            res.IsMatch = _children.ContainsKey(key);
            if (res.IsMatch)
            {
                res.Route = _children[key];
            }

            return res;
        }

        public IRoute GetChild(string key)
        {
            return _children[key];
        }

        public bool AddChild(string key, IRoute route)
        {
            _children[key] = route;
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
            pretty += padding + "[data="+_handler+"]";
            pretty += "\n";

            foreach (var item in _children)
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
        protected IHandler _handler;
        protected Dictionary<string, IRoute> _children;
        protected Dictionary<string, IRoute> _regexChildren;
        protected ConcurrentDictionary<string, Regex> _regexes;

        public RegexRoute()
        {
            _children = new Dictionary<string, IRoute>();
            _regexChildren = new Dictionary<string, IRoute>();
            _regexes = new ConcurrentDictionary<string, Regex>();
        }

        public IRoute Create()
        {
            return new RegexRoute();
        }

        public IHandler GetData()
        {
            return _handler;
        }

        public bool SetData(IHandler handler)
        {
            _handler = handler;
            return true;
        }

        public bool HasChild(string key)
        {
            return (_children.ContainsKey(key) ||
                    _regexChildren.ContainsKey(key));
        }

        public RouteMatch TryMatch(string key)
        {
            var res = new RouteMatch();

            res.IsMatch = false;
            res.Stop = false;

            // If the route is not a regex and can be
            // found then we just go grab that route
            // before we execute any slow regex parsing:
            if (_children.ContainsKey(key))
            {
                res.IsMatch = true;
                res.Route = _children[key];
                return res;
            }

            foreach (KeyValuePair<string, IRoute> child in _regexChildren)
            {
                Regex regex;

                if (_regexes.ContainsKey(child.Key))
                {
                    regex = _regexes[child.Key];
                }
                else
                {
                    regex = new Regex(child.Key, RegexOptions.Compiled);
                    _regexes[child.Key] = regex;
                }

                var match = regex.Match(key);
                if (match.Success)
                {
                    if (child.Key.LastIndexOf('$') >= 0)
                    {
                        res.Stop = true;
                    }

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
            if (_children.ContainsKey(key))
            {
                return _children[key];
            }

            return _regexChildren[key];
        }

        public bool AddChild(string key, IRoute route)
        {
            if (key.IndexOf('(') >= 0 && key.IndexOf(')') >= 0)
            {
                _regexChildren[key] = route;
                return true;
            }

            _children[key] = route;
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
            pretty += padding + "[data="+_handler+"]";
            pretty += "\n";

            foreach (var item in _children)
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
        private Dictionary<string, string[]> _cache;
        private ISegmenter _segmenter;

        private int _size;

        public CachedSegmenter(ISegmenter segmenter, int size = 5000)
        {
            _cache = new Dictionary<string, string[]>();
            _segmenter = segmenter;
            _size = size;
        }

        public string[] Split(string url)
        {
            if(!_cache.ContainsKey(url))
            {
                if (_cache.Count >= _size) {
                    Random rand = new Random();
                    var itemKey = _cache.Keys.ToList()[rand.Next(0, _cache.Count)];
                    _cache.Remove(itemKey);
                }

                var parsed = _segmenter.Split(url);
                _cache[url] = parsed;
                return parsed;
            }
            return _cache[url];
        }
    }

    class DelimiterSegmenter : ISegmenter
    {
        private char _delimiter = '/';

        public DelimiterSegmenter(char delimiter = '/')
        {
            _delimiter = delimiter;
        }

        public string[] Split(string url)
        {
            return url.Split(_delimiter);
        }
    }

    class Router
    {
        protected IRoute _root;
        protected ISegmenter _segmenter;
        protected IRouteCreator _routeCreator;

        public Router(IRouteCreator routeCreator, ISegmenter segmenter)
        {
            _segmenter = segmenter;
            _routeCreator = routeCreator;
            _root = routeCreator.Create();
        }

        public void Add(string url, IHandler handler)
        {
            string[] segments = _segmenter.Split(url);

            add(ref _root, segments, handler);
        }

        private void add(ref IRoute route, string[] segments, IHandler handler)
        {
            if (segments.Length == 0)
            {
                route.SetData(handler);
                return;
            }

            if (!route.HasChild(segments[0]))
            {
                IRoute newRoute = _routeCreator.Create();
                newRoute.SetData(handler);
                route.AddChild(segments[0], newRoute);
            }

            var nextRoute = route.GetChild(segments[0]);
            add(ref nextRoute, pop(1, segments), handler);
            return;
        }

        public KeyValuePair<IHandler, UrlParamsBag> Lookup(string url)
        {
            var parameters = new UrlParamsBag();
            string[] segments = _segmenter.Split(url);

            return lookup(ref _root, segments, ref parameters);
        }

        private KeyValuePair<IHandler, UrlParamsBag> lookup(ref IRoute route, string[] segments, ref UrlParamsBag parameters)
        {
            if (segments.Length == 0)
            {
                return new KeyValuePair<IHandler, UrlParamsBag>(null, parameters);
            }

            RouteMatch m = route.TryMatch(segments[0]);
            if (!m.IsMatch)
            {
                return new KeyValuePair<IHandler, UrlParamsBag>(null, parameters);
            }

            parameters.Merge(ref m.Params);

            if (segments.Length == 1 || m.Stop)
            {
                return new KeyValuePair<IHandler, UrlParamsBag>(m.Route.GetData(), parameters);
            }

            var nextRoute = m.Route;
            return lookup(ref nextRoute, pop(1, segments), ref parameters);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private string[] pop(int n, string[] values)
        {
            var popped = new string[values.Length - n];
            int ptr = 0;
            for(int i = n; i < values.Length; i++)
            {
                popped[ptr++] = values[i];
            }
            return popped;
        }
    }
}
