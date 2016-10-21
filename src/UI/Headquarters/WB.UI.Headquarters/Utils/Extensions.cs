using System.Web.Http.Filters;

namespace WB.UI.Headquarters.Utils
{
    public static class Extensions
    {
        public static T GetActionArgument<T>(this HttpActionExecutedContext context, string argument)
        {
            object value;
            context.ActionContext.ActionArguments.TryGetValue(argument, out value);
            return (T)value;
        }
    }
}