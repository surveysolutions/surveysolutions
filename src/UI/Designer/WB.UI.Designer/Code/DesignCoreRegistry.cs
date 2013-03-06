using WB.UI.Designer.Providers.CQRS;
using WB.UI.Designer.Providers.CQRS.Accounts;
using Main.Core;
using System.Linq;

namespace WB.UI.Designer
{
    using WB.UI.Designer.Providers.CQRS.Accounts;

    public class DesignCoreRegistry : CoreRegistry
    {
        public DesignCoreRegistry(string repositoryPath, bool isEmbeded)
            : base(repositoryPath, isEmbeded)
        {
 
        }

        public override System.Collections.Generic.IEnumerable<System.Reflection.Assembly> GetAssweblysForRegister()
        {
            return Enumerable.Concat(base.GetAssweblysForRegister(), new[] {typeof (AccountAR).Assembly});
        }
    }
}