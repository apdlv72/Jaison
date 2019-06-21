namespace Jaison.Exceptions
{
    public class ImmutableException : JaisonException
    {
        public string key;

        public ImmutableException(string message) : base(message)
        {
        }

        public ImmutableException(string key, string message) : this(message)
        {
            this.key = key;
        }
    }
}
