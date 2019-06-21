using System.Collections.Generic;

namespace Jaison.Containers
{
    public class UnsealedList : List<object>, ISealableList
    {
        public void Seal()
        {
            // don't do anything here
        }
    }
}
