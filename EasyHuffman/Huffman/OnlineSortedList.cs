using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Easy.Huffman
{
    public class OnlineSortedList<T> : IReadOnlyList<T>, ICollection<T> where T : IComparable<T>
    {
        private readonly IList<T> _list;
        private readonly Order _order;

        public OnlineSortedList(Order order = Order.Ascending) : this(new List<T>(), order)
        {
            
        }

        public OnlineSortedList(int capacity, Order order = Order.Ascending) : this(new List<T>(capacity), order)
        {

        }

        public OnlineSortedList(IEnumerable<T> collection, Order order = Order.Ascending) : this(new List<T>(collection.OrderBy(element => element)), order)
        {
        }

        public OnlineSortedList(IList<T> internalList, Order order = Order.Ascending)
        {
            for(int i = 0; i + 1 < internalList.Count; i++)
            {
                if ((order == Order.Ascending && internalList[i].CompareTo(internalList[i + 1]) > 0) ||
                    (order == Order.Descending && internalList[i].CompareTo(internalList[i + 1]) < 0))
                    throw new ArgumentException("The list is not sorted.", nameof(internalList));
            }

            _list = internalList;
            _order = order;
        }

        public T this[int index] => _list[index];

        public int Count => _list.Count;

        public bool IsReadOnly => _list.IsReadOnly;

        public void Add(T item)
        {
            IEnumerator<T> enumerator = _list.GetEnumerator();

            int index = 0;
            while (enumerator.MoveNext() && ((_order == Order.Descending) ? (item.CompareTo(enumerator.Current) < 0) : (item.CompareTo(enumerator.Current) > 0)))
            {
                index++;
            }

            _list.Insert(index, item);
        }

        public void Clear() => _list.Clear();

        public bool Contains(T item) => _list.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

        public bool Remove(T item) => _list.Remove(item);

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_list).GetEnumerator();
    }
}
