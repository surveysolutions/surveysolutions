using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Main.Core;
using WB.Core.BoundedContexts.Headquarters.Questionnaires;
using WB.Core.BoundedContexts.Headquarters.Questionnaires.Implementation;
using WB.Core.Infrastructure.EventBus;
using WB.Core.SharedKernel.Utils.Compression;
using WB.Core.SharedKernel.Utils.Serialization;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using WB.Core.SharedKernels.DataCollection;
using WB.UI.Headquarters.Api.Models;
using WB.UI.Shared.Web.CommandDeserialization;

namespace WB.UI.Headquarters
{
    public class HeadquartersRegistry : CoreRegistry
    {
        protected override IEnumerable<Assembly> GetAssembliesForRegistration()
        {
            return base.GetAssembliesForRegistration().Concat(new[]
            {
                typeof(DataCollectionSharedKernelModule).Assembly
            });
        }

        protected override void RegisterEventHandlers()
        {
            base.RegisterEventHandlers();

            BindInterface(this.GetAssembliesForRegistration(), typeof(IEventHandler), (c) => this.Kernel);
        }

        protected override void RegisterDenormalizers()
        {
        }

        public override void Load()
        {
            base.Load();

            RegisterViewFactories();

            this.Bind<IJsonUtils>().To<NewtonJsonUtils>();
            this.Bind<IStringCompressor>().To<GZipJsonCompressor>();
            this.Bind<ISupportedVersionProvider>().To<SupportedVersionProvider>();
            this.Bind<ICommandDeserializer>().To<HeadquartersCommandDeserializer>();
        }
    }
}