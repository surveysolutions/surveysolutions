using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Main.Core;
using Main.Core.Documents;
using Main.DenormalizerStorage;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ninject;
using WB.Core.GenericSubdomains.Logging;
using WB.Core.Questionnaire.ExportServices;
using WB.Core.SharedKernel.Utils.Logging;
using WB.UI.Designer.Providers.CQRS.Accounts;
using WB.UI.Designer.Views.Questionnaire;

namespace WB.UI.Designer.Code
{
    using System;
    using System.Web.Mvc;

    using WB.UI.Designer.Filters;
    using WB.UI.Designer.WebServices;

    public class DesignerRegistry : CoreRegistry
    {
        public DesignerRegistry(string repositoryPath, bool isEmbeded)
            : base(repositoryPath, isEmbeded) {}

        public override IEnumerable<Assembly> GetAssweblysForRegister()
        {
            return
                base.GetAssweblysForRegister()
                    .Concat(new[]
                    {
                        typeof(QuestionnaireView).Assembly, 
                        typeof(DesignerRegistry).Assembly,
                        typeof(AccountAR).Assembly,
                        typeof(PublicService).Assembly
                    });
        }
        
        public override void Load()
        {
            base.Load();
            this.Bind<ILogger>().ToMethod(
                context => LogManager.GetLogger(context.Request.Target.Member.DeclaringType));
        }

        protected override IEnumerable<KeyValuePair<Type, Type>> GetTypesForRegistration()
        {
            return base.GetTypesForRegistration().Concat(new Dictionary<Type, Type>
            {
                { typeof(IFilterProvider), typeof(RequiresReadLayerFilterProvider) },
            });
        }
    }
}