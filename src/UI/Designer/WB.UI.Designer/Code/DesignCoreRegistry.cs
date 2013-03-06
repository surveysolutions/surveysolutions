using Designer.Web.Providers.CQRS;
using Designer.Web.Providers.CQRS.Accounts;
using Main.Core;
using System.Linq;

namespace Designer.Web
{
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