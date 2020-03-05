using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WB.UI.Headquarters.Code
{
    public static class ActionExecutingContextExtensions
    {
        public static T GetActionArgumentOrDefault<T>(this ActionExecutingContext context, string argument, T defaultValue)
        {
            if (!context.ActionArguments.TryGetValue(argument, out var value))
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
    }
}
