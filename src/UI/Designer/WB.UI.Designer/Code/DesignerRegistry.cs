using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Configuration;
using Main.Core;
using Ncqrs;
using Ninject;
using Ninject.Activation;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.Infrastructure.Raven.Implementation.ReadSide.RepositoryAccessors;
using WB.UI.Designer.Providers.CQRS.Accounts;

namespace WB.UI.Designer.Code
{
    using System;
    using System.Web.Mvc;
    using WB.UI.Designer.WebServices;

    public class DesignerRegistry : CoreRegistry
    {
        protected override IEnumerable<Assembly> GetAssembliesForRegistration()
        {
            return
                base.GetAssembliesForRegistration()
                    .Concat(new[]
                    {
                        typeof(DesignerRegistry).Assembly,
                        typeof(PublicService).Assembly,
                        typeof(Questionnaire).Assembly

                    });
        }

        protected override object GetReadSideRepositoryWriter(IContext context)
        {
            return this.Kernel.Get(typeof(RavenReadSideRepositoryWriter<>).MakeGenericType(context.GenericArguments[0]));
        }

        protected override object GetReadSideRepositoryReader(IContext context)
        {
            return this.Kernel.Get(typeof(RavenReadSideRepositoryReader<>).MakeGenericType(context.GenericArguments[0]));
        }
    }
}