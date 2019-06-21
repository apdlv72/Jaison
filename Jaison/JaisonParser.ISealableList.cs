using System.Collections.Generic;

namespace Jaison
{
    public partial class JaisonParser
    {
        public interface ISealableList : IList<object>
        {
            void Seal();
        }
    }
}
