﻿using System;
using System.Collections.Generic;
using System.Text;
using Jaison.Exceptions;
using Jaison.Containers;

namespace Jaison
{
    public class JaisonParser
    {
        // TODO: Might be usefull to make MAX_BUFFER_SIZE configurable.
        public static int MAX_BUFFER_SIZE = 10 * 1024; // 10k

        public JaisonParser(string json, bool immutable, bool ordered, bool sorted, bool strict, Dictionary<string, object> vars)
        {
            Reset(json, immutable, ordered, sorted, strict, vars);
        }

        public JaisonParser(bool immutable, bool ordered, bool sorted, bool strict, Dictionary<string, object> vars)
        {
            Reset("{}", immutable, ordered, sorted, strict, vars);
        }

        public JaisonParser Reset(string json, bool immutable, bool ordered, bool sorted, bool strict, Dictionary<string, object> vars)
        {
            this.immutable = immutable;
            this.ordered = ordered;
            this.sorted = sorted;
            this.strict = strict;
            this.variables = vars;
            this.pos = 0;
            this.line = 0;
            this.data = json.ToCharArray();

            // allocate an byte aray that is as large as the whole josn string (worst case);
            if (null != json)
            {
                int size = json.Length;
                if (size > MAX_BUFFER_SIZE)
                {
                    size = MAX_BUFFER_SIZE;
                }
                if (null == this.sb)
                {
                    this.sb = new StringBuilder(size);
                }
            }
            this.sb.Clear();
            return this;
        }

        /**
         * Parse the json string passed in the constructor and return a plain java objects representation.
         * @return A Map, List, string, Long, Integer, Double, bool or null;
         */
        public Object Parse()
        {
            // Call the internal, invisible method.
            Object rtv = DeserializeObject("TOP");
            // And forget about the data afterwards to allow garbage collection ASAP.
            data = null;
            // also deallocate the buffer if it exceeded MAX_BUFFER_SIZE
            if (this.sb.Length > MAX_BUFFER_SIZE)
            {
                sb = null;
            }
            return rtv;
        }

        protected object DeserializeObject(string name)
        {

            char c = SkipWhitespace();
            if ('{' == c)
            {
                return ParseStruct(name);
            }
            else if ('[' == c)
            {
                return ParseArray();
            }
            else if ('"' == c)
            {
                return Parsestring();
            }
            else if (('0' <= c && c <= '9') || '-' == c)
            {
                return ParseNumber();
            }
            // [t]rue, [f]alse, [n]ull
            else if (null != variables || ('t' == c || 'f' == c || 'n' == c))
            {
                return ParseSpecial();
            }
            else if (!strict)
            {
                // TODO: Allow strings with single quotes instead of double quotes or no quotres a t all.
                // TODO: Allow variables e.g. for Jinjamarena DoTag (lookup values from variables);
                Fail(DESERIALIZE_METHOD);
            }
            else
            {
                Fail(DESERIALIZE_METHOD);
            }
            return null;
        }

        protected ISealableDictionary ParseStruct(string name)
        {

            char c = (char)data[pos];
            if ('{' != c) Fail(PARSE_STRUCT_METHOD);
            pos++;

            ISealableDictionary map = newDict();
            bool first = true;
            do
            {
                c = SkipWhitespace();
                if ('}' == c)
                {
                    pos++;
                    break;
                }

                if (!first)
                {
                    c = SkipWhitespace();
                    if (',' != c) Fail(PARSE_STRUCT_METHOD);
                    pos++;
                }
                first = false;

                c = SkipWhitespace();
                // allow common error: { "key1": "val1", "key2", "val2", }
                if (!strict && '}' == c)
                {
                    pos++;
                    break;
                }
                if ('"' != c) Fail(PARSE_STRUCT_METHOD);
                string key = Parsestring();
                if (strict && map.ContainsKey(key))
                {
                    throw new ParseException("Key appeared more than once in " + name + ": \"" + key + "\"", pos, line);
                }

                c = SkipWhitespace();
                if (':' != c) Fail(PARSE_STRUCT_METHOD);
                pos++;
                Object value = DeserializeObject(key);

                //System.out.println("parseStruct(" + name + "): " + key  + " -> " + value);
                map.Add(key, value);
            }
            while (true);

            map.Seal();
            return map;
        }

        protected ISealableList ParseArray()
        {

            var list = newList();
            pos++; // skip [
            bool first = true;
            int index = 0;
            do
            {
                char c = SkipWhitespace();

                if (']' == c)
                {
                    break;
                }
                if (!first)
                {
                    c = SkipWhitespace();
                    if (',' != c) Fail(PARSE_ARRAY_METHOD);
                    pos++;
                    c = SkipWhitespace();
                }
                first = false;

                object element = DeserializeObject("" + (index++));
                list.Add(element);
            }
            while (true);

            pos++;

            // TODO: list.seal
            //list.seal();
            return list;
        }

        protected string Parsestring()
        {

            char c = data[pos++]; // skip '"'
            if ('"' != c) Fail(PARSE_STRUCT_METHOD);

            int initial = pos;
            int start = pos;
            int len = data.Length;
            bool reset = false;
            bool done = false;
            while (!done)
            {

                int i;
                for (i = pos; i < len; i++)
                {
                    c = data[i];
                    if ('"' == c || '\\' == c)
                    {
                        break;
                    }
                }

                // found end of string before any escaped character
                if ('"' == c)
                {
                    pos = i + 1;

                    if (initial == start)
                    {
                        return new string(data, start, i - start);
                    }

                    sb.Append(data, start, i - start);
                    return sb.ToString();
                }

                if (!reset)
                {
                    sb.Clear();
                    reset = true;
                }

                sb.Append(data, start, i - start);
                pos = i;

                // otherwise we need to handle an escaped sequence
                c = data[++pos];

                switch (c)
                {
                    case 'u': // utf-16 character
                        string hex = new string(data, pos + 1, 4);
                        pos += 4;
                        int code = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);
                        byte[] bytes1 = new byte[2];
                        bytes1[0] = (byte)(code / 256);
                        bytes1[1] = (byte)(code % 256);

                        string utf = Encoding.UTF8.GetString(bytes1);
                        sb.Append(utf);
                        break;
                    case 'n': sb.Append('\n'); break;
                    case 'r': sb.Append('\r'); break;
                    case 't': sb.Append('\t'); break;
                    case '"': sb.Append('"'); break;
                    case '\\': sb.Append('\\'); break;
                    default: sb.Append(c); break;
                }

                c = data[++pos];
                start = pos;
            }

            return sb.ToString();
        }

        /**
         * This method adds characters to resulting stringBuildr one by one because while
         * checking for escape sequences. In contract to parsestring() it is 30% slower.
         * @return
         */
        protected string ParseStringCharWise()
        {

            sb.Clear();
            pos++;

            char c = '\x0';

            if (checkEscape)
            {
                // detect if we need to care of escaping ...
                int i;
                for (i = pos; i < data.Length; i++)
                {
                    c = data[i];
                    if ('"' == c || '\\' == c)
                    {
                        break;
                    }
                }

                // no backslash found before closing double quotes -> no escaping used in string
                if ('"' == c)
                {

                    string s = new string(data, pos, i - pos);
                    pos = i + 1;
                    return s;
                }
            }

            bool done = false;
            bool escape = false;
            while (!done)
            {
                c = data[pos];

                if (escape)
                {
                    switch (c)
                    {
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        default: sb.Append(c); break;
                    }
                    escape = false;
                }
                else
                {
                    switch (c)
                    {
                        case '\\':
                            escape = true;
                            break;
                        case '"':
                            done = true;
                            break;
                        default:
                            sb.Append(c);
                            break;
                    }
                }
                pos++;
            }

            string str = sb.ToString();
            return str;
        }

        protected object ParseNumber()
        {

            int start = pos;

            bool fractional = false;
            bool done = false;
            bool scientific = false;
            while (!done)
            {
                char c = data[pos];
                switch (c)
                {
                    // negative/positive numbers
                    case '-':
                    case '+':
                        // TODO: Could have better checking here ..
                        // but will throw Exception anyway below.
                        break;
                    case '.':
                        if (fractional)
                        {
                            // allow decimal dot at most once
                            Fail(PARSE_NUMBER_METHOD);
                        }
                        fractional = true;
                        break;
                    case 'e':
                    case 'E':
                        if (scientific)
                        {
                            // same for scientific notation
                            Fail(PARSE_NUMBER_METHOD);
                        }
                        fractional = scientific = true;
                        break;
                    default:
                        if (!('0' <= c && c <= '9'))
                        {
                            done = true;
                        }
                        break;
                }
                if (!done)
                {
                    pos++;
                }
            }

            string numstr = new string(data, start, pos - start);
            if (fractional)
            {
                double d = double.Parse(numstr);
                return d;
            }

            long num = 0;
            try
            {
                num = long.Parse(numstr);
                if (int.MinValue <= num && num <= int.MaxValue)
                {
                    return (int)num;
                }
            }
            catch (Exception)
            {
                Fail(PARSE_NUMBER_METHOD);
            }
            return num;
        }

        /**
         * Parse the special values "true", "false" and "null".
         * @return
         */
        protected object ParseSpecial()
        {

            int start = pos;
            char c = data[pos];
            while ('a' <= c && c <= 'z')
            {
                c = data[++pos];
            }

            string str = new string(data, start, pos - start);
            if ("false".Equals(str)) return false;
            if ("true".Equals(str)) return true;
            if ("null".Equals(str)) return null;

            if (null != variables)
            {
                Object value = variables.GetValueOrDefault(str);
                return value;
            }

            Fail(PARSE_SPECIALM_ETHOD);
            return null;
        }

        protected string GetRest()
        {
            int len = data.Length - pos;
            string s = new string(data, pos, len);
            return s;
        }

        /**
         * Skip over all whitespace characters after the current parsing position and return
         * the first non-whitespace character.
         * @return
         */
        protected char SkipWhitespace()
        {
            char c = data[pos];
            do
            {
                if ('\n' == c)
                {
                    line++;
                }
                else if (!(' ' == c || '\r' == c || '\n' == c || '\t' == c))
                {
                    return c;
                }
                try
                {
                    c = data[++pos];
                }
                catch (Exception)
                {
                    throw new ParseException("Unexpected end of input", pos, line);
                }
            }
            while (pos < data.Length);
            throw new ParseException("Unexpected end of input", pos, line);
        }

        private ISealableList newList()
        {
            if (immutable)
                return new SealedList();
            return new UnsealedList();
        }

        private ISealableDictionary newDict()
        {
            if (immutable)
            {
                if (ordered)
                {
                    return new SealedOrderedDictionary();
                }
                else if (sorted)
                {
                    return new SealedSortedDictionary();
                }
                return new SealedDictionary();
            }

            if (ordered)
            {
                return new UnsealedOrderedDictionary();
            }
            else if (sorted)
            {
                return new UnsealedSortedDictionary();
            }
            return new UnsealedDictionary();
        }

        private void Fail(string method)
        {
            int end = pos + 40;
            string dots = "";
            if (end > data.Length)
            {
                end = data.Length;
                dots = "...";
            }

            string rest = new string(data, pos, end - pos);
            string msg = string.Format(
                "{0}: Unexpected input at string index {1}, line {2} at '{3}{4}'",
                method, pos, line, rest, dots);
            throw new ParseException(msg, pos, line);
        }

        private StringBuilder sb;

        private char[] data;
        private int pos;
        private int line;
        private Dictionary<string, object> variables;

        private static readonly string DESERIALIZE_METHOD = "deserialize";
        private static readonly string PARSE_ARRAY_METHOD = "parseArray";
        private static readonly string PARSE_STRUCT_METHOD = "parseStruct";
        private static readonly string PARSE_NUMBER_METHOD = "parseNumber";
        private static readonly string PARSE_SPECIALM_ETHOD = "parseSpecial";

        private bool strict = true;
        private bool sorted;
        private bool ordered;
        private bool immutable;
        private bool checkEscape = true;
    }
}
