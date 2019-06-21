# Jaison
A simple but fast C# Json parser and writer that (de)serializes from/to generic Dictionaries and Lists

This is my first C# project that I started actually only in order to learn this language.

Example:

	    string JSON_STRING = "{ \"string\": \"value\" }";

            Jai j = new Jai();
            IDictionary<string, object> dict = j.Deserialize<IDictionary<string, object>>("{ \"a\": 1 }");

            dict.Add("b", 2);
            string json = j.Serialize(dict);
            Console.WriteLine("actual: " + actual);


