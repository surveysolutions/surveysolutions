using System;
using Autofac;
using Quartz;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Infrastructure.Native.Storage.Postgre;
using WB.UI.Shared.Enumerator.Services.Internals;

namespace WB.Core.BoundedContexts.Headquarters.QuartzIntegration
{
    public class AsyncScopedJobDecorator : IJob
    {
        private readonly Type jobType;
        private IContainer container;

        public AsyncScopedJobDecorator(Type jobType)
        {
            this.jobType = jobType;
        }

        public void Execute(IJobExecutionContext context)
        {
            using (var scope = ServiceLocator.Current.CreateChildContainer())
            {
                //preserve scope
                var serviceLocator = scope.Resolve<IServiceLocator>(new NamedParameter("kernel", scope));
                var unitOfWork = scope.Resolve<IUnitOfWork>();
                try
                {
                    var job = scope.Resolve(jobType) as IJob;
                    if (job == null)
                        throw new ArgumentNullException(nameof(job));
                    job.Execute(context);

                    unitOfWork.AcceptChanges();
                }
                catch (Exception)
                {
                    unitOfWork.Dispose();
                    throw;
                }
            }
        }
    }
}
