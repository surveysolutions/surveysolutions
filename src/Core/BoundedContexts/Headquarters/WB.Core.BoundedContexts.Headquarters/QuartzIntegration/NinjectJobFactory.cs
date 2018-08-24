using System;
using System.Globalization;
using Ninject;
using Quartz;
using Quartz.Spi;
using WB.Infrastructure.Native.Ioc;
using WB.Infrastructure.Native.Storage.Postgre;

namespace WB.Core.BoundedContexts.Headquarters.QuartzIntegration
{
    public class NinjectJobFactory : IJobFactory
    {
        private readonly IKernel kernel;

        public NinjectJobFactory(IKernel kernel)
        {
            this.kernel = kernel;
        }

        /// <summary>
        /// Called by the scheduler at the time of the trigger firing, in order to
        /// produce a <see cref="IJob" /> instance on which to call Execute.
        /// Instance creation is delegated to the Ninject Kernel.
        /// </summary>
        /// <remarks>
        /// It should be extremely rare for this method to throw an exception -
        /// basically only the the case where there is no way at all to instantiate
        /// and prepare the Job for execution.  When the exception is thrown, the
        /// Scheduler will move all triggers associated with the Job into the
        /// <see cref="TriggerState.Error" /> state, which will require human
        /// intervention (e.g. an application restart after fixing whatever
        /// configuration problem led to the issue wih instantiating the Job.
        /// </remarks>
        /// <param name="bundle">The TriggerFiredBundle from which the <see cref="IJobDetail" />
        ///   and other info relating to the trigger firing can be obtained.</param>
        /// <param name="scheduler"></param>
        /// <returns>the newly instantiated Job</returns>
        /// <throws>  SchedulerException if there is a problem instantiating the Job. </throws>
        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            IJobDetail jobDetail = bundle.JobDetail;
            Type jobType = jobDetail.JobType;
            try
            {
                return new LazyNinjectJobWripper(jobType, kernel);
            }
            catch (Exception e)
            {
                var se = new SchedulerException(string.Format(CultureInfo.InvariantCulture, "Problem instantiating class '{0}'", jobDetail.JobType.FullName), e);
                throw se;
            }
        }

        /// <summary>
        /// Allows the the job factory to destroy/cleanup the job if needed. 
        /// No-op when using SimpleJobFactory.
        /// </summary>
        public void ReturnJob(IJob job)
        {
        }
    }

    public class LazyNinjectJobWripper : IJob
    {
        private IKernel kernel;
        private Type jobType;

        public LazyNinjectJobWripper(Type jobType,  IKernel kernel)
        {
            this.kernel = kernel;
            this.jobType = jobType;
        }

        public void Execute(IJobExecutionContext context)
        {
            using (var scope = new NinjectAmbientScope())
            {
                var unitOfWork = kernel.Get<IUnitOfWork>();
                try
                {
                    var job = kernel.Get(jobType) as IJob;
                    if (job == null) throw new ArgumentNullException(nameof(job));
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
