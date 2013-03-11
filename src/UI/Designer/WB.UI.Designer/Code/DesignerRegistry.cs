using Main.Core.Events;
using Main.Core.Export;
using Main.Core.View.Export;
using RavenQuestionnaire.Core.Views.Questionnaire;
using WB.UI.Designer.Providers.CQRS;
using WB.UI.Designer.Providers.CQRS.Accounts;
using Main.Core;
using System.Linq;

namespace WB.UI.Designer
{
    using WB.UI.Designer.Providers.CQRS.Accounts;

    public class DesignerRegistry : CoreRegistry
    {
        public DesignerRegistry(string repositoryPath, bool isEmbeded)
            : base(repositoryPath, isEmbeded)
        {
 
        }

        public override System.Collections.Generic.IEnumerable<System.Reflection.Assembly> GetAssweblysForRegister()
        {
            return Enumerable.Concat(base.GetAssweblysForRegister(), new[]
                {
                    typeof(QuestionnaireView).Assembly, 
                    typeof (AccountAR).Assembly
                });
        }
    }
}