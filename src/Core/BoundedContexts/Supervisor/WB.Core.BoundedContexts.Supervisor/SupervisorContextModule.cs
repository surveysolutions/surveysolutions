using System;
using System.Net;
using System.Net.Http;
using Ninject;
using Ninject.Modules;
using WB.Core.BoundedContexts.Supervisor.Interviews;
using WB.Core.BoundedContexts.Supervisor.Interviews.Implementation;
using WB.Core.BoundedContexts.Supervisor.Interviews.Implementation.EventHandlers;
using WB.Core.BoundedContexts.Supervisor.Questionnaires;
using WB.Core.BoundedContexts.Supervisor.Questionnaires.Implementation;
using WB.Core.BoundedContexts.Supervisor.Synchronization;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Atom;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Atom.Implementation;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using WB.Core.BoundedContexts.Supervisor.Users;
using WB.Core.BoundedContexts.Supervisor.Users.Implementation;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernels.DataCollection.Services;

namespace WB.Core.BoundedContexts.Supervisor
{
    public class SupervisorBoundedContextModule : NinjectModule
    {
        private readonly IHeadquartersSettings headquartersSettings;
        private readonly SchedulerSettings schedulerSettings;

        public SupervisorBoundedContextModule(IHeadquartersSettings headquartersSettings,
            SchedulerSettings schedulerSettings)
        {
            this.headquartersSettings = headquartersSettings;
            this.schedulerSettings = schedulerSettings;
        }

        public override void Load()
        {
            this.Bind<IHeadquartersSettings>().ToConstant(this.headquartersSettings);
            this.Bind<IHeadquartersLoginService>().To<HeadquartersLoginService>();
            this.Bind<ILocalFeedStorage>().To<LocalFeedStorage>();
            this.Bind<SynchronizationJob>().ToSelf();
            this.Bind<ISynchronizer>().To<Synchronizer>();
            this.Bind<Func<ISynchronizer>>().ToMethod(context => () => context.Kernel.Get<ISynchronizer>());
            this.Bind<IInterviewsSynchronizer>().To<InterviewsSynchronizer>();
            this.Bind<IUserChangedFeedReader>().To<UserChangedFeedReader>();
            this.Bind<IQuestionnaireSynchronizer>().To<QuestionnaireSynchronizer>();
            this.Bind<ILocalUserFeedProcessor>().To<LocalUserFeedProcessor>();
            this.Bind<IHeadquartersUserReader>().To<HeadquartersUserReader>();
            this.Bind<IHeadquartersQuestionnaireReader>().To<HeadquartersQuestionnaireReader>();
            this.Bind<IHeadquartersInterviewReader>().To<HeadquartersInterviewReader>();
            this.Bind<IAtomFeedReader>().To<AtomFeedReader>();
            this.Bind<HeadquartersPullContext>().ToSelf().InSingletonScope();
            this.Bind<HeadquartersPushContext>().ToSelf().InSingletonScope();
            this.Bind<SchedulerSettings>().ToConstant(this.schedulerSettings);
            this.Bind<BackgroundSyncronizationTasks>().ToSelf();

            this.Bind<IEventHandler>().To<ReadyToSendToHeadquartersInterviewDenormalizer>().InSingletonScope();

            this.Bind<Func<HttpMessageHandler>>().ToMethod(x => () => new HttpClientHandler(){ AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate});
            //this.Bind<HttpMessageHandler>().To<HttpClientHandler>();
        }
    }
}