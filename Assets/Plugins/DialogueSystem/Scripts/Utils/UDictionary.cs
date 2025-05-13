using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class UDictionary<K, V> : ISerializationCallbackReceiver
{
	//variables
	private Dictionary<K, V> backingDictionary = new Dictionary<K, V>();

	[SerializeField]
	private List<K> keys;
	[SerializeField]
	private List<V> values;
	private int length;

	public UDictionary()
	{
		//does nothing
	}

	public UDictionary(IEqualityComparer<K> _c)
	{
		backingDictionary = new Dictionary<K, V>(_c);
	}

	public void OnBeforeSerialize()
	{
		if (keys != null && values != null)
		{
			ResetSize();

            int i = 0;
			foreach (var kvp in backingDictionary)
			{
				if (i >= length) { break; }
				keys[i] = kvp.Key;
				values[i] = kvp.Value;
				i++;
			}
		}
	}

	public void OnAfterDeserialize()
	{
		if (keys != null && values != null)
		{
			ResetSize();

			backingDictionary.Clear();

			for (int i = 0; i < length; i++)
			{
				if (keys[i] == null) { continue; }
				K key = keys[i];
				V value = values[i];
				
				if (!backingDictionary.ContainsKey(key))
				{
					backingDictionary.Add(key, value);
				}
			}
		}
	}

	private void ResetSize()
	{
		if (keys.Count == values.Count)
		{
			length = keys.Count;
			return;
		}

		int newLength = keys.Count != length ? keys.Count : values.Count;
		int diff = length - newLength;
		int numOfElements = diff < 0 ? length : newLength;

		if (newLength != keys.Count) //if number of keys is different
		{
			K[] oldKeys = keys.ToArray();
			keys = new List<K>(new K[newLength]);

			for (int i = 0; i < numOfElements; i++)
			{
                keys[i] = oldKeys[i];
			}
		} //if the number of values is different
		else
		{
			V[] oldVals = values.ToArray();
			values = new List<V>(new V[newLength]);

			for (int i = 0; i < numOfElements; i++)
			{
                values[i] = oldVals[i];
			}
		}

		length = newLength;
	}

	public bool ContainsKey(K key)
	{
		return backingDictionary.ContainsKey(key);
	}

	public bool ContainsValue(V value)
	{
		return backingDictionary.ContainsValue(value);
	}

	public IEqualityComparer<K> Comparer
	{
		get
		{
			return backingDictionary.Comparer;
		}
	}

	public void Add(K key, V value)
	{
		keys.Add(key);
		values.Add(value);
		backingDictionary.Add(key, value);
	}

	public void Remove(K key)
	{
		int idx = keys.IndexOf(key);
		if (idx == -1) { return; }
		keys.RemoveAt(idx);
		values.RemoveAt(idx);
		backingDictionary.Remove(key);
	}

	public V this[K key]
	{
		get
		{
			return backingDictionary[key];
		}
		
		set
		{
			backingDictionary[key] = value;
		}
	}

	public IEnumerable<K> Keys
	{
		get
		{
			return backingDictionary.Keys;
		}
	}

	public IEnumerable<V> Values
	{
		get
		{
			return backingDictionary.Values;
		}
	}

	public int Count
	{
		get
		{
			return backingDictionary.Count;
		}
	}
}