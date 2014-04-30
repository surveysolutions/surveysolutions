using Main.Core;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WB.Core.SharedKernels.DataCollection.Commands.Questionnaire;
using UserDenormalizer = WB.Core.SharedKernels.SurveyManagement.EventHandler.UserDenormalizer;

namespace CapiDataGenerator
{
    public class CapiDataGeneratorRegistry : CoreRegistry
    {
        protected override IEnumerable<Assembly> GetAssembliesForRegistration()
        {
            return base.GetAssembliesForRegistration().Concat(new[]
            {
                GetType().Assembly,
                typeof(UserDenormalizer).Assembly,
                typeof(ImportFromDesigner).Assembly,
            });
        }

        protected override void RegisterDenormalizers()
        {
        }
    }
}
