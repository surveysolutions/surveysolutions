using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Utils.Implementation;
using WB.Core.GenericSubdomains.Utils.Services;
using WB.Core.SharedKernels.SurveySolutions.Api.Designer;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Services
{
    public class DesignerApiServiceAccessor
    {
        private readonly IRestService restService;
        private readonly IPrincipal principal;

        public DesignerApiServiceAccessor(IRestService restService, IPrincipal principal)
        {
            this.restService = restService;
            this.principal = principal;
        }

        public Task<QuestionnaireListItem[]> GetPagedQuestionnairesAsync(int pageIndex, int pageSize, CancellationToken token)
        {
            return this.restService.GetAsync<QuestionnaireListItem[]>(
                url: "questionnaires",
                token: token,
                credentials:
                    new RestCredentials()
                    {
                        Login = this.principal.CurrentUserIdentity.Name,
                        Password = this.principal.CurrentUserIdentity.Password
                    },
                queryString: new { pageIndex = pageIndex, pageSize = pageSize });
        }

        public async Task<Questionnaire> GetQuestionnaireAsync(string questionnaireId, Action<decimal> downloadProgress, CancellationToken token)
        {
            return await this.restService.GetWithProgressAsync<Questionnaire>(
                url: string.Format("questionnaires/{0}", questionnaireId),
                credentials:
                    new RestCredentials()
                    {
                        Login = this.principal.CurrentUserIdentity.Name,
                        Password = this.principal.CurrentUserIdentity.Password
                    },
                progressPercentage: downloadProgress, token: token);
        }
    }
}