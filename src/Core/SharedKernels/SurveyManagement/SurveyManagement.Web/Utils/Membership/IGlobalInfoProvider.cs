using System;
using Main.Core.Entities.SubEntities;

namespace WB.Core.SharedKernels.SurveyManagement.Web.Utils.Membership
{


    public interface IGlobalInfoProvider
    {
        UserLight GetCurrentUser();

        bool IsAnyUserExist();

        bool IsHeadquarter { get; }

        bool IsSurepvisor { get; }

        bool IsAdministrator { get; }
    }
}