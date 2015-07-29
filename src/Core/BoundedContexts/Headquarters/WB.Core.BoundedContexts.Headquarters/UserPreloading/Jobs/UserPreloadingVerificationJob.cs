using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using NHibernate;
using Ninject;
using Quartz;
using WB.Core.BoundedContexts.Headquarters.Resources;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Dto;
using WB.Core.BoundedContexts.Headquarters.UserPreloading.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.Infrastructure.Storage;
using WB.Core.Infrastructure.Storage.Postgre;
using WB.Core.Infrastructure.Storage.Postgre.Implementation;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection.Views;

namespace WB.Core.BoundedContexts.Headquarters.UserPreloading.Jobs
{
    [DisallowConcurrentExecution]
    internal class UserPreloadingVerificationJob : IJob
    {
        IUserPreloadingVerifier UserPreloadingVerifier
        {
            get { return ServiceLocator.Current.GetInstance<IUserPreloadingVerifier>(); }
        }

        public void Execute(IJobExecutionContext context)
        {
            IsolatedThreadManager.MarkCurrentThreadAsIsolated();
            try
            {
                UserPreloadingVerifier.VerifyProcessFromReadyToBeVerifiedQueue();
            }
            finally
            {
                IsolatedThreadManager.ReleaseCurrentThreadFromIsolation();
            }
        }
    }
}