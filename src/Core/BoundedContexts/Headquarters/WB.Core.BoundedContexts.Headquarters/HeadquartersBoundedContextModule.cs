using Ninject.Modules;
using WB.Core.BoundedContexts.Headquarters.Interviews.Denormalizers;
using WB.Core.BoundedContexts.Headquarters.Questionnaires;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Implementation;
using WB.Core.BoundedContexts.Headquarters.Synchronization;
using WB.Core.BoundedContexts.Headquarters.Synchronization.Implementation;
using WB.Core.Infrastructure.FunctionalDenormalization;

namespace WB.Core.BoundedContexts.Headquarters
{
    public class HeadquartersBoundedContextModule : NinjectModule
    {
        private readonly InterviewDetailsDataLoaderSettings interviewDetailsDataLoaderSettings;
        public HeadquartersBoundedContextModule(InterviewDetailsDataLoaderSettings interviewDetailsDataLoaderSettings)
        {
            this.interviewDetailsDataLoaderSettings = interviewDetailsDataLoaderSettings;
        }

        public override void Load()
        {
            this.Bind<IVersionedQuestionnaireReader>().To<VersionedQustionnaireDocumentViewFactory>();
            this.Kernel.RegisterDenormalizer<InterviewsFeedDenormalizer>();

            this.Bind<IInterviewDetailsDataLoader>().To<InterviewDetailsDataLoader>();
            this.Bind<IInterviewDetailsDataProcessor>().To<InterviewDetailsDataProcessor>();
            this.Bind<InterviewDetailsDataProcessorContext>().ToSelf().InSingletonScope();
            this.Bind<InterviewDetailsDataLoaderSettings>().ToConstant(this.interviewDetailsDataLoaderSettings);
            this.Bind<InterviewDetailsBackgroundSchedulerTask>().ToSelf();
        }
    }
}
