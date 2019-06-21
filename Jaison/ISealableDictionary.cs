using System.Collections.Generic;

namespace Jaison.Containers
{
    public interface ISealableDictionary : IDictionary<string, object>
    {
        void Seal();
    }
}
