using System.Collections.Generic;

namespace Cms.Sync
{
    // Pool is a set of temporary objects that may be saved and fetched.
    // The purpose is to cache allocated but unused items in order to reuse them later. 
    // The Pool is safe for use for multiple threads.
    public class Pool<T> where T : new() // T must have a public constructor
    {
        private Stack<T> _items = new Stack<T>();
        private object _sync = new object();

        public T Get()
        {
            lock (_sync)
            {
                if (_items.Count == 0)
                {
                    return new T();
                }
                else
                {
                    return _items.Pop();
                }
            }
        }

        public void Free(T item)
        {
            lock (_sync)
            {
                _items.Push(item);
            }
        }
    }
}
