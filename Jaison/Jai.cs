using System;
using System.Collections.Generic;
using System.Threading;

namespace Jaison
{
    public class Jai
    {
        public Jai()
        {
            parsers = new ThreadLocal<JaisonParser>();
            writers = new ThreadLocal<JaisonWriter>();
        }

        public Jai WithImmutable(bool on = true)
        {
            immutable = on;
            return this;
        }

        public Jai WithOrdered(bool on = true)
        {
            ordered = on;
            // TODO: Implement WithOrdered() 
            throw new NotImplementedException("No generic eqivalent C# equivalent of Java's LinkedHashMap");
            //return this;
        }

        public Jai WithSorted(bool on = true)
        {
            sorted = on;
            return this;
        }

        public Jai WithStrict(bool on = true)
        {
            strict = on;
            return this;
        }

        public Jai WithIndent(int i = 2)
        {
            indent = i;
            return this;
        }

        public string Serialize(object o)
        {
            JaisonWriter w = GetWriter();
            w.Reset(o, 2);
            return w.Serialize();
        }

        public T Deserialize<T>(string json, Dictionary<string, object> vars = null)
        {
            JaisonParser parser = GetParser().Reset(json, immutable, ordered, sorted, strict, vars);
            return (T) parser.Parse();
        }

        private JaisonParser GetParser()
        {
            if (null == parsers.Value)
            {
                parsers.Value = new JaisonParser(immutable, ordered, sorted, strict, null);
            }
            return parsers.Value;
        }

        private JaisonWriter GetWriter()
        {
            if (null == writers.Value)
            {
                writers.Value = new JaisonWriter(indent);
            }
            return writers.Value;
        }

        private bool immutable, ordered, sorted, strict;
        private int indent;
        private ThreadLocal<JaisonParser> parsers;
        private ThreadLocal<JaisonWriter> writers;
    }
}
