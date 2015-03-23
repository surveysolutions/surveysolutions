using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;

namespace WB.UI.QuestionnaireTester.Ninject
{
    public class CapiTesterCoreRegistry : CoreRegistry
    {
        protected override IEnumerable<Assembly> GetAssembliesForRegistration()
        {
            return
                Enumerable.Concat(base.GetAssembliesForRegistration(), new[] { typeof(ImportFromDesignerForTester).Assembly, this.GetType().Assembly });
        }
    }
}