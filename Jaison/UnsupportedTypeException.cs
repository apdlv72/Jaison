using System;

namespace Jaison.Exceptions
{
    public class UnsupportedTypeException : JaisonException
    {
        public Type type;
        public string message;

        public UnsupportedTypeException(Type type, string message) : base(message)
        {
            this.type = type;
            this.message = message;
        }
    }
}
