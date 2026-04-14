using System;
using WB.Core.BoundedContexts.Designer.MembershipProvider;

namespace WB.UI.Designer.Services
{
    public interface IWebTesterService
    {
        string CreateTestQuestionnaire(Guid questionnaireId, DesignerIdentityUser? user = null);
    }
}