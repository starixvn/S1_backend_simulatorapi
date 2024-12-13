using Microsoft.Extensions.Caching.Memory;

namespace SabaSimulator.API.Helper;

public class ConcurrentList<T>
{
	private readonly IMemoryCache _cache;

	private readonly string _listKey;

	public ConcurrentList(string listKey, IMemoryCache memoryCache)
	{
		_listKey = listKey;
		_cache = memoryCache;
		_cache.Set(listKey, new List<T>());
	}

	private List<T> GetList()
	{
		return _cache.Get(_listKey) as List<T>;
	}

	public void Add(T item)
	{
		var list = GetList();
		if (list != null)
		{
			lock (list) // Ensure thread safety
			{
				list.Add(item);
				_cache.Set(_listKey, list);
			}
		}
	}

	public void Remove(T item)
	{
		var list = GetList();
		if (list != null)
		{
			lock (list) // Ensure thread safety
			{
				list.Remove(item);
				_cache.Set(_listKey, list);
			}
		}
	}

	public List<T> GetAll()
	{
		var list = GetList();
		return list != null ? new List<T>(list) : new List<T>();
	}

	public void Clear()
	{
		var list = GetList();
		if (list != null)
		{
			lock (list) // Ensure thread safety
			{
				list.Clear();
				_cache.Set(_listKey, list);
			}
		}
	}

	public T Find(Func<T, bool> predicate)
	{
		var list = GetList();
		lock (list)
		{
			return list.FirstOrDefault(predicate);
		}
	}

	// Get the first item that matches a given predicate, or default if none found
	public T FirstOrDefault(Func<T, bool> predicate)
	{
		var list = GetList();
		lock (list)
		{
			return list.FirstOrDefault(predicate);
		}
	}

	public int Count()
	{
		var list = GetList();
		return list?.Count ?? 0;
	}
}