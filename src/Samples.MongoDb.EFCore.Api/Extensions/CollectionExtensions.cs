namespace Samples.MongoDb.EFCore.Api.Extensions
{
    public static class CollectionExtensions
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static bool ContainedIn<T>(this IEnumerable<T> subset, IEnumerable<T> superset)
        {
            return new HashSet<T>(subset ?? new List<T>()).IsSubsetOf(superset ?? new List<T>());
        }
        public static bool Contains<T>(this IEnumerable<T> superset, IEnumerable<T> subset)
        {
            return new HashSet<T>(superset ?? new List<T>()).IsSupersetOf(subset ?? new List<T>());
        }
        public static bool IsEqual<T>(this IEnumerable<T> compare, IEnumerable<T> compareTo)
        {
            return new HashSet<T>(compare ?? new List<T>()).SetEquals(compareTo ?? new List<T>());
        }
    }
}
