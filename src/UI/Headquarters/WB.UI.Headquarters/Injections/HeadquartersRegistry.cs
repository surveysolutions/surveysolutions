using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using Main.Core;
using WB.Core.BoundedContexts.Headquarters;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernel.Utils.Compression;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveyManagement.Views.User;
using WB.Core.SharedKernels.SurveyManagement.Web.Utils.Security;
using WB.UI.Shared.Web.Filters;

namespace WB.UI.Headquarters.Injections
{
    public class HeadquartersRegistry : CoreRegistry
    {
        protected override IEnumerable<Assembly> GetAssembliesForRegistration()
        {
            return base.GetAssembliesForRegistration().Concat(new[]
            {
                typeof(UserViewFactory).Assembly,
                typeof(QuestionnaireMembershipProvider).Assembly,
                typeof(DataCollectionSharedKernelModule).Assembly,
                typeof(HeadquartersBoundedContextModule).Assembly
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

            this.BindInterface(this.GetAssembliesForRegistration(), typeof(IEventHandler), (c) => this.Kernel);
        }

        public override void Load()
        {
            base.Load();

            this.RegisterViewFactories();

            this.Bind<IJsonUtils>().To<NewtonJsonUtils>();
            this.Bind<IStringCompressor>().To<GZipJsonCompressor>();
        }
    }
}