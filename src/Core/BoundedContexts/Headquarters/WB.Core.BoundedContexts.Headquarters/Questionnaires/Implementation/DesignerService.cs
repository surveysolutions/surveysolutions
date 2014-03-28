using System.Linq;
using System.Web;
using WB.Core.BoundedContexts.Headquarters.DesignerPublicService;

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

        public QuestionnaireListDto GetQuestionnaireList(string filter, int page, int pageSize)
        {
            var service = new PublicServiceClient();
            service.ClientCredentials.UserName.UserName = (string) this.sessionState[DesignerUserName];
            service.ClientCredentials.UserName.Password = (string) this.sessionState[DesignerPassword];

            var publicService = service as IPublicService;
            var questionnaireListRequest = new QuestionnaireListRequest(filter, page, pageSize, null);

            QuestionnaireListViewMessage list = publicService.GetQuestionnaireList(questionnaireListRequest);

            return new QuestionnaireListDto
            {
                Total = list.TotalCount,
                Items = list.Items.Select(x => new QuestionnaireDto { Id = x.Id, Title = x.Title }).ToList()
            };
        }
    }
}