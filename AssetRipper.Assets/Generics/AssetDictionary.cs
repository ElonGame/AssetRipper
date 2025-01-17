﻿using System.Diagnostics.CodeAnalysis;

namespace AssetRipper.Assets.Generics
{
	/// <summary>
	/// A dictionary class supporting non-unique keys
	/// </summary>
	/// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
	/// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
	public sealed class AssetDictionary<TKey, TValue> : AccessDictionaryBase<TKey, TValue>
		where TKey : notnull, new()
		where TValue : notnull, new()
	{
		private const int DefaultCapacity = 4;
		private AccessPairBase<TKey, TValue>[] pairs;
		private int count = 0;

		public AssetDictionary() : this(DefaultCapacity) { }

		public AssetDictionary(int capacity)
		{
			pairs = capacity == 0 ? Array.Empty<AccessPairBase<TKey, TValue>>() : new AccessPairBase<TKey, TValue>[capacity];
		}

		/// <inheritdoc/>
		public override int Count => count;

		/// <inheritdoc/>
		public override int Capacity
		{
			get => pairs.Length;
			set
			{
				if (value < count)
				{
					throw new ArgumentOutOfRangeException(nameof(value));
				}

				if (value != pairs.Length)
				{
					if (value > 0)
					{
						AccessPairBase<TKey, TValue>[] newPairs = new AccessPairBase<TKey, TValue>[value];
						if (count > 0)
						{
							Array.Copy(pairs, newPairs, count);
						}
						pairs = newPairs;
					}
					else
					{
						pairs = Array.Empty<AccessPairBase<TKey, TValue>>();
					}
				}
			}
		}

		/// <inheritdoc/>
		public override void Add(TKey key, TValue value)
		{
			Add(new AssetPair<TKey, TValue>(key, value));
		}

		/// <inheritdoc/>
		public override void Add(AccessPairBase<TKey, TValue> pair)
		{
			if (count == Capacity)
			{
				Grow(count + 1);
			}

			pairs[count] = pair;
			count++;
		}

		/// <inheritdoc/>
		public override void AddNew() => Add(new TKey(), new TValue());

		public void AddRange(IEnumerable<AccessPairBase<TKey, TValue>> range)
		{
			foreach (AccessPairBase<TKey, TValue> pair in range)
			{
				Add(pair);
			}
		}

		/// <inheritdoc/>
		public override TKey GetKey(int index)
		{
			if ((uint)index >= (uint)count)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			return pairs[index].Key;
		}

		/// <inheritdoc/>
		public override void SetKey(int index, TKey newKey)
		{
			if ((uint)index >= (uint)count)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			pairs[index].Key = newKey;
		}

		/// <inheritdoc/>
		public override TValue GetValue(int index)
		{
			if (index < 0 || index >= count)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			return pairs[index].Value;
		}

		/// <inheritdoc/>
		public override void SetValue(int index, TValue newValue)
		{
			if ((uint)index >= (uint)count)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			pairs[index].Value = newValue;
		}

		public override AccessPairBase<TKey, TValue> GetPair(int index)
		{
			if ((uint)index >= (uint)count)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			return pairs[index];
		}

		/// <inheritdoc/>
		public override int IndexOf(AccessPairBase<TKey, TValue> item) => Array.IndexOf(pairs, item, 0, count);

		/// <inheritdoc/>
		public override void Insert(int index, AccessPairBase<TKey, TValue> item)
		{
			// Note that insertions at the end are legal.
			if ((uint)index > (uint)count)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			if (count == pairs.Length)
			{
				Grow(count + 1);
			}

			if (index < count)
			{
				Array.Copy(pairs, index, pairs, index + 1, count - index);
			}

			pairs[index] = item;
			count++;
		}

		/// <inheritdoc/>
		public override void RemoveAt(int index)
		{
			if ((uint)index >= (uint)count)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			count--;
			if (index < count)
			{
				Array.Copy(pairs, index + 1, pairs, index, count - index);
			}
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
			pairs[count] = default;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
		}

		/// <inheritdoc/>
		public override void Clear()
		{
			if (count > 0)
			{
				Array.Clear(pairs, 0, count); // Clear the elements so that the gc can reclaim the references.
			}
			count = 0;
		}

		/// <inheritdoc/>
		public override bool Contains(AccessPairBase<TKey, TValue> item)
		{
			return IndexOf(item) >= 0;
		}

		/// <inheritdoc/>
		public override void CopyTo(AccessPairBase<TKey, TValue>[] array, int arrayIndex)
		{
			if (array == null)
			{
				throw new ArgumentNullException(nameof(array));
			}

			if (arrayIndex < 0 || arrayIndex >= array.Length - count)
			{
				throw new ArgumentOutOfRangeException(nameof(arrayIndex));
			}

			Array.Copy(pairs, 0, array, arrayIndex, count);
		}

		/// <inheritdoc/>
		public override bool Remove(AccessPairBase<TKey, TValue> item)
		{
			int index = IndexOf(item);
			if (index >= 0)
			{
				RemoveAt(index);
				return true;
			}
			return false;
		}

		protected override bool TryGetSinglePairForKey(TKey key, [NotNullWhen(true)] out AccessPairBase<TKey, TValue>? pair)
		{
			if (key is null)
			{
				throw new ArgumentNullException(nameof(key));
			}

			int hash = key.GetHashCode();
			bool found = false;
			pair = null;
			for (int i = Count - 1; i > -1; i--)
			{
				AccessPairBase<TKey, TValue> p = pairs[i];
				if (p.Key.GetHashCode() == hash && key.Equals(p.Key))
				{
					if (found)
					{
						throw new Exception("Found more than one matching key");
					}
					else
					{
						found = true;
						pair = p;
					}
				}
			}
			return found;
		}

		/// <summary>
		/// Ensures that the capacity of this list is at least the specified <paramref name="capacity"/>.
		/// If the current capacity of the list is less than specified <paramref name="capacity"/>,
		/// the capacity is increased by continuously twice current capacity until it is at least the specified <paramref name="capacity"/>.
		/// </summary>
		/// <param name="capacity">The minimum capacity to ensure.</param>
		/// <returns>The new capacity of this list.</returns>
		public int EnsureCapacity(int capacity)
		{
			if (capacity < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(capacity));
			}
			if (pairs.Length < capacity)
			{
				Grow(capacity);
			}

			return pairs.Length;
		}

		private void Grow(int capacity)
		{
			long newcapacity = pairs.Length == 0 ? DefaultCapacity : 2L * pairs.Length;

			// Allow the list to grow to maximum possible capacity (~2G elements) before encountering overflow.
			// Note that this check works even when _items.Length overflowed thanks to the (uint) cast
			if (newcapacity > Array.MaxLength)
			{
				newcapacity = Array.MaxLength;
			}

			// If the computed capacity is still less than specified, set to the original argument.
			// Capacities exceeding Array.MaxLength will be surfaced as OutOfMemoryException by Array.Resize.
			if (newcapacity < capacity)
			{
				newcapacity = capacity;
			}

			Capacity = (int)newcapacity;
		}
	}
}
