namespace CPUSimulator.Core.Assembly
{
    public static class Extensions
    {
        public static bool All<T>(this ReadOnlySpan<T> span, Func<T, bool> cond)
        {
            foreach (T t in span)
            {
                if (!cond(t)) return false;
            }
            return true;
        }

        public static ulong Sum<T>(this IEnumerable<T> list, Func<T, ulong> func)
        {
            ulong sum = 0;
            foreach (T t in list)
            {
                sum += func(t);
            }
            return sum;
        }
    }
}