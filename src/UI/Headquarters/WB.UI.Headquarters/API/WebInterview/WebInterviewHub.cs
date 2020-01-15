using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Commands.Interview;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.DataCollection.Views.Interview.Overview;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Models;
using WB.Enumerator.Native.WebInterview.Pipeline;
using WB.UI.Headquarters.API.WebInterview.Pipeline;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.WebInterview
{
    [HubName(@"interview")]
    [WebInterviewAuthorize]
    public class WebInterviewHub : Enumerator.Native.WebInterview.WebInterview, ILifetimeHub
    {
        public WebInterviewHub(IPipelineModule[] hubPipelineModules) : base(hubPipelineModules)
        {
        }

        public event EventHandler OnDisposing;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                var handler = OnDisposing;
                handler?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
