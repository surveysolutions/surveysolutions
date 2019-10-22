using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WB.Core.BoundedContexts.Designer.Classifications;

namespace WB.UI.Designer.Controllers.Api.Designer
{
    public class ClassificationsExceptionFilterAttribute : ExceptionFilterAttribute 
    {
        public override void OnException(ExceptionContext context)
        {
            if (!(context.Exception is ClassificationException exception)) return;
            switch (exception.ErrorType)
            {
                case ClassificationExceptionType.Undefined:
                    context.Result = new BadRequestResult();;
                    break;
                case ClassificationExceptionType.NoAccess:
                    context.Result = new ForbidResult();
                    break;
            }
        }
    }
}
