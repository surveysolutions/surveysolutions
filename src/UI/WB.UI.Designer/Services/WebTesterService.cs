using System;
using WB.Core.BoundedContexts.Designer.MembershipProvider;

namespace WB.UI.Designer.Services
{
    public class WebTesterService : IWebTesterService
    {
        private readonly IJwtTokenService jwtTokenService;

        public WebTesterService(IJwtTokenService jwtTokenService)
        {
            this.jwtTokenService = jwtTokenService;
        }

        public string CreateTestQuestionnaire(Guid questionnaireId, DesignerIdentityUser? user = null)
        {
            return jwtTokenService.GenerateWebTesterToken(user, questionnaireId);
        }
    }
}
