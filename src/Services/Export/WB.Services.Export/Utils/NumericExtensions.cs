namespace WB.Services.Export
{
    public static class NumericExtensions
    {
        public static int PercentOf(this long source, long totalCount)
        {
            if (source > totalCount) return 100;
            return (int)((decimal)source / totalCount * 100);
        }

        public static double PercentDOf(this long source, long totalCount)
        {
            if (source > totalCount) return 100.0;
            return (source / (double) totalCount) * 100.0;
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
