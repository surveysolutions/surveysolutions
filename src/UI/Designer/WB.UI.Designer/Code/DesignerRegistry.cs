using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Main.Core;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.UI.Designer.Services;

namespace WB.UI.Designer.Code
{
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

        protected override void RegisterDenormalizers() { }
    }
}