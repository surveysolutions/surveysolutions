using System.Net.Http;
using System.Web.Http;
using Moq;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer;

namespace WB.Tests.Unit
{
    internal static partial class Create
    {
        internal static class Controller
        {
            public static InterviewerQuestionnairesController InterviewerQuestionnaires(
            IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor = null,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory = null,
            ISerializer serializer = null)
            {
                return new InterviewerQuestionnairesController(
                    questionnareAssemblyFileAccessor ?? Mock.Of<IQuestionnaireAssemblyFileAccessor>(),
                    questionnaireBrowseViewFactory ?? Mock.Of<IQuestionnaireBrowseViewFactory>(),
                    serializer ?? Mock.Of<ISerializer>(),
                    Mock.Of<IPlainQuestionnaireRepository>(),
                    Mock.Of<IPlainStorageAccessor<QuestionnaireBrowseItem>>())
                {
                    Request = new HttpRequestMessage(),
                    Configuration = new HttpConfiguration()
                };
            }
        }
    }
}
