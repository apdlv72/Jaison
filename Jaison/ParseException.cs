namespace Jaison.Exceptions
{
    public class ParseException : JaisonException
    {
        public ParseException(string message, int pos, int line) : base(message)
        {
            this.pos = pos;
            this.line = line;
        }

        public int pos;
        public int line;
    }
}
