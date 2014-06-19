using System;
using Quartz;
using WB.Core.SharedKernels.SurveyManagement.Synchronization.Schedulers.InterviewDetailsDataScheduler;

namespace WB.Core.SharedKernels.SurveyManagement.Implementation.Synchronization.Schedulers.InterviewDetailsDataScheduler
{
    internal class InterviewDetailsDataLoader : IInterviewDetailsDataLoader, IJob
    {
        private readonly IInterviewDetailsDataProcessor interviewDetailsDataProcessor;
        private readonly InterviewDetailsDataProcessorContext interviewDetailsDataProcessorContext;

        private bool isSynchronizationRunning;
        private static readonly object LockObject = new object();

        public InterviewDetailsDataLoader(
            IInterviewDetailsDataProcessor interviewDetailsDataProcessor,
            InterviewDetailsDataProcessorContext interviewDetailsDataProcessorContext)
        {

            if (interviewDetailsDataProcessor == null) throw new ArgumentNullException("interviewDetailsDataProcessor");
            if (interviewDetailsDataProcessorContext == null)
                throw new ArgumentNullException("interviewDetailsDataProcessorContext");

            this.interviewDetailsDataProcessor = interviewDetailsDataProcessor;
            this.interviewDetailsDataProcessorContext = interviewDetailsDataProcessorContext;
        }

        public void Load()
        {
            if (!this.isSynchronizationRunning)
            {
                lock (LockObject)
                {
                    if (!this.isSynchronizationRunning)
                    {
                        try
                        {
                            this.isSynchronizationRunning = true;
                            this.interviewDetailsDataProcessorContext.Start();

                            this.interviewDetailsDataProcessor.Process();
                        }
                        finally
                        {
                            this.isSynchronizationRunning = false;
                            this.interviewDetailsDataProcessorContext.Stop();
                        }
                    }
                }
            }
        }

        public void Execute(IJobExecutionContext context)
        {
            this.Load();
        }
    }
}