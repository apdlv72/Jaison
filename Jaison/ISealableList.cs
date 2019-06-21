using System.Collections.Generic;

namespace Jaison.Containers
{
    public interface ISealableList : IList<object>
    {
        void Seal();
    }
}
