using System;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Infrastructure
{
    public static class Extensions
    {
        public static string GetDomainName(this string url)
        {
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException("url");
            Uri uri;

            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
                throw new ArgumentException("invalid url string");

            return uri.ToString().Replace(uri.PathAndQuery, "");
        }

        public static string FormatGuid(this Guid guid)
        {
            return guid.ToString("N");
        }
    }

    public static class TypeSwitch
    {
        public class CaseInfo
        {
            public bool IsDefault { get; set; }
            public Type Target { get; set; }
            public Action<object> Action { get; set; }
        }

        public static void Do(object source, params CaseInfo[] cases)
        {
            var type = source.GetType();
            foreach (var entry in cases)
            {
                if (entry.IsDefault || type == entry.Target)
                {
                    entry.Action(source);
                    break;
                }
            }
        }

        public static CaseInfo Case<T>(Action action)
        {
            return new CaseInfo()
            {
                Action = x => action(),
                Target = typeof(T)
            };
        }

        public static CaseInfo Case<T>(Action<T> action)
        {
            return new CaseInfo()
            {
                Action = (x) => action((T)x),
                Target = typeof(T)
            };
        }

        public static CaseInfo Default(Action action)
        {
            return new CaseInfo()
            {
                Action = x => action(),
                IsDefault = true
            };
        }
    }
}
