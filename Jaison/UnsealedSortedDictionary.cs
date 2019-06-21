using System.Collections.Generic;

namespace Jaison.Containers
{
    public class UnsealedSortedDictionary : SortedDictionary<string, object>, ISealableDictionary
    {
        public void Seal()
        {
        }
    }
}
