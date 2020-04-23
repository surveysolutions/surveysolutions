using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Exceptions;

namespace WB.Enumerator.Native.WebInterview
{
    public class HandleCommandErrorAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            var interviewException = context.Exception.GetSelfOrInnerAs<InterviewException>();
            if (interviewException != null)
            {
                string errorMessage = WB.Enumerator.Native.WebInterview.WebInterview.GetUiMessageFromException(interviewException);

                context.Result = new JsonResult(new {ErrorMessage = errorMessage})
                {
                    StatusCode = StatusCodes.Status400BadRequest
                };
                context.ExceptionHandled = true;
            }
        }
    }
}
