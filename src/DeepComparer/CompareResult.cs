namespace DeepComparerNS
{

    /// <summary>
    /// Represents the result of a deep comparison between two objects,
    /// including whether they are equal and a list of any differences found.
    /// </summary>
    public class CompareResult
    {
        public bool AreEqual { get; set; }
        public List<string> Differences { get; } = new();

        public override string ToString() =>
            AreEqual ? "Objects are equal."
                        : "Objects differ:\n - " + string.Join("\n - ", Differences);
    }
}



