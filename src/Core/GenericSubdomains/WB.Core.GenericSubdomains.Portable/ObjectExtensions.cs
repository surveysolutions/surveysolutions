namespace WB.Core.GenericSubdomains.Portable
{
    public static class ObjectExtensions
    {
        public static string AsCompositeKey(object key, object value)
        {
            return string.Format("{0}${1}", key, value);
        }
    }
}
