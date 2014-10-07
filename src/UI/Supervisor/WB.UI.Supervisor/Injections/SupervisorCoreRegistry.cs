using Main.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernel.Utils.Compression;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveyManagement;
using WB.Core.SharedKernels.SurveyManagement.EventHandler;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Security;
using WB.UI.Shared.Web.Filters;
using WB.UI.Supervisor.Code;

namespace WB.UI.Supervisor.Injections
{
    public class SupervisorCoreRegistry : CoreRegistry
    {
        protected override IEnumerable<Assembly> GetAssembliesForRegistration()
        {
            return base.GetAssembliesForRegistration().Concat(new[]
            {
                typeof(SurveyManagementSharedKernelModule).Assembly,
                typeof(QuestionnaireMembershipProvider).Assembly,
                typeof(DataCollectionSharedKernelModule).Assembly,
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
        }
    }
}