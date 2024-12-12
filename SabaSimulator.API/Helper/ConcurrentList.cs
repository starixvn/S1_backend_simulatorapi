namespace SabaSimulator.API.Helper;

public class ConcurrentList<T>
{
	private List<T> _list;
	private readonly object _lock = new object();

	public ConcurrentList()
	{
		_list = new List<T>();
	}

	public List<T> GetData() => _list;

	// Add an item to the list
	public void Add(T item)
	{
		lock (_lock)
		{
			_list.Add(item);
		}
	}

	// Remove an item from the list
	public bool Remove(T item)
	{
		lock (_lock)
		{
			return _list.Remove(item);
		}
	}

	// Get an item at a specific index
	public T Get(int index)
	{
		lock (_lock)
		{
			if (index >= 0 && index < _list.Count)
			{
				return _list[index];
			}
			throw new IndexOutOfRangeException();
		}
	}

	// Get the count of items in the list
	public int Count
	{
		get
		{
			lock (_lock)
			{
				return _list.Count;
			}
		}
	}

	// Iterate over the list safely
	public IEnumerable<T> GetAll()
	{
		lock (_lock)
		{
			return new List<T>(_list);
		}
	}

	// Clear all items from the list
	public void Clear()
	{
		lock (_lock)
		{
			_list.Clear();
		}
	}

	public T Find(Func<T, bool> predicate)
	{
		lock (_lock)
		{
			return _list.FirstOrDefault(predicate);
		}
	}

	// Get the first item that matches a given predicate, or default if none found
	public T FirstOrDefault(Func<T, bool> predicate)
	{
		lock (_lock)
		{
			return _list.FirstOrDefault(predicate);
		}
	}
}