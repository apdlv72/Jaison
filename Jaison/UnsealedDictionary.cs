using System.Collections.Generic;

namespace Jaison.Containers
{
    public class UnsealedDictionary : Dictionary<string, object>, ISealableDictionary
    {
        public void Seal()
        {
        }
    }
}
