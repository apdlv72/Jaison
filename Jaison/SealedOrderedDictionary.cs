using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Jaison.Exceptions;

namespace Jaison.Containers
{
    // TODO: Fully implement UnSealedOrderedDictionary
    public class SealedOrderedDictionary : OrderedDictionary, ISealableDictionary
    {
        public new void Clear()
        {
            if (_sealed)
            {
                throw new ImmutableException("Dictionary is read only");
            }
            base.Clear();
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            Check(item.Key);
            if (Contains(item))
            {
                base.Remove(item.Key);
                return true;
            }
            return false;
        }


        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            System.Collections.IDictionaryEnumerator e = base.GetEnumerator();
            // TODO: Just casting is probbaly not sufficient here.
            return (System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<string, object>>)e;
        }

        public void Add(string key, object value)
        {
            Check(key);
            base.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return base.Contains(key);
        }

        public bool Remove(string key)
        {
            if (Contains(key))
            {
                base.Remove(key);
                return true;
            }
            return false;
        }

        public bool TryGetValue(string key, out object value)
        {
            try
            {
                value = base[key];
                return true;
            }
            catch (SystemException)
            {
                value = null;
                return false;
            }
        }

        public void Add(KeyValuePair<string, object> item)
        {
            base.Add(item.Key, item.Value);
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            object value = null;
            if (TryGetValue(item.Key, out value))
            {
                return (null == value && null == item.Value) || (null != value && value.Equals(item.Value));
            }
            return false;
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        // TODO: Just casting is probbaly not sufficient here.
        ICollection<string> IDictionary<string, object>.Keys => (System.Collections.Generic.ICollection<string>)base.Keys;

        // TODO: Just casting is probbaly not sufficient here.
        ICollection<object> IDictionary<string, object>.Values => (System.Collections.Generic.ICollection<object>)base.Values;

        public object this[string key]
        {
            get => base[key];
            // TODO: Implement set
            set => throw new NotImplementedException();
        }

        public void Seal()
        {
            _sealed = true;
        }

        public void Check(string key)
        {
            if (_sealed)
            {
                throw new ImmutableException(key, "Dictionary is read only");
            }
        }

        private bool _sealed;
    }
}
