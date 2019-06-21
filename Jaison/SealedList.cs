using System;
using System.Collections.Generic;
using Jaison.Exceptions;

namespace Jaison.Containers
{
    public class SealedList : List<object>, ISealableList
    {
        public new void Add(Object e)
        {
            Check();
            base.Add(e);
        }

        public new bool Remove(Object o)
        {
            Check();
            return base.Remove(o);
        }

        public new void AddRange(IEnumerable<object> c)
        {
            Check();
            base.AddRange(c);
        }

        public new void RemoveAll(Predicate<object> c)
        {
            Check();
            base.RemoveAll(c);
        }

        public new void Clear()
        {
            Check();
            base.Clear();
        }

        public new void Insert(int index, object element)
        {
            Check();
            base.Insert(index, element);
        }

        public void Seal()
        {
            sealed_ = true;
        }

        protected void Check()
        {
            if (sealed_)
            {
                throw new ImmutableException("List is read only");
            }
        }

        //private static readonly long serialVersionUID = -2252514607541446347L;
        private bool sealed_;

    }

}
