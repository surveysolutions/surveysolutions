using System.Linq;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.Modularity;
using WB.UI.Shared.Web.CommandDeserialization;

namespace WB.UI.Designer.CommandDeserialization
{
    public class DesignerCommandDeserializationModule : IModule
    {
        public void Load(IIocRegistry registry)
        {
            registry.Bind<ICommandDeserializer, DesignerCommandDeserializer>();
        }

        public void Init(IServiceLocator serviceLocator)
        {
            
        }
    }
}
