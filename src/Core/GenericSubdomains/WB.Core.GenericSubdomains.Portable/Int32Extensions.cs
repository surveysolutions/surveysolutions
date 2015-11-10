namespace WB.Core.GenericSubdomains.Portable
{
    public static class Int32Extensions
    {
        public static int PercentOf(this int source, int totalCount)
        {
            return (int) ((double) source/totalCount*100);
        }
    }
}