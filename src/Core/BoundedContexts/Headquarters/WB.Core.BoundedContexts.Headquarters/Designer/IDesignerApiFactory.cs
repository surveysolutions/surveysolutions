using WB.Core.BoundedContexts.Headquarters.Implementation.Services;

namespace WB.Core.BoundedContexts.Headquarters.Designer
{
    public interface IDesignerApiFactory
    {
        IDesignerApi Get();
        IDesignerApi Get(IDesignerUserCredentials userCredentials);
    }
}
