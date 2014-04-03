using Main.Core;
using Questionnaire.Core.Web.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernel.Utils.Compression;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.UI.Shared.Web.Filters;
using Web.Supervisor.Code;

namespace Web.Supervisor.Injections
{
    public class SupervisorCoreRegistry : CoreRegistry
    {
        protected override IEnumerable<Assembly> GetAssembliesForRegistration()
        {
            return base.GetAssembliesForRegistration().Concat(new[]
            {
                typeof(UserViewFactory).Assembly,
                typeof(QuestionnaireMembershipProvider).Assembly,
                typeof(DataCollectionSharedKernelModule).Assembly,
                typeof(UserDenormalizer).Assembly
            });
        }

        protected override void RegisterDenormalizers() { }

        protected override IEnumerable<KeyValuePair<Type, Type>> GetTypesForRegistration()
        {
            var supervisorSpecificTypes = new Dictionary<Type, Type>
            {
                { typeof(IExceptionFilter), typeof(HandleUIExceptionAttribute) }
            };

            return base.GetTypesForRegistration().Concat(supervisorSpecificTypes);
        }
        protected override void RegisterEventHandlers()
        {
            base.RegisterEventHandlers();

            BindInterface(this.GetAssembliesForRegistration(), typeof(IEventHandler), (c) => this.Kernel);
        }

        public override void Load()
        {
            base.Load();

            RegisterViewFactories();

            this.Bind<IJsonUtils>().To<NewtonJsonUtils>();
            this.Bind<IStringCompressor>().To<GZipJsonCompressor>();
            this.Bind<IRevalidateInterviewsAdministrationService>().To<RevalidateInterviewsAdministrationService>().InSingletonScope();
        }
    }
}