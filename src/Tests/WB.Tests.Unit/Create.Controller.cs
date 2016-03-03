using System;
using System.Net.Http;
using System.Web.Http;
using Main.Core.Documents;
using Moq;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer;
using WB.Core.SharedKernels.SurveyManagement.Web.Api.Interviewer.v1;

namespace WB.Tests.Unit
{
    internal static partial class Create
    {
        internal static class Controller
        {
            public static QuestionnairesApiController InterviewerQuestionnaires(
            IQuestionnaireAssemblyFileAccessor questionnareAssemblyFileAccessor = null,
            IQuestionnaireBrowseViewFactory questionnaireBrowseViewFactory = null,
            ISerializer serializer = null,
            QuestionnaireDocument questionnaire=null,
            QuestionnaireBrowseItem questionnaireBrowseItem=null
            )
            {
                return new QuestionnairesApiController(
                    questionnareAssemblyFileAccessor ?? Mock.Of<IQuestionnaireAssemblyFileAccessor>(),
                    questionnaireBrowseViewFactory ?? Mock.Of<IQuestionnaireBrowseViewFactory>(),
                    serializer ?? Mock.Of<ISerializer>(),
                    Mock.Of<IPlainQuestionnaireRepository>(_ => _.GetQuestionnaireDocument(Moq.It.IsAny<Guid>(), Moq.It.IsAny<long>()) == questionnaire),
                    Mock.Of<IPlainStorageAccessor<QuestionnaireBrowseItem>>(_ => _.GetById(Moq.It.IsAny<object>()) == questionnaireBrowseItem))
                {
                    Request = new HttpRequestMessage(),
                    Configuration = new HttpConfiguration()
                };
            }
        }
    }
}
