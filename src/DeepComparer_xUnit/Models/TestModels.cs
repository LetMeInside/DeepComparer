using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DeepComparer_xUnit.Models
{
    public class Person
    {
        public string Name { get; set; } = "";
        public int Age { get; set; }
        public List<Person> Friends { get; set; } = new();
    }

    public class WithCollections
    {
        public List<int> Numbers { get; set; } = new();
        public HashSet<string> Tags { get; set; } = new();
        public Dictionary<string, int> Scores { get; set; } = new();
        // Simulate DynamicData SourceList by exposing Items:
        public FakeSourceList<string> FakeList { get; set; } = new();
        // Simulate DynamicData SourceCache by exposing Values:
        public FakeSourceCache<string, int> FakeCache { get; set; } = new();
    }

    public class WithVisibility
    {
        public int PublicInt { get; set; } = 1;

        private int PrivateInt { get; set; } = 2;
        protected string ProtectedString { get; set; } = "p";

        public void SetPrivate(int v) => PrivateInt = v;
        public void SetProtected(string s) => ProtectedString = s;
    }

    public class WithFields
    {
        public int PublicField = 5;
        private int PrivateField = 7;

        public void SetPrivateField(int v) => PrivateField = v;
    }

    public class WithJsonIgnore
    {
        [JsonIgnore]
        public string Secret { get; set; } = "shh";

        public string Visible { get; set; } = "hello";
    }

    public class Node
    {
        public int Value { get; set; }
        public Node? Next { get; set; }
    }

    public class WithWeak
    {
        public WeakReference? TargetRef { get; set; }
        public WeakReference<Person>? GenericRef { get; set; }
        public string Marker { get; set; } = "";
    }

    // Minimal shims to emulate Items/Values enumerable patterns
    public class FakeSourceList<T>
    {
        public List<T> Items { get; } = new();
    }

    public class FakeSourceCache<TKey, TValue>
    {
        public Dictionary<TKey, TValue> Inner { get; } = new();
        public IEnumerable<TValue> Values => Inner.Values;
    }
}
