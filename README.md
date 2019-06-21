# Jaison
A simple but fast C# Json parser and writer that (de)serializes from/to generic Dictionaries and Lists

This is my first C# project that I started actually only in order to learn this language.

Don't use JaisonParser but the main class Jai if you need thread safety. 
Jai will maintain a pool of parser instances in thread local storage because JaisonParser was designed for speed and is not thread safe.

Example:


    string JSON_STRING = "{ \"string\": \"value\" }";

    Jai j = new Jai().withSorted(true).withImmutable(false);
    IDictionary<string, object> dict = j.Deserialize<IDictionary<string, object>>("{ \"a\": 1 }");

    dict.Add("b", 2);
    string json = j.Serialize(dict);
    Console.WriteLine("json: " + actual);


