using System;
using System.Web;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Utils
{
    public static class ExceptionExtensions
    {
        public static bool IsHttpNotFound(this Exception e)
        {
            var httpException = e as HttpException;
            return httpException != null && httpException.GetHttpCode() == 404;
        }
    }
}