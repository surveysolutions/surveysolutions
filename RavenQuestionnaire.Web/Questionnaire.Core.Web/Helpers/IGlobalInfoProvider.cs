using Main.Core.Entities.SubEntities;

namespace Questionnaire.Core.Web.Helpers
{


    public interface IGlobalInfoProvider
    {
        UserLight GetCurrentUser();

        bool IsAnyUserExist();

        bool IsHeadquarter { get; }

        bool IsSurepvisor { get; }
    }
}