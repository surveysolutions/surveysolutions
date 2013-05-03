using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Main.Core;
using Main.Core.Documents;
using Main.DenormalizerStorage;
using Ncqrs;
using Ncqrs.Commanding.ServiceModel;
using Ninject;
using WB.Core.Questionnaire.ExportServices;
using WB.UI.Designer.Providers.CQRS.Accounts;
using WB.UI.Designer.Views.Questionnaire;

namespace WB.UI.Designer.Code
{
    using System;
    using System.Web.Mvc;

    using WB.UI.Designer.Filters;

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
                    });
        }
        public override void Load()
        {
            base.Load();
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