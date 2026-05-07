namespace Social.Common.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool IsNullIsEmptyOrFirstIsNull<TSource>(
            this IEnumerable<TSource> source)
        {
            if (source == null) return true;
            var enumerable = source as TSource[] ?? source.ToArray();
            if (!enumerable.Any()) return true;
            return enumerable[0] == null;
        }
    }
}