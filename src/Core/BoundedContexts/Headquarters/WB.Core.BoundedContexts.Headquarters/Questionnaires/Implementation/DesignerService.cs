using System;
using System.Linq;
using System.Web;
using WB.Core.BoundedContexts.Headquarters.DesignerPublicService;
using QuestionnaireVersion = WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects.QuestionnaireVersion;

namespace WB.Core.BoundedContexts.Headquarters.Questionnaires.Implementation
{
    internal class DesignerService : IDesignerService
    {
        private readonly HttpSessionStateBase sessionState = null;
        private const string DesignerUserName = "DesignerUserName";
        private const string DesignerPassword = "DesignerPassword";


        public DesignerService(HttpContextBase httpContext)
        {
            this.sessionState = httpContext.Session;
        }

        public void TryLogin(string userName, string password)
        {
            var service = new PublicServiceClient();
            service.ClientCredentials.UserName.UserName = userName;
            service.ClientCredentials.UserName.Password = password;
            service.Dummy();

            sessionState[DesignerUserName] = userName;
            sessionState[DesignerPassword] = password;
        }

        public QuestionnaireListDto GetQuestionnaireList(string filter, int page, int pageSize, string sortOrder)
        {
            var service = this.GetService();

           
            var questionnaireListRequest = new QuestionnaireListRequest(filter, page, pageSize, sortOrder);

            QuestionnaireListViewMessage list = service.GetQuestionnaireList(questionnaireListRequest);

            return new QuestionnaireListDto
            {
                Total = list.TotalCount,
                Items = list.Items.Select(x => new QuestionnaireDto { Id = x.Id, Title = x.Title }).ToList()
            };
        }

        public RemoteFileInfo DownloadQuestionnaire(Guid questionnaireId, QuestionnaireVersion version)
        {
            var service = this.GetService();

            DownloadQuestionnaireRequest requst = new DownloadQuestionnaireRequest(questionnaireId, new DesignerPublicService.QuestionnaireVersion
            {
                Major = version.Major,
                Minor = version.Minor,
                Patch = version.Patch
            });
            var remoteFileInfo = service.DownloadQuestionnaire(requst);

            return new RemoteFileInfo{FileByteStream = remoteFileInfo.FileByteStream};
        }

        public bool CurrentUserIsLoggedIn()
        {
           return  !string.IsNullOrEmpty((string) this.sessionState[DesignerUserName]) && !string.IsNullOrEmpty((string) this.sessionState[DesignerPassword]);
        }

        private IPublicService GetService()
        {
            var service = new PublicServiceClient();
            service.ClientCredentials.UserName.UserName = (string) this.sessionState[DesignerUserName];
            service.ClientCredentials.UserName.Password = (string) this.sessionState[DesignerPassword];
            return service;
        }
    }
}