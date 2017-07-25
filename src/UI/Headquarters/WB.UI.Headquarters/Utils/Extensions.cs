using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Resources;
using System.Web.Http.Filters;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace WB.UI.Headquarters.Utils
{
    public static class Extensions
    {
        public static T GetActionArgumentOrDefault<T>(this HttpActionExecutedContext context, string argument, T defaultValue)
        {
            object value;
            if (!context.ActionContext.ActionArguments.TryGetValue(argument, out value))
                return defaultValue;

            if (value is T)
            {
                return (T)value;
            }

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (InvalidCastException)
            {
                return defaultValue;
            }
        }

        [Localizable(false)]
        public static Dictionary<string, string> Translations(this ResourceManager[] resources)
        {
            var result = new Dictionary<string, string>();

            foreach (var resource in resources)
            {
                IEnumerable<string> keys = resource
                    .GetResourceSet(CultureInfo.InvariantCulture, true, true)
                    .Cast<DictionaryEntry>()
                    .Select(entry => entry.Key)
                    .Cast<string>();

                foreach (var key in keys)
                {
                    var lastDot = resource.BaseName.LastIndexOf(".", StringComparison.Ordinal);
                    result.Add(resource.BaseName.Substring(lastDot > 0 ? lastDot + 1 : 0) + "." + key, resource.GetString(key, CultureInfo.CurrentUICulture));
                }
            }

            return result;
        }
    }
}