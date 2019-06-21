using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Jaison
{
    public class JaisonWriter
    {
        public JaisonWriter(int indent)
        {
            Reset(null, indent);
        }

        public JaisonWriter(object input, int indent)
        {
            Reset(input, indent);
        }

        public string Serialize()
        {

            int indent = 0;

            if (null == input)
            {
                return null;
            }
            else if (input is string)
            {
                Escape((string)input);
                //return "\"" + escape((string) input) + "\"";
            }
            else if (input is IDictionary<string, object>)
            {
                IDictionary<string, object> dict = (IDictionary<string, object>)input;
                WriteDictionary(indent, dict);
            }
            else if (input is ICollection)
            {
                WriteCollection(indent, (ICollection)input);
            }
            else if (input is object[])
            {
                List<object> list = new List<object>();
                foreach (object o in (object[])input)
                {
                    list.Add(o);
                }
                WriteCollection(indent, list);
            }
            else
            {
                WriteObject(indent, "null", input);
            }

            string rtv = output.ToString();
            input = null;
            return rtv;
        }

        public void Reset(object input, int indent)
        {
            this.input = input;
            this.pretty = (this.indentSize = indent) > 0;
            if (null == this.output)
            {
                this.output = new StringBuilder(BUFFER_SIZE);
            }
            else
            {
                if (this.output.Capacity > BUFFER_SIZE)
                {
                    this.output = new StringBuilder(BUFFER_SIZE);
                }
                this.output.Clear();
            }
        }

        protected void WriteDictionary(int indent, IDictionary<string, object> map)
        {

            output.Append('{');
            if (pretty)
            {
                output.Append('\n');
            }

            if (null == map)
            {
                output.Append("null");
                return;
            }

            ICollection<string> keys = map.Keys;

            int newIndent = indent + indentSize;
            bool first = true;
            foreach (string key in keys)
            {

                if (first)
                {
                    first = false;
                }
                else
                {
                    output.Append(',');
                    if (pretty)
                    {
                        output.Append('\n');
                    }
                }
                if (pretty)
                {
                    AppendIndent(newIndent);
                }
                Escape(key);
                output.Append(':');

                object value = map[key];

                WriteObject(newIndent, key, value);
            }

            if (pretty)
            {
                output.Append('\n');
                AppendIndent(indent);
            }
            output.Append('}');
        }

        protected void WriteCollection(int indent, ICollection collection)
        {

            output.Append('[');
            if (pretty)
            {
                output.Append('\n');
            }

            int newIndent = indent + indentSize;
            bool first = true;
            int index = 0;
            foreach (object o in collection)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    output.Append(',');
                    if (pretty)
                    {
                        output.Append('\n');
                    }
                }
                if (pretty)
                {
                    AppendIndent(newIndent);
                }
                first = false;
                WriteObject(newIndent, "" + (++index), o);
            }

            if (pretty)
            {
                output.Append('\n');
                AppendIndent(indent);
            }
            output.Append(']');
        }

        private void WriteObject(int indent, string key, object value)
        {
            if (value is IDictionary<string, object>)
            {
                IDictionary<string, object> submap = (IDictionary<string, object>)value;
                WriteDictionary(indent, submap);
            }
            else if (value is ICollection)
            {
                ICollection coll = (ICollection)value;
                WriteCollection(indent, coll);
            }
            else if (value is string)
            {
                string s = (string)value;
                Escape(s);
            }
            else if (value is int)
            {
                output.Append((int)value);
            }
            else if (value is long)
            {
                output.Append((long)value);
            }
            else if (value is bool)
            {
                output.Append((bool)value);
            }
            else if (value is double)
            {
                output.Append((double)value);
            }
            else if (value is float)
            {
                output.Append((float)value);
            }
            //else if (value instanceof JsonSerializable) {
            //JsonSerializable js = (JsonSerializable)value;
            // Map<string, object> submap = js.tostringobjectMap();
            //writeMap(indent, submap);
            //}
            //else if (value instanceof Serializable) {
            //output.Append("" + value);
            // }
            else if (null == value)
            {
                output.Append("null");
            }
            //else if (input instanceof Jsonobject) {
            //    throw new RuntimeException("Unsupported type " + input);
            //}
            else
            {
                throw new UnsupportedTypeException(this.GetType(), "Unexpected value " + value.GetType() + " for key '" + key + "'");
            }
        }

        private void AppendIndent(int indent)
        {
            if (indent < 1)
            {
                return;
            }
            else if (indent > MAX_INDENT)
            {
                indent = MAX_INDENT;
            }
            // Yes .... astonishing .... but adding indentation character by character
            // seems indeed be quicker than adding as a chunk of characters. Why?
            for (int i = 0; i < indent; i++)
            {
                output.Append(' ');
            }
            //output.Append(INDENTATION, 0, indent);
        }


        private void EscapeCharWise(string input)
        {

            char[] inp = input.ToCharArray();
            int len = inp.Length;
            //char[] out = new char[in.length*2];

            //int  o = 0;
            char c = '\x00';

            output.Append('"');

            for (int i = 0; i < len; i++)
            {
                c = inp[i];
                switch (c)
                {
                    case '\n':
                        output.Append('\\');
                        output.Append('n');
                        //LF++;
                        break;
                    case '"':
                        output.Append('\\');
                        output.Append('"');
                        //DQ++;
                        break;
                    case '\\':
                        output.Append('\\');
                        output.Append('\\');
                        //SL++;
                        break;
                    case '\r':
                        output.Append('\\');
                        output.Append('r');
                        //CR++;
                        break;
                    case '\t':
                        // TAB
                        output.Append('\\');
                        output.Append('t');
                        break;
                    default:
                        output.Append(c);
                        break;
                }
            }

            output.Append('"');
        }

        private void Escape(string input)
        {
            char[] inp = input.ToCharArray();
            int len = inp.Length;
            char[] outp = new char[inp.Length * 2];

            int o = 0;
            char c = '\x00';

            for (int i = 0; i < len; i++)
            {
                c = inp[i];
                switch (c)
                {
                    case '\n':
                        outp[o++] = '\\';
                        outp[o++] = 'n';
                        //LF++;
                        break;
                    case '"':
                        outp[o++] = '\\';
                        outp[o++] = '"';
                        //DQ++;
                        break;
                    case '\\':
                        outp[o++] = '\\';
                        outp[o++] = '\\';
                        //SL++;
                        break;
                    case '\r':
                        outp[o++] = '\\';
                        outp[o++] = 'r';
                        //CR++;
                        break;
                    case '\t':
                        // TAB
                        outp[o++] = ('\\');
                        outp[o++] = ('t');
                        break;
                    default:
                        outp[o++] = c;
                        break;
                }
            }

            output.Append('"').Append(outp, 0, o).Append('"');
        }

        private int indentSize = 4;
        private bool pretty;
        private StringBuilder output;
        private object input;

        public static readonly int BUFFER_SIZE = 10 * 1024;

        private static readonly int MAX_INDENT = 200;

        private static readonly char[] INDENTATION;

        static JaisonWriter()
        {
            INDENTATION = new char[MAX_INDENT];
            for (int i = 0; i < MAX_INDENT; i++)
            {
                INDENTATION[i] = ' ';
            }
        }
    }

    class UnsupportedTypeException : SystemException
    {
        private object p1;
        private object p2;

        public UnsupportedTypeException(object p1, object p2)
        {
            this.p1 = p1;
            this.p2 = p2;
        }
    }
}
