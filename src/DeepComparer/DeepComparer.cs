using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DeepComparerNS
{
    /// <summary>
    /// Provides deep comparison utilities for objects, supporting configurable
    /// reflection-based comparison of properties, fields, and collections,
    /// with caching for performance, circular reference detection, and
    /// optional detailed difference reporting.
    /// </summary>
    public static partial class DeepComparer
    {
        // ===========================
        // Public APIs (bool and detailed)
        // ===========================

        /// <summary>
        /// Compares two objects for deep equality using reflection.
        /// </summary>
        /// <typeparam name="T">Type of the objects being compared.</typeparam>
        /// <param name="obj1">First object to compare.</param>
        /// <param name="obj2">Second object to compare.</param>
        /// <param name="publicOnly">If true, only public members are compared.</param>
        /// <param name="propertiesOnly">If true, only properties (not fields) are compared.</param>
        /// <param name="jsonIgnore">If true, members marked with [JsonIgnore] are ignored.</param>
        /// <returns>True if the objects are deeply equal; otherwise, false.</returns>
        public static bool Compare<T>(
            T obj1, T obj2,
            bool publicOnly = true,
            bool propertiesOnly = true,
            bool jsonIgnore = true,
            CompareOptions? options = null)
        {
            var opts = options ?? new CompareOptions();
            var ctx = new CompareContext(publicOnly, propertiesOnly, jsonIgnore, opts);
            return CompareObjects(obj1, obj2, ctx, null, path: "", depth: 0, options: opts);
        }

        /// <summary>
        /// Compares two objects for deep equality and returns a detailed report of any differences found.
        /// </summary>
        /// <typeparam name="T">Type of the objects being compared.</typeparam>
        /// <param name="obj1">First object to compare.</param>
        /// <param name="obj2">Second object to compare.</param>
        /// <param name="publicOnly">If true, only public members are compared.</param>
        /// <param name="propertiesOnly">If true, only properties (not fields) are compared.</param>
        /// <param name="jsonIgnore">If true, members marked with [JsonIgnore] are ignored.</param>
        /// <returns>
        /// A <see cref="CompareResult"/> object containing a boolean indicating equality
        /// and a list of differences, if any.
        /// </returns>
        public static CompareResult CompareWithReport<T>(
            T obj1, T obj2,
            bool publicOnly = true,
            bool propertiesOnly = true,
            bool jsonIgnore = true,
            CompareOptions? options = null)
        {
            var opts = options ?? new CompareOptions();
            var ctx = new CompareContext(publicOnly, propertiesOnly, jsonIgnore, opts);
            var result = new CompareResult();
            result.AreEqual = CompareObjects(obj1, obj2, ctx, result.Differences, path: "", depth: 0, options: opts);
            return result;
        }

        // ===========================
        // Internal Context (per call)
        // ===========================

        /// <summary>
        /// Provides configuration and state tracking for deep object comparisons,
        /// including options for member visibility, property and field selection,
        /// JSON ignore handling, recursion depth control, and circular reference prevention.
        /// </summary>
        private sealed class CompareContext
        {
            public readonly bool PublicOnly;
            public readonly bool PropertiesOnly;
            public readonly bool JsonIgnore;

            public readonly int? MaxDepth;
            public readonly DepthBehavior OnMaxDepthReached;
            public readonly Func<Type, bool>? CustomSimpleTypePredicate;

            // Track visited (objectA, objectB) by reference to prevent infinite recursion
            private readonly HashSet<(int, int)> _visited = new();

            public CompareContext(bool publicOnly, bool propertiesOnly, bool jsonIgnore, CompareOptions opts)
            {
                PublicOnly = publicOnly;
                PropertiesOnly = propertiesOnly;
                JsonIgnore = jsonIgnore;

                MaxDepth = opts.MaxDepth;
                OnMaxDepthReached = opts.OnMaxDepthReached;
                CustomSimpleTypePredicate = opts.CustomSimpleTypePredicate;
            }

            public bool MarkVisited(object a, object b)
            {
                int ha = RuntimeHelpers.GetHashCode(a);
                int hb = RuntimeHelpers.GetHashCode(b);
                var key = ha <= hb ? (ha, hb) : (hb, ha);
                return _visited.Add(key);
            }
        }

        // ===========================
        // Static caches (thread-safe)
        // ===========================

        //private static readonly ConcurrentDictionary<Type, bool> _simpleTypeCache = new();
        private static readonly Dictionary<(Type, Func<Type, bool>?), bool> _simpleTypeCache = new();
        private static readonly ConcurrentDictionary<PropertyInfo, bool> _jsonIgnoreCache = new();

        private readonly struct MemberCacheKey : IEquatable<MemberCacheKey>
        {
            public readonly Type Type;
            public readonly bool IncludeNonPublic;
            public readonly bool IncludeFields;
            public readonly bool HonorJsonIgnore;

            public MemberCacheKey(Type type, bool includeNonPublic, bool includeFields, bool honorJsonIgnore)
            {
                Type = type;
                IncludeNonPublic = includeNonPublic;
                IncludeFields = includeFields;
                HonorJsonIgnore = honorJsonIgnore;
            }

            public bool Equals(MemberCacheKey other) =>
                Type == other.Type &&
                IncludeNonPublic == other.IncludeNonPublic &&
                IncludeFields == other.IncludeFields &&
                HonorJsonIgnore == other.HonorJsonIgnore;

            public override bool Equals(object? obj) => obj is MemberCacheKey other && Equals(other);

            public override int GetHashCode()
            {
                unchecked
                {
                    int h = Type.GetHashCode();
                    h = (h * 397) ^ (IncludeNonPublic ? 1 : 0);
                    h = (h * 397) ^ (IncludeFields ? 1 : 0);
                    h = (h * 397) ^ (HonorJsonIgnore ? 1 : 0);
                    return h;
                }
            }
        }

        private static readonly ConcurrentDictionary<MemberCacheKey, List<MemberAccessor>> _memberAccessorsCache = new();

        // For TryGetEnumerable optimization: which accessor (self/Items/Values) to use per type
        private enum EnumerableAccessKind { None, SelfEnumerable, ItemsProperty, ValuesProperty }
        private sealed class EnumerableAccessor
        {
            public EnumerableAccessKind Kind { get; }
            public Func<object, IEnumerable?> Getter { get; }

            public EnumerableAccessor(EnumerableAccessKind kind, Func<object, IEnumerable?> getter)
            {
                Kind = kind;
                Getter = getter;
            }
        }
        private static readonly ConcurrentDictionary<Type, EnumerableAccessor?> _enumerableAccessorCache = new();

        // ===========================
        // Core Comparison
        // ===========================

        private static bool CompareObjects(object? a, object? b, CompareContext ctx, List<string>? diffs, string path, int depth, CompareOptions options)
        {
            // Depth control
            if (ctx.MaxDepth.HasValue && depth > ctx.MaxDepth.Value)
            {
                return HandleMaxDepth(ctx, diffs, path);
            }

            if (ReferenceEquals(a, b)) return true;
            if (a is null || b is null)
            {
                if (diffs is not null) diffs.Add($"{path}: One is null, other is not");
                else Debug.WriteLine($"{path}: One is null, other is not");
                return false;
            }

            var ta = a.GetType();
            var tb = b.GetType();
            if (!ReferenceEquals(ta, tb))
            {
                if (diffs is not null) diffs.Add($"{path}: Types differ ({ta.FullName} vs {tb.FullName})");
                else Debug.WriteLine($"{path}: Types differ ({ta.FullName} vs {tb.FullName})");
                return false;
            }

            if (IsWeakReferenceType(ta)) return true;

            if (IsSimpleComparable(ta, ctx))
            {
                if (!Equals(a, b))
                {
                    var msg = $"{path}: Values differ ({FormatValue(a)} vs {FormatValue(b)})";
                    if (diffs is not null) diffs.Add(msg); else Debug.WriteLine(msg);
                    return false;
                }
                return true;
            }

            if (TryGetEnumerable(a, out var ea) | TryGetEnumerable(b, out var eb))
            {
                if (ea is null || eb is null)
                {
                    var msg = $"{path}: One is collection, other is not";
                    if (diffs is not null) diffs.Add(msg); else Debug.WriteLine(msg);
                    return false;
                }
                return CompareEnumerablesUnordered(ea, eb, ctx, diffs, path, depth + 1, options);
            }

            // Avoid cycles
            if (!ctx.MarkVisited(a, b)) return true;

            var members = GetComparableMembers(ta, ctx);
            if (members.Count == 0)
            {
                Debug.WriteLine($"[DeepComparer] Skipping unknown/non-comparable type: {ta.FullName}");
                return true; // treat as ignored
            }

            bool allEqual = true;
            foreach (var m in members)
            {
                string subPath = string.IsNullOrEmpty(path) ? m.Name : $"{path}.{m.Name}";
                object? va = null, vb = null;
                try { va = m.Getter(a); vb = m.Getter(b); }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[DeepComparer] Failed to read '{m.Name}' on {ta.FullName}: {ex.Message}. Skipping.");
                    continue;
                }

                if (va is not null && IsWeakReferenceType(va.GetType())) continue;
                if (vb is not null && IsWeakReferenceType(vb.GetType())) continue;

                if (!CompareObjects(va, vb, ctx, diffs, subPath, depth + 1, options))
                    allEqual = false;
            }

            return allEqual;
        }

        private static bool HandleMaxDepth(CompareContext ctx, List<string>? diffs, string path)
        {
            switch (ctx.OnMaxDepthReached)
            {
                case DepthBehavior.TreatAsEqual:
                    if (diffs is null) Debug.WriteLine($"{path}: Depth limit reached, treating as equal");
                    return true;

                case DepthBehavior.TreatAsDifferent:
                    if (diffs is not null) diffs.Add($"{path}: Depth limit ({ctx.MaxDepth}) reached");
                    else Debug.WriteLine($"{path}: Depth limit ({ctx.MaxDepth}) reached (treating as different)");
                    return false;

                case DepthBehavior.LogDifference:
                    if (diffs is not null) diffs.Add($"{path}: Depth limit ({ctx.MaxDepth}) reached");
                    else Debug.WriteLine($"{path}: Depth limit ({ctx.MaxDepth}) reached");
                    return true; // equality not determinable, but per option we don't fail
            }

            // Fallback (shouldn't hit)
            return false;
        }

        private static bool CompareEnumerablesUnordered(IEnumerable a, IEnumerable b, CompareContext ctx, List<string>? diffs, string path, int depth, CompareOptions options)
        {
            if (ctx.MaxDepth.HasValue && depth > ctx.MaxDepth.Value)
                return HandleMaxDepth(ctx, diffs, path);

            // Materialize once
            var listA = a.Cast<object?>().ToList();
            var listB = b.Cast<object?>().ToList();

            if (listA.Count != listB.Count)
            {
                var msg = $"{path}: Collection counts differ ({listA.Count} vs {listB.Count})";
                if (diffs is not null) diffs.Add(msg); else Debug.WriteLine(msg);
                return false;
            }
            if (listA.Count == 0) return true;

            // Fast path for all-simple items: multiset by value
            if (listA.All(o => o is null || IsSimpleComparable(o.GetType(), ctx) || (o is not null && (ctx.CustomSimpleTypePredicate?.Invoke(o.GetType()) ?? false))) &&
                listB.All(o => o is null || IsSimpleComparable(o.GetType(), ctx) || (o is not null && (ctx.CustomSimpleTypePredicate?.Invoke(o.GetType()) ?? false))))
            {
                var countsA = BuildMultiset(listA);
                var countsB = BuildMultiset(listB);
                if (countsA.Count != countsB.Count) return false;
                foreach (var kv in countsA)
                {
                    if (!countsB.TryGetValue(kv.Key, out int cb) || cb != kv.Value)
                    {
                        var msg = $"{path}: Collection item multiset differs";
                        if (diffs is not null) diffs.Add(msg); else Debug.WriteLine(msg);
                        return false;
                    }
                }
                return true;
            }

            // General case: order-insensitive greedy O(n^2)
            var used = new bool[listB.Count];
            bool allEqual = true;

            for (int i = 0; i < listA.Count; i++)
            {
                bool matched = false;
                for (int j = 0; j < listB.Count; j++)
                {
                    if (used[j]) continue;
                    if (CompareObjects(listA[i], listB[j], ctx, diffs, $"{path}[{i}]", depth + 1, options))
                    {
                        used[j] = true;
                        matched = true;
                        break;
                    }
                }
                if (!matched)
                {
                    var msg = $"{path}[{i}]: No matching item found";
                    if (diffs is not null) diffs.Add(msg); else Debug.WriteLine(msg);
                    allEqual = false;
                }
            }

            return allEqual;

            static Dictionary<object?, int> BuildMultiset(IEnumerable<object?> items)
            {
                var dict = new Dictionary<object?, int>(new NullSafeEqualityComparer());
                foreach (var it in items)
                {
                    dict.TryGetValue(it, out int c);
                    dict[it] = c + 1;
                }
                return dict;
            }
        }

        // ===========================
        // Member Discovery (cached)
        // ===========================

        private sealed record MemberAccessor(string Name, Func<object, object?> Getter);

        private static List<MemberAccessor> GetComparableMembers(Type t, CompareContext ctx)
        {
            var key = new MemberCacheKey(
                t,
                includeNonPublic: !ctx.PublicOnly,
                includeFields: !ctx.PropertiesOnly,
                honorJsonIgnore: ctx.JsonIgnore);

            return _memberAccessorsCache.GetOrAdd(key, static (k) =>
            {
                var result = new List<MemberAccessor>();
                var flags = BindingFlags.Instance | BindingFlags.Public;
                if (k.IncludeNonPublic) flags |= BindingFlags.NonPublic;

                // PROPERTIES
                foreach (var p in k.Type.GetProperties(flags))
                {
                    if (p.GetIndexParameters().Length != 0) continue;
                    var getter = p.GetGetMethod(nonPublic: true);
                    if (getter == null) continue;
                    if (!k.IncludeNonPublic && !getter.IsPublic) continue;
                    if (getter.IsStatic) continue;

                    if (k.HonorJsonIgnore && HasNewtonsoftJsonIgnoreCached(p)) continue;
                    if (typeof(Delegate).IsAssignableFrom(p.PropertyType)) continue;

                    result.Add(new MemberAccessor(p.Name, BuildUntypedGetterForProperty(p)));
                }

                // FIELDS
                if (k.IncludeFields)
                {
                    foreach (var f in k.Type.GetFields(flags))
                    {
                        if (!k.IncludeNonPublic && !f.IsPublic) continue;
                        if (f.IsStatic) continue;
                        if (typeof(Delegate).IsAssignableFrom(f.FieldType)) continue;

                        result.Add(new MemberAccessor(f.Name, BuildUntypedGetterForField(f)));
                    }
                }

                return result;
            });
        }

        // Build cached untyped getters via expression trees (fast & allocations minimized)
        private static Func<object, object?> BuildUntypedGetterForProperty(PropertyInfo p)
        {
            try
            {
                var instance = Expression.Parameter(typeof(object), "obj");
                var cast = Expression.Convert(instance, p.DeclaringType!);
                var call = Expression.Property(cast, p);
                var box = Expression.Convert(call, typeof(object));
                var lambda = Expression.Lambda<Func<object, object?>>(box, instance);
                return lambda.Compile();
            }
            catch
            {
                // Fallback to reflection if expression compilation fails (rare)
                return obj => SafeGet(() => p.GetValue(obj));
            }
        }

        private static Func<object, object?> BuildUntypedGetterForField(FieldInfo f)
        {
            try
            {
                var instance = Expression.Parameter(typeof(object), "obj");
                var cast = Expression.Convert(instance, f.DeclaringType!);
                var fld = Expression.Field(cast, f);
                var box = Expression.Convert(fld, typeof(object));
                var lambda = Expression.Lambda<Func<object, object?>>(box, instance);
                return lambda.Compile();
            }
            catch
            {
                return obj => SafeGet(() => f.GetValue(obj));
            }
        }

        private static object? SafeGet(Func<object?> getter)
        {
            try { return getter(); }
            catch { return null; }
        }

        // ===========================
        // Attribute check (cached)
        // ===========================

        private static bool HasNewtonsoftJsonIgnoreCached(PropertyInfo p) =>
            _jsonIgnoreCache.GetOrAdd(p, static prop =>
            {
                try
                {
                    foreach (var a in prop.GetCustomAttributes(true))
                    {
                        if (a.GetType().FullName == "Newtonsoft.Json.JsonIgnoreAttribute")
                            return true;
                    }
                }
                catch { /* ignore */
}
return false;
            });

        // ===========================
        // Enumerable detection (cached)
        // ===========================

        private static bool TryGetEnumerable(object obj, out IEnumerable? enumerable)
        {
            var t = obj.GetType();

            var acc = _enumerableAccessorCache.GetOrAdd(t, static tt =>
            {
                // string is IEnumerable<char> — explicitly exclude
                if (typeof(IEnumerable).IsAssignableFrom(tt) && tt != typeof(string))
                {
                    return new EnumerableAccessor(
                        EnumerableAccessKind.SelfEnumerable,
                        o => (IEnumerable?)o // already IEnumerable
                    );
                }

                // Try Items / Values pattern (e.g., DynamicData SourceList/SourceCache)
                foreach (var name in _itemsOrValuesPropertyNames)
                {
                    var prop = tt.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
                    if (prop != null && typeof(IEnumerable).IsAssignableFrom(prop.PropertyType))
                    {
                        // Create fast getter
                        var getter = BuildEnumerablePropertyGetter(prop);
                        return new EnumerableAccessor(
                            name == "Items" ? EnumerableAccessKind.ItemsProperty : EnumerableAccessKind.ValuesProperty,
                            getter
                        );
                    }
                }

                return null; // not enumerable-like
            });

            if (acc is null)
            {
                enumerable = null;
                return false;
            }

            try
            {
                enumerable = acc.Kind switch
                {
                    EnumerableAccessKind.SelfEnumerable => (IEnumerable)obj,
                    _ => acc.Getter(obj)
                };
                return enumerable is not null;
            }
            catch
            {
                enumerable = null;
                return false;
            }
        }

        private static readonly string[] _itemsOrValuesPropertyNames = new[] { "Items", "Values" };

        private static Func<object, IEnumerable?> BuildEnumerablePropertyGetter(PropertyInfo p)
        {
            try
            {
                var instance = Expression.Parameter(typeof(object), "obj");
                var cast = Expression.Convert(instance, p.DeclaringType!);
                var call = Expression.Property(cast, p);
                var box = Expression.Convert(call, typeof(IEnumerable));
                var lambda = Expression.Lambda<Func<object, IEnumerable?>>(box, instance);
                return lambda.Compile();
            }
            catch
            {
                return obj => p.GetValue(obj) as IEnumerable;
            }
        }

        // ===========================
        // Type helpers (cached)
        // ===========================

        private static bool IsWeakReferenceType(Type t) =>
            t == typeof(WeakReference) ||
            (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(WeakReference<>));

        // Thread-safe cache for built-in "simple" type determination (predicate-independent)
        private static readonly ConcurrentDictionary<Type, bool> _builtinSimpleTypeCache = new();

        private static bool IsSimpleComparable(Type t, CompareContext ctx)
        {
            // 1) Honor custom predicate first (do NOT cache per-delegate to avoid key explosion)
            if (ctx.CustomSimpleTypePredicate?.Invoke(t) == true)
                return true;

            // 2) Built-in rules, cached once per Type
            return _builtinSimpleTypeCache.GetOrAdd(t, static tt =>
            {
                if (tt.IsPrimitive || tt.IsEnum) return true;

                if (tt == typeof(string) || tt == typeof(decimal) ||
                    tt == typeof(DateTime) || tt == typeof(DateTimeOffset) ||
                    tt == typeof(TimeSpan) || tt == typeof(Guid))
                    return true;

                var ut = Nullable.GetUnderlyingType(tt);
                if (ut is not null)
                {
                    if (ut.IsPrimitive || ut.IsEnum) return true;
                    if (ut == typeof(decimal) || ut == typeof(DateTime) ||
                        ut == typeof(DateTimeOffset) || ut == typeof(TimeSpan) || ut == typeof(Guid))
                        return true;
                }

                // WPF/BCL structs (value types) typically have reliable Equals
                if (tt.IsValueType) return true;

                // Reference types that implement IComparable — assume stable value semantics
                if (typeof(IComparable).IsAssignableFrom(tt)) return true;

                return false;
            });
        }

        private sealed class NullSafeEqualityComparer : IEqualityComparer<object?>
        {
            public new bool Equals(object? x, object? y) => object.Equals(x, y);
            public int GetHashCode(object? obj) => obj?.GetHashCode() ?? 0;
        }

        // ===========================
        // Utility
        // ===========================

        private static string FormatValue(object? v)
        {
            if (v is null) return "null";
            return v switch
            {
                string s => $"\"{s}\"",
                DateTime dt => dt.ToString("O"),
                DateTimeOffset dto => dto.ToString("O"),
                _ => v.ToString() ?? v.GetType().Name
            };
        }
    }
}



