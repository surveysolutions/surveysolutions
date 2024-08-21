using System.Collections.Specialized;
using WB.Core.GenericSubdomains.Portable;

namespace WB.UI.Shared.Web.Extensions
{
    public static class NameValueCollectionExtensions
    {
        public static string? GetString(this NameValueCollection collection, string key, string? @default = null)
        {
            return collection[key] ?? @default;
        }

        public static bool GetBool(this NameValueCollection collection, string key, bool @default)
        {
            return collection.GetString(key).ToBool(@default);
        }

        public static int GetInt(this NameValueCollection collection, string key, int @default)
        {
            return collection.GetString(key)?.ToIntOrDefault(@default) ?? @default;
        }
    }

}
