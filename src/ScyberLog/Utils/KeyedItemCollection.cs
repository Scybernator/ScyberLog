using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ScyberLog 
{
    public interface IKeyedItem
    {
        string Key { get; }
    }

    public class KeyedItemCollection<T> : KeyedCollection<string, T> where T : IKeyedItem
    {
        public KeyedItemCollection() { }
        public KeyedItemCollection(IEnumerable<T> items)
        {
            foreach(var item in items)
            {
                this.Add(item);
            }
        }

        protected override string GetKeyForItem(T item) => item.Key;
    }
}