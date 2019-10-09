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
using WB.UI.Headquarters.API.WebInterview.Pipeline;
using WB.UI.Headquarters.Code;

namespace WB.UI.Headquarters.API.WebInterview
{
    [HubName(@"interview")]
    [WebInterviewAuthorize]
    public class WebInterviewHub : Enumerator.Native.WebInterview.WebInterview, ILifetimeHub
    {
        private IInterviewBrokenPackagesService interviewBrokenPackagesService => this.ServiceLocator.GetInstance<IInterviewBrokenPackagesService>();
        private IAuthorizedUser authorizedUser => this.ServiceLocator.GetInstance<IAuthorizedUser>();
        private IChangeStatusFactory changeStatusFactory => this.ServiceLocator.GetInstance<IChangeStatusFactory>();
        private IInterviewFactory interviewFactory => this.ServiceLocator.GetInstance<IInterviewFactory>();
        private IStatefullInterviewSearcher statefullInterviewSearcher => this.ServiceLocator.GetInstance<IStatefullInterviewSearcher>();
        private IInterviewOverviewService overviewService => this.ServiceLocator.GetInstance<IInterviewOverviewService>();
        
        
        
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
