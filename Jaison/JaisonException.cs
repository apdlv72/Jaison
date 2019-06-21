using System;

namespace Jaison.Exceptions
{
    public class JaisonException : SystemException
    {
        public JaisonException(string message) : base(message)
        {
        }
    }
}
