using System.Collections.Generic;
using Jaison.Exceptions;

namespace Jaison.Containers
{
    public class SealedDictionary : Dictionary<string, object>, ISealableDictionary
    {
        public new void Add(string key, object value)
        {
            Check(key);
            base.Add(key, value);
        }

        public new bool Remove(string key)
        {
            Check(key);
            return base.Remove(key);
        }

        public new void Clear()
        {
            if (_sealed)
            {
                throw new ImmutableException("Blocked attempt to clear read only dictionary");
            }
            base.Clear();
        }

        public void Seal()
        {
            _sealed = true;
        }

        public void Check(string key)
        {
            if (_sealed)
            {
                throw new ImmutableException(key, "Blocked attempt to add key '" + key + "' to read only dictionary");
            }
        }

        public new string ToString()
        {
            return base.ToString();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            object value = null;
            if (TryGetValue(item.Key, out value))
            {
                return value.Equals(item.Value);
            }
            return false;
        }


        private bool _sealed;

    }
}
