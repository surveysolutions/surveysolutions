namespace WB.Services.Export.Utils
{
    public static class NumericExtensions
    {
        public static int PercentOf(this long source, long totalCount)
        {
            if (source > totalCount) return 100;
            return (int)((decimal)source / totalCount * 100);
        }

        public static int PercentOf(this long? source, long? totalCount)
        {
            if (source == null || totalCount == null) return 0;
            if (totalCount == 0) return 0;

            if (source > totalCount) return 100;
            return (int)((decimal)source / totalCount * 100);
        }
    }
}
