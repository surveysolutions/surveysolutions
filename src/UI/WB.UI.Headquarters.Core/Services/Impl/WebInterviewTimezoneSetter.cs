using System;
using Microsoft.AspNetCore.Http;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.SharedKernels.DataCollection.Commands.Interview.Base;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;

namespace WB.UI.Headquarters.Services.Impl
{
    public class WebInterviewTimezoneSetter : IWebInterviewTimezoneSetter
    {
        private readonly IHttpContextAccessor contextAccessor;

        public WebInterviewTimezoneSetter(IHttpContextAccessor contextAccessor)
        {
            this.contextAccessor = contextAccessor;
        }

        public void Process(StatefulInterview aggregate, InterviewCommand command)
        {
            if (TryGetTimezoneOffset(out var offset))
            {
                command.OriginDate = DateTimeOffset.UtcNow.ToOffset(-TimeSpan.FromMinutes(offset));
            }
        }
        
        private bool TryGetTimezoneOffset(out int offset)
        {
            var httpContext = contextAccessor.HttpContext;

            if (httpContext?.Request?.Cookies != null)
            {
                if (httpContext.Request.Cookies.TryGetValue("_tz", out var tz))
                {
                    if (int.TryParse(tz, out var val))
                    {
                        offset = val;
                        return true;
                    }
                }
            }

            offset = 0;
            return false;
        }
    }
}
