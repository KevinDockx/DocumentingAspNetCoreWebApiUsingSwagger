namespace Library.API;

public static class IListExtensions
{
    public static void AddRange<T>(this IList<T> list, IEnumerable<T> items)
    {
        ArgumentNullException.ThrowIfNull(list);
        ArgumentNullException.ThrowIfNull(items);

        if (list is List<T>)
        {
            ((List<T>)list).AddRange(items);
        }
        else
        {
            foreach (var item in items)
            {
                list.Add(item);
            }
        }
    }
}
