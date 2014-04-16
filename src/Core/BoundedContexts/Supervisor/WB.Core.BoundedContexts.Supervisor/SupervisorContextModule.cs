using System.Net.Http;
using Ninject.Modules;
using WB.Core.BoundedContexts.Supervisor.Interviews;
using WB.Core.BoundedContexts.Supervisor.Interviews.Implementation;
using WB.Core.BoundedContexts.Supervisor.Questionnaires;
using WB.Core.BoundedContexts.Supervisor.Questionnaires.Implementation;
using WB.Core.BoundedContexts.Supervisor.Synchronization;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Atom;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Atom.Implementation;
using WB.Core.BoundedContexts.Supervisor.Synchronization.Implementation;
using WB.Core.BoundedContexts.Supervisor.Users;
using WB.Core.BoundedContexts.Supervisor.Users.Implementation;

namespace WB.Core.BoundedContexts.Supervisor
{
    public class SupervisorBoundedContextModule : NinjectModule
    {
        private readonly HeadquartersSettings headquartersSettings;

        public SupervisorBoundedContextModule(HeadquartersSettings headquartersSettings)
        {
            this.headquartersSettings = headquartersSettings;
        }

        public override void Load()
        {
            this.Bind<HeadquartersSettings>().ToConstant(this.headquartersSettings);
            this.Bind<IHeadquartersLoginService>().To<HeadquartersLoginService>();
            this.Bind<ILocalFeedStorage>().To<LocalFeedStorage>();
            this.Bind<ISynchronizer>().To<Synchronizer>();
            this.Bind<IInterviewsSynchronizer>().To<InterviewsSynchronizer>();
            this.Bind<IUserChangedFeedReader>().To<UserChangedFeedReader>();
            this.Bind<ILocalUserFeedProcessor>().To<LocalUserFeedProcessor>();
            this.Bind<IHeadquartersUserReader>().To<HeadquartersUserReader>();
            this.Bind<IHeadquartersQuestionnaireReader>().To<HeadquartersQuestionnaireReader>();
            this.Bind<IHeadquartersInterviewReader>().To<HeadquartersInterviewReader>();
            this.Bind<IAtomFeedReader>().To<AtomFeedReader>();

            this.Bind<HttpMessageHandler>().To<HttpClientHandler>();
        }
    }
}