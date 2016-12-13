using Main.Core.Entities.SubEntities;
using WB.Core.GenericSubdomains.Portable.Implementation;

namespace WB.Core.BoundedContexts.Headquarters.Services
{
    public interface IGlobalInfoProvider
    {
        UserLight GetCurrentUser();

        RestCredentials GetDesignerUserCredentials();

        bool IsAnyUserExist();

        bool IsHeadquarter { get; }

        bool IsSupervisor { get; }

        bool IsAdministrator { get; }
    }
}