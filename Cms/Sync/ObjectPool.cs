using System.Collections.Generic;

namespace Cms.Sync
{
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
