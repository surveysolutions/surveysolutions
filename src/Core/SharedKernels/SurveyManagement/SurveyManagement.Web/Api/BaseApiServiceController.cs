using System;
using System.Web.Http;
using WB.Core.GenericSubdomains.Portable.Services;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Api
{
    public abstract class BaseApiServiceController : ApiController
    {
        protected readonly ILogger Logger;
        
        protected int MaxPageSize = 40;

        protected BaseApiServiceController(ILogger logger)
        {
            this.Logger = logger;
        }

        protected int CheckAndRestrictLimit(int limit)
        {
            return limit < 0 ? 1 : Math.Min(limit, this.MaxPageSize);
        }

        protected int CheckAndRestrictOffset(int offset)
        {
            return Math.Max(offset, 1);
        }
    }
}




