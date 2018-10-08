namespace WB.Services.Export.Utils
{
    public static class NumericExtensions
    {
        public static int PercentOf(this long source, long totalCount)
        {
            if (source > totalCount) return 100;
            return (int)((decimal)source / totalCount * 100);
        }
    }
}
