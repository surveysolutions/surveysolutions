using System;
using System.ComponentModel;
using System.Resources;
using System.Web.Http.Filters;
using WB.UI.Headquarters.Models;

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
        public static TranslationModel Translations(this ResourceManager[] resources)
        {
            return new TranslationModel(resources);
        }
    }
}