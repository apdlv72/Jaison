using System.Collections.Generic;
using Jaison.Exceptions;

namespace Jaison.Containers
{
    public class SealedSortedDictionary : SortedDictionary<string, object>, ISealableDictionary
    {
        public new void Add(string key, object value)
        {
            Check(key);
            base.Add(key, value);
        }

        public new object Remove(string key)
        {
            Check(key);
            return base.Remove(key);
        }

        public new void Clear()
        {
            if (_sealed)
            {
                throw new ImmutableException("Blocked attempt clear read only dictionary");
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

        private bool _sealed;
    }

}
