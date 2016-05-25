using System.Web.Http.Filters;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Utils
{
    public static class Extensions
    {
        public static T GetActionArgument<T>(this HttpActionExecutedContext context, string argument)
        {
            return (T) context.ActionContext.ActionArguments[argument];
        }
    }
}