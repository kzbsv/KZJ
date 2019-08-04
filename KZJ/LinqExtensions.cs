using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KzL.Windows.Forms {

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IEnumerableCircular<T> {
        /// <summary>
        /// 
        /// </summary>
        T Next { get; }
        /// <summary>
        /// 
        /// </summary>
        T Prev { get; }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class LinqExtensions {

        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxLength"></param>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string LastInnerMessage(this Exception ex, int maxLength = 0) {
            var m = ex.Message ?? "Unknown Exception";
            while (ex.InnerException != null) { ex = ex.InnerException; if (!String.IsNullOrWhiteSpace(ex.Message)) m = ex.Message; }
            if (maxLength > 0) {
                var i = m.IndexOf('.');
                if (i > -1 && i < maxLength)
                    m = m.Substring(0, i);
                else if (m.Length > maxLength)
                    m = m.Substring(0, maxLength - 1) + '…';
            }
            return m;
        }

        /// <summary>
        /// Adds a single newLast item to the original enumerable sequence.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="newLast"></param>
        /// <returns></returns>
        public static IEnumerable<T> Concat<T>(this IEnumerable<T> enumerable, T newLast) {
            foreach (var item in enumerable) yield return item;
            yield return newLast;
        }

        /// <summary>
        /// Returns a sequence of at most the first keep number of items from the input sequence.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="keep"></param>
        /// <returns></returns>
        public static IEnumerable<T> First<T>(this IEnumerable<T> enumerable, int keep) {
            foreach (var item in enumerable) {
                if (keep > 0) yield return item;
                keep--;
                if (keep <= 0) break;
            }
        }

        /// <summary>
        /// Grabs items from enumerable in chunks of chunkSize, then iterates through each chunk.
        /// Uses GetChunksOfSize method.
        /// Useful in Entity Framework to allow doing updates within a foreach loop.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="chunkSize"></param>
        /// <returns></returns>
        public static IEnumerable<T> EnumerateInChunksOf<T>(this IEnumerable<T> enumerable, int chunkSize) {
            foreach (var chunk in enumerable.GetChunksOfSize(chunkSize)) {
                foreach (T item in chunk)
                    yield return item;
            }
        }

        /// <summary>
        /// Grabs items from enumerable in chunks of chunkSize.
        /// Useful in Entity Framework to allow doing updates within a foreach loop.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumerable"></param>
        /// <param name="chunkSize"></param>
        /// <returns></returns>
        public static IEnumerable<T[]> GetChunksOfSize<T>(this IEnumerable<T> enumerable, int chunkSize) {
            int count = enumerable.Count();
            for (int chunkIndex = 0; chunkIndex * chunkSize < count; chunkIndex++)
                yield return enumerable.Skip(chunkIndex * chunkSize).Take(chunkSize).ToArray();
        }
#if false
        /// <summary>
        /// Combine two sequences by applying func to pairs of elements from each.
        /// Result is as long as the shorter of a and b.
        /// </summary>
        /// <typeparam name="TA"></typeparam>
        /// <typeparam name="TB"></typeparam>
        /// <typeparam name="TR"></typeparam>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="func"></param>
        /// <returns></returns>
        public static IEnumerable<TR> Zip<TA, TB, TR>(this IEnumerable<TA> a, IEnumerable<TB> b, Func<TA, TB, TR> func) {
            var ie1 = a.GetEnumerator();
            var ie2 = b.GetEnumerator();

            while (ie1.MoveNext() && ie2.MoveNext())
                yield return func(ie1.Current, ie2.Current);
        }

        /// <summary>
        /// Combine two sequences by applying action to pairs of elements from each.
        /// </summary>
        /// <typeparam name="TA"></typeparam>
        /// <typeparam name="TB"></typeparam>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static void ZipA<TA, TB>(this IEnumerable<TA> a, IEnumerable<TB> b, Action<TA, TB> action) {
            var ie1 = a.GetEnumerator();
            var ie2 = b.GetEnumerator();

            while (ie1.MoveNext() && ie2.MoveNext())
                action(ie1.Current, ie2.Current);
        }
#endif

        /// <summary>
        /// Return sequence of Next linked nodes starting with node.Next.
        /// Last node will have Next == null or be equal to node if circularly linked.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static IEnumerable<T> Nexts<T>(this T node) where T : class, IEnumerableCircular<T> {
            var e = node.Next;
            while (e != null && e != node) {
                yield return e;
                e = e.Next;
            }
            if (e != null) yield return e;
        }

        /// <summary>
        /// Return sequence of Prev linked nodes starting with node.Prev.
        /// Last node will have Prev == null or be equal to node if circularly linked.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static IEnumerable<T> Prevs<T>(this IEnumerableCircular<T> node) where T : class, IEnumerableCircular<T> {
            var e = node.Prev;
            while (e != null && e != node) {
                yield return e;
                e = e.Prev;
            }
            if (e != null) yield return e;
        }

        /// <summary>
        /// Returns a new sequence where each value is an array of two items:
        /// Index 0 of each array is the corresponding value in the original sequence.
        /// Index 1 of each array is the following value in the original sequence
        /// except for the last tuple where it is the first value in the original sequence.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="seq"></param>
        /// <returns></returns>
        public static IEnumerable<T[]> ByPairsCircular<T>(this IEnumerable<T> seq) {
            T i0 = seq.First(), i1 = i0;
            foreach (T i2 in seq.Skip(1)) {
                yield return new[] { i1, i2 };
                i1 = i2;
            }
            yield return new[] { i1, i0 };
        }

        /// <summary>
        /// Returns a new sequence where each value is an array of two items:
        /// Index 0 of each array is the corresponding value in the original sequence.
        /// Index 1 of each array is the following value in the original sequence
        /// except for the last tuple where it is default(T).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="seq"></param>
        /// <returns></returns>
        public static IEnumerable<T[]> ByPairs<T>(this IEnumerable<T> seq) {
            T i1 = seq.First();
            foreach (T i2 in seq.Skip(1)) {
                yield return new[] { i1, i2 };
                i1 = i2;
            }
            yield return new[] { i1, default(T) };
        }

        /// <summary>
        /// Wraps index to range 0..Count-1 and returns the value at that location.
        /// index can be positive or negative.
        /// Not optimized for large negative index values (larger than -Count).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="vs"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static T IndexMod<T>(this List<T> vs, int index)
        {
            while (index < 0) index += vs.Count;
            return vs[index % vs.Count];
        }

        /// <summary>
        /// Returns a new sequence where each value is an array of three items:
        /// Index 0 of each array is the preceding value to the corresponding value in the original sequence.
        /// Index 1 of each array is the corresponding value in the original sequence.
        /// Index 2 of each array is the following value to the corresponding value in the original sequence.
        /// The first array's index 0 will be the last value in the original sequence.
        /// The last array's index 2 will be the first value in the original sequence.
        /// NOTE: The original sequence will be completely traveresed before the first tuple is returned.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="seq"></param>
        /// <returns></returns>
        public static IEnumerable<T[]> ByTriplesCircular<T>(this IEnumerable<T> seq) {
            var list = new List<T>(seq);
            for (int i = 0; i < list.Count; i++)
                yield return new[] { list.IndexMod(i - 1), list[i], list.IndexMod(i + 1) };
        }

        /// <summary>
        /// Returns a new sequence where each value is an array of three items:
        /// Index 0 of each array is the preceding value to the corresponding value in the original sequence.
        /// Index 1 of each array is the corresponding value in the original sequence.
        /// Index 2 of each array is the following value to the corresponding value in the original sequence.
        /// The first array's index 0 will be default(T).
        /// The last array's index 2 will be default(T).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="seq"></param>
        /// <returns></returns>
        public static IEnumerable<T[]> ByTriples<T>(this IEnumerable<T> seq) {
            T i0 = default(T), i1 = seq.First();
            foreach (T i2 in seq.Skip(1)) {
                yield return new[] { i0, i1, i2 };
                i0 = i1;
                i1 = i2;
            }
            yield return new[] { i0, i1, default(T) };
        }

        /// <summary>
        /// From a list of rows set of 2D data, return sequence of pairs of row neighbors
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static IEnumerable<T[]> GridPairsI<T>(this List<List<T>> data) {
            for (int j = 0; j < data.Count; j++) {
                for (int i = 0; i < data[0].Count - 1; i++) {
                    yield return new[] { data[j][i], data[j][i + 1] };
                }
            }
        }

        /// <summary>
        /// From a list of rows set of 2D data, return sequence of pairs of column neighbors.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        public static IEnumerable<T[]> GridPairsJ<T>(this List<List<T>> data) {
            for (int i = 0; i < data[0].Count; i++) {
                for (int j = 0; j < data.Count - 1; j++) {
                    yield return new[] { data[j][i], data[j + 1][i] };
                }
            }
        }

        /// <summary>
        /// Perform action on each value in sequence.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="seq"></param>
        /// <param name="action"></param>
        public static void Action<T>(this IEnumerable<T> seq, Action<T> action) {
            foreach (T s in seq) action(s);
        }

        /// <summary>
        /// Perform action on each value in sequence.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="seq"></param>
        /// <param name="action"></param>
        public static void Action<T>(this IEnumerable<T> seq, Action<T, int> action) {
            int i = 0;
            foreach (T s in seq) action(s, i++);
        }

        /// <summary>
        /// Return the index of the first element in the sequence to satisfy pred.
        /// Returns -1 if none is found.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="seq"></param>
        /// <param name="pred"></param>
        /// <returns></returns>
        public static int Findi<T>(this IEnumerable<T> seq, Func<T, bool> pred) {
            int i = 0;
            foreach (T s in seq) {
                if (pred(s)) return i;
                i++;
            }
            return -1;
        }

        /// <summary>
        /// Return the index of the sequence member that generates the minimum value for minMap
        /// Returns -1 if sequence is empty.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="seq"></param>
        /// <param name="minMap"></param>
        /// <returns></returns>
        public static int Mini<T>(this IEnumerable<T> seq, Func<T, double> minMap) {
            int i = 0;
            double minBest = double.MaxValue;
            int iBest = -1;
            foreach (T s in seq) {
                double min = minMap(s);
                if (min < minBest) {
                    minBest = min;
                    iBest = i;
                }
                i++;
            }
            return iBest;
        }

        /// <summary>
        /// Return the index of the sequence member that generates the maximum value for maxMap
        /// Returns -1 if sequence is empty.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="seq"></param>
        /// <param name="maxMap"></param>
        /// <returns></returns>
        public static int Maxi<T>(this IEnumerable<T> seq, Func<T, double> maxMap) {
            int i = 0;
            double maxBest = double.MinValue;
            int iBest = -1;
            foreach (T s in seq) {
                double max = maxMap(s);
                if (max > maxBest) {
                    maxBest = max;
                    iBest = i;
                }
                i++;
            }
            return iBest;
        }

        /// <summary>
        /// Return the value computed by selectMap from the sequence member
        /// with the maximum value generated by comparisonMap.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="seq"></param>
        /// <param name="comparisonMap"></param>
        /// <param name="selectMap"></param>
        /// <returns></returns>
        public static V Max<T, V>(this IEnumerable<T> seq,
                                 Func<T, int> comparisonMap,
                                 Func<T, V> selectMap) {
            int maxValue = 0;
            V maxElement = default(V);
            bool gotAny = false;
            foreach (T sourceValue in seq) {
                int value = comparisonMap(sourceValue);
                if (!gotAny || value > maxValue) {
                    maxValue = value;
                    maxElement = selectMap(sourceValue);
                    gotAny = true;
                }
            }
            return maxElement;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="source"></param>
        /// <param name="comparisonMap"></param>
        /// <param name="selectMap"></param>
        /// <returns></returns>
        public static V Max<T, V>(this IEnumerable<T> source,
                                 Func<T, double> comparisonMap,
                                 Func<T, V> selectMap) {
            double maxValue = 0;
            V maxElement = default(V);
            bool gotAny = false;
            foreach (T sourceValue in source) {
                double value = comparisonMap(sourceValue);
                if (!gotAny || value > maxValue) {
                    maxValue = value;
                    maxElement = selectMap(sourceValue);
                    gotAny = true;
                }
            }
            return maxElement;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="source"></param>
        /// <param name="comparisonMap"></param>
        /// <param name="selectMap"></param>
        /// <returns></returns>
        public static V Min<T, V>(this IEnumerable<T> source,
                                 Func<T, double> comparisonMap,
                                 Func<T, V> selectMap) {
            double minValue = double.MaxValue;
            V minElement = default(V);
            bool gotAny = false;
            foreach (T sourceValue in source) {
                double value = comparisonMap(sourceValue);
                if (!gotAny || value < minValue) {
                    minValue = value;
                    minElement = selectMap(sourceValue);
                    gotAny = true;
                }
            }
            return minElement;
        }

        /// <summary>
        /// Returns a list of items in sequence for which
        /// comparisonMap generates the same minimal value over the sequence.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="source"></param>
        /// <param name="comparisonMap"></param>
        /// <param name="selectMap"></param>
        /// <returns></returns>
        public static List<V> Mins<T, V>(this IEnumerable<T> source,
                                 Func<T, double> comparisonMap,
                                 Func<T, V> selectMap) {
            var minValue = double.MaxValue;
            var minElements = new List<V>();
            foreach (T sourceValue in source) {
                double value = comparisonMap(sourceValue);
                if (minElements.Count == 0 || value <= minValue) {
                    if (value < minValue)
                        minElements.Clear();
                    minValue = value;
                    minElements.Add(selectMap(sourceValue));
                }
            }
            return minElements;
        }

        /// <summary>
        /// AddRange ignores the return value of Add.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="set"></param>
        /// <param name="seq"></param>
        public static void AddRange<T>(this HashSet<T> set, IEnumerable<T> seq) {
            foreach (var v in seq) set.Add(v);
        }

        /// <summary>
        /// Skip the last value in a sequence.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="seq"></param>
        /// <returns></returns>
        public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> seq) {
            return seq.SkipLast(1);
        }

        /// <summary>
        /// Skip the last count values in a sequence.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="seq"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> seq, int count) {
            var vs = seq.GetEnumerator();
            Queue<T> q = new Queue<T>();
            while (count-- > 0 && vs.MoveNext()) q.Enqueue(vs.Current);
            while (vs.MoveNext()) {
                q.Enqueue(vs.Current);
                yield return q.Dequeue();
            }
        }

        /// <summary>
        /// For n input lists, returns all the permutations such that
        /// the first value in each returned list is drawn from the first input list,
        /// the second value is drawn from the second list,
        /// etc.
        /// AND each value in each returned list is unique.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sequences"></param>
        /// <returns></returns>
        public static IEnumerable<List<T>> Permutations<T>(this IEnumerable<List<T>> sequences) {
            var seqs = new List<List<T>>(sequences);
            if (seqs.Count > 0) {
                if (seqs.Count == 1)
                    foreach (var v in seqs.First()) yield return new List<T>(new[] { v });
                else {
                    foreach (var v in seqs.First()) {
                        foreach (var r in seqs.Skip(1).Permutations()) {
                            if (!r.Contains(v)) {
                                var p = new List<T>(new[] { v });
                                p.AddRange(r);
                                yield return p;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns all the combinations of the input sequence taking elements k at a time.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sequence"></param>
        /// <param name="k"></param>
        /// <returns></returns>
        public static IEnumerable<List<T>> Combinations<T>(this IEnumerable<T> sequence, int k) {
            if (k == 1) {
                foreach (var v in sequence) yield return new List<T>(new[] { v });
            } else {
                foreach (var v in sequence.SkipLast(k - 1)) {
                    foreach (var r in sequence.Skip(1).Combinations(k - 1)) {
                        var c = new List<T>(new[] { v });
                        c.AddRange(r);
                        yield return c;
                    }
                }
            }
        }
    }
}
