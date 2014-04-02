using System;
using System.Web;
using Main.Core.View;
using Ninject.Modules;
using WB.Core.BoundedContexts.Headquarters.Interview.EventHandlers;
using WB.Core.BoundedContexts.Headquarters.Interview.ViewFactories;
using WB.Core.BoundedContexts.Headquarters.Interview.Views;
using WB.Core.BoundedContexts.Headquarters.Interview.Views.TakeNew;
using WB.Core.BoundedContexts.Headquarters.PasswordPolicy;
using WB.Core.BoundedContexts.Headquarters.Questionnaires;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Implementation;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Views;
using WB.Core.BoundedContexts.Headquarters.Reports.ViewFactories;
using WB.Core.BoundedContexts.Headquarters.Reports.ViewFactories.Implementation;
using WB.Core.BoundedContexts.Headquarters.Team.EventHandlers;
using WB.Core.BoundedContexts.Headquarters.Team.Models;
using WB.Core.BoundedContexts.Headquarters.Team.ViewFactories;
using WB.Core.BoundedContexts.Headquarters.Team.ViewFactories.Implementation;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.GenericSubdomains.Utils.Implementation;
using WB.Core.Infrastructure.EventBus;
using WB.Core.Infrastructure.FunctionalDenormalization;

namespace WB.Core.BoundedContexts.Headquarters
{
    public class HeadquartersBoundedContextModule : NinjectModule
    {
        private readonly int supportedQuestionnaireVersionMajor;
        private readonly int supportedQuestionnaireVersionMinor;
        private readonly int supportedQuestionnaireVersionPatch;

        public HeadquartersBoundedContextModule(int supportedQuestionnaireVersionMajor, int supportedQuestionnaireVersionMinor, int supportedQuestionnaireVersionPatch)
        {
            this.supportedQuestionnaireVersionMajor = supportedQuestionnaireVersionMajor;
            this.supportedQuestionnaireVersionMinor = supportedQuestionnaireVersionMinor;
            this.supportedQuestionnaireVersionPatch = supportedQuestionnaireVersionPatch;
        }

        public override void VerifyRequiredModulesAreLoaded()
        {
            if (!this.Kernel.HasModule(typeof(PasswordPolicyModule).FullName))
            {
                throw new InvalidOperationException("PasswordPolicyModule is required");
            }
        }

        public override void Load()
        {
            this.Bind<IViewFactory<UserListViewInputModel, UserListView>>().To<UserListViewFactory>();
            this.Bind<IViewFactory<InterviewersInputModel, InterviewersView>>().To<InterviewersViewFactory>();
            this.Bind<IViewFactory<UserViewInputModel, UserView>>().To<UserViewFactory>();
            this.Bind<IUserListViewFactory>().To<UserListViewFactory>();
            this.Bind<IAllUsersAndQuestionnairesFactory>().To<AllUsersAndQuestionnairesFactory>();
            this.Bind<IViewFactory<TakeNewInterviewInputModel, TakeNewInterviewView>>().To<TakeNewInterviewViewFactory>();
            this.Bind<IViewFactory<AllInterviewsInputModel, AllInterviewsView>>().To<AllInterviewsFactory>();

            this.Bind<IEventHandler>().To<QuestionnaireBrowseItemEventHandler>();

            DispatcherRegistryHelper.RegisterDenormalizer<UserDenormalizer>(this.Kernel);
            DispatcherRegistryHelper.RegisterDenormalizer<InterviewSummaryEventHandlerFunctional>(this.Kernel);

            this.Bind<IPasswordHasher>().To<PasswordHasher>().InSingletonScope(); // external class which cannot be put to self-describing module because ninject is not portable

            this.Bind<IDesignerService>().To<DesignerService>();

            this.Unbind(typeof(HttpContextBase));
            this.Bind<HttpContextBase>().ToMethod(ctx => new HttpContextWrapper(HttpContext.Current)).InTransientScope();

            this.Bind<ApplicationVersionSettings>().ToMethod(context => new ApplicationVersionSettings
            {
                SupportedQuestionnaireVersionMajor = this.supportedQuestionnaireVersionMajor,
                SupportedQuestionnaireVersionMinor = this.supportedQuestionnaireVersionMinor,
                SupportedQuestionnaireVersionPatch = this.supportedQuestionnaireVersionPatch
            });
        }
    }
}
