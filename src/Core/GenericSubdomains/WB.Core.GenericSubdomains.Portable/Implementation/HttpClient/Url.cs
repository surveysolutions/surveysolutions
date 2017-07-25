using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace WB.Core.GenericSubdomains.Portable.Implementation
{
    public class Url
    {
        public Url(string host, string path, object queryParams)
        {
            Host = host;
            Path = path;
            QueryParams = ParseQueryParams(queryParams);
        }

        public string Host { get; }
        public string Path { get; }
        public IEnumerable<KeyValuePair<string, object>> QueryParams { get; }

        private List<KeyValuePair<string, object>> ParseQueryParams(object values)
        {
            if (values == null)
                return null;

            var keyValuePairs = from prop in values.GetType().GetRuntimeProperties()
                let val = prop.GetValue(values, null)
                select new KeyValuePair<string, object>(prop.Name, val);

            return keyValuePairs.ToList();
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder(Host.TrimEnd('/'));
            if (!Path.IsNullOrEmpty())
                stringBuilder.Append('/').Append(Path.Trim('/'));

            if (QueryParams == null || !QueryParams.Any())
                return stringBuilder.ToString();

            stringBuilder.Append("?");

            foreach (var keyValuePair in QueryParams)
            {
                stringBuilder.Append(keyValuePair.Key).Append('=').Append(keyValuePair.Value).Append('&');
            }
            stringBuilder.Remove(stringBuilder.Length - 1, 1);

            return stringBuilder.ToString();
        }
    }
}