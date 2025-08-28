namespace DeepComparerNS
{
    /// <summary>
    /// Specifies the action to take when the maximum recursion depth is reached
    /// during a deep object comparison.
    /// </summary>
    public enum OnMaxDepthReached
    {
        /// <summary>
        /// Treat objects at maximum depth as equal.
        /// </summary>
        TreatAsEqual,

        /// <summary>
        /// Treat objects at maximum depth as different.
        /// </summary>
        TreatAsDifferent,

        /// <summary>
        /// Log a difference entry and continue without further traversal.
        /// </summary>
        LogDifference
    }

    public enum DepthBehavior
    {
        TreatAsEqual = 0,
        TreatAsDifferent = 1,
        LogDifference = 2
    }

    /// <summary>
    /// Comparison options. Defaults match your request:
    /// MaxDepth = 20, OnMaxDepthReached = TreatAsDifferent.
    /// </summary>
    public sealed class CompareOptions
    {
        /// <summary>
        /// Maximum recursion depth. Root comparison starts at depth 0.
        /// When null, depth is unlimited. Default: 20.
        /// </summary>
        public int? MaxDepth { get; set; } = 20;

        /// <summary>
        /// What to do when MaxDepth is reached. Default: TreatAsDifferent.
        /// </summary>
        public DepthBehavior OnMaxDepthReached { get; set; } = DepthBehavior.TreatAsDifferent;

        /// <summary>
        /// Optional predicate to treat additional types as "simple" (compared via Equals).
        /// Return true to mark a type as simple.
        /// </summary>
        public Func<Type, bool>? CustomSimpleTypePredicate { get; set; }
    }
}



