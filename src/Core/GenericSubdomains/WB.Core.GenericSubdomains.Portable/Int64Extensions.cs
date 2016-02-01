namespace WB.Core.GenericSubdomains.Portable
{
    public static class Int64Extensions
    {
        public static int PercentOf(this long source, long totalCount)
        {
            if (source > totalCount) return 100;
            return (int) ((decimal) source/totalCount*100);
        }
    }
}