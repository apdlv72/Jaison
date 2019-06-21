using System;
using System.Reflection;
using System.Diagnostics;
using System.Collections.Generic;

namespace Jaison
{
    public class TestJaison
    {
        static readonly string JSON_STRING =
            "{ \"string\": \"value\" }";

        private readonly string JSON_LISTS = 
            "{\n" +
            "  \"string\":[\n" +
            "    \"one\",\n" +
            "    \"two\"\n" +
            "  ],\n" +
            "  \"ints\":[\n" +
            "    1,\n" +
            "    2\n" +
            "  ],\n" +
            "  \"longs\":[\n" +
            "    1,\n" +
            "    1\n" +
            "  ],\n" +
            "  \"floats\":[\n" +
            "    1,\n" +
            "    2\n" +
            "  ],\n" +
            "  \"objects\":[\n" +
            "    \"one\",\n" +
            "    2,\n" +
            "    3.0001\n" +
            "  ]\n" +
            "}";


        // TODO: Need top learn standards for unti tests in C#
        static void Main()
        {
            TestJaison tester = new TestJaison();
            MethodInfo[] methods = tester
                .GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            int failed = 0, total = 0;
            foreach (MethodInfo m in methods)
            {                
                try
                {
                    total++;
                    m.Invoke(tester, null);
                    Console.WriteLine("OK: " + total + ": " + m.Name);
                }
                catch (SystemException e)
                {
                    Console.WriteLine("ERROR: " + total + ": " + m.Name + ": " + e);
                    failed++;
                }
            }
            if (failed<1)
            {
                Console.WriteLine("SUCCESS");
            }
            else
            {
                Console.WriteLine("FAILED: " + failed + " of " + total + " tests");
            }
        }

        public void testString()
        {
            Jai jai = new Jai();

            string actual = jai.Serialize("te\"st");
            string expected = "\"te\\\"st\"";

            // Console.WriteLine("actual: " + actual);
            AssertEquals(expected, actual);
        }

        public void testList()
        {
            Jai jai = new Jai();

            List<string> list = new List<string>();
            list.Add("one");
            list.Add("two");
            list.Add("three");

            string actual = jai.Serialize(list);
            //Console.WriteLine("actual: " + actual);

            string expected = "" +
            "[\n" +
            "  \"one\",\n" +
            "  \"two\",\n" +
            "  \"three\"\n" +
            "]";
            AssertEquals(expected, actual);
        }

        public void testUnsorted()
        {
            Jai j = new Jai();
            IDictionary<string, object> dict = j.Deserialize<IDictionary<string, object>>(JSON_STRING);

            dict.Add("a", 2);
            string actual = j.Serialize(dict);
            //Console.WriteLine("actual: " + actual);

            string expected = "" +
            "{\n" +
            "  \"string\":\"value\",\n" +
            "  \"a\":2\n" +
            "}";
            AssertEquals(expected, actual);
        }

        public void testLists()
        {
            Jai j = new Jai();
            IDictionary<string, object> dict = new Dictionary<string, object>();

            List<string> strings = new List<string>();
            List<int> ints = new List<int>();
            List<long> longs = new List<long>();
            List<float> floats = new List<float>();
            List<object> objects = new List<object>();

            strings.Add("one");
            strings.Add("two");
            ints.Add(1);
            ints.Add(2);
            longs.Add(1);
            longs.Add(1);
            floats.Add(1);
            floats.Add(2);
            objects.Add("one");
            objects.Add(2);
            objects.Add(3.0001);

            dict.Add("string", strings);
            dict.Add("ints", ints);
            dict.Add("longs", longs);
            dict.Add("floats", floats);
            dict.Add("objects", objects);

            string actual = j.Serialize(dict);
            string expected = JSON_LISTS;

            //Console.WriteLine("actual: " + actual);
            AssertEquals(expected, actual);
        }

        public void testPerformance() {

            Jai j = new Jai();
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            object output = null;
            int iterations = 100 * 1000;
            for (int i=0; i<iterations; i++)
            {
                output = j.Deserialize<object>(JSON_LISTS);
            }

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;

            string elapsedTime = String.Format("{0} iterations in {1:00}:{2:00}:{3:00}.{4:00}",
                        iterations,
                        ts.Hours, ts.Minutes, ts.Seconds,
                        ts.Milliseconds / 10);
            Console.WriteLine("testPerformance:  " + elapsedTime);
        }

        public void testSorted()
        {
            Jai j = new Jai().WithSorted();
            IDictionary<string, object> dict = new Dictionary<string, object>();

            List<string> list = new List<string>();
            list.Add("one");
            list.Add("two");
            list.Add("three");

            int[] array = { 1, 2, 3 };

            dict.Add("list", list);
            dict.Add("array", array);

            dict.Add("long", 9999999999999999L);
            dict.Add("int", 2);
            dict.Add("double", 47.11);
            dict.Add("float", 47.11f);
            dict.Add("bool", true);
            dict.Add("null", null);

            string actual = j.Serialize(dict);
            //Console.WriteLine("actual: " + actual);

            string expected = "" +
            "{\n" +
            "  \"list\":[\n" +
            "    \"one\",\n" +
            "    \"two\",\n" +
            "    \"three\"\n" +
            "  ],\n" +
            "  \"array\":[\n" +
            "    1,\n" +
            "    2,\n" +
            "    3\n" +
            "  ],\n" +
            "  \"long\":9999999999999999,\n" +
            "  \"int\":2,\n" +
            "  \"double\":47.11,\n" +
            "  \"float\":47.11,\n" +
            "  \"bool\":True,\n" +
            "  \"null\":null\n" +
            "}";

            AssertEquals(expected, actual);
        }

        private void AssertEquals(string expected, string actual)
        {
            if (!expected.Equals(actual))
            {
                throw new SystemException("Expected value\n'" + expected + "',\nbut actual was \n'" + actual + "'");
            }
        }
    }
}
