using System;
using System.Linq;
using DocumentFormat.OpenXml.Vml;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Headquarters.Services;
using WB.Core.BoundedContexts.Headquarters.Views.ChangeStatus;
using WB.Core.BoundedContexts.Headquarters.Views.Interview;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Enumerator.Native.WebInterview;
using WB.Enumerator.Native.WebInterview.Controllers;
using WB.Enumerator.Native.WebInterview.Models;
using WB.Tests.Abc;
using WB.UI.Headquarters.API.WebInterview;
using WB.UI.Headquarters.Services;

namespace WB.Tests.Web.Headquarters.Controllers.WebInterview
{
    public class InterviewDataControllerTests
    {
        
        [Test]
        public void GetNavigationButtonState_for_next_in_create_assignment_section_if_all_subsections_are_disabled()
        {
            var sectionId = Guid.NewGuid();
            var groupId = Guid.NewGuid();
            var intQuestionId = Guid.NewGuid();
            
            var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocument(children: new IComposite[]
            {
                Abc.Create.Entity.Group(sectionId, enablementCondition:"1!=1",children: new IComposite[]
                {
                    Abc.Create.Entity.NumericIntegerQuestion(intQuestionId)
                    
                })
            });
            

            var plainQuestionnaire = Abc.Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var statefulInterview = SetUp.StatefulInterview(questionnaireDocument);
            
            statefulInterview.GetGroup(new Identity(sectionId, RosterVector.Empty)).Disable();
            
            var webInterview = CreateInterviewDataController(statefulInterview, questionnaireDocument);

            var navigationButtonState = webInterview.GetNavigationButtonState(statefulInterview.Id, null, "NavigationButton", plainQuestionnaire);

            Assert.That(navigationButtonState.Type, Is.EqualTo(ButtonType.Complete));
        }

        [TestCase("not-a-guid")]
        [TestCase("00000000-0000-0000-0000-000000000000")]
        public void GetFullSectionInfo_returns_null_when_section_id_is_not_a_valid_identity(string invalidSectionId)
        {
            var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocument();
            var statefulInterview = SetUp.StatefulInterview(questionnaireDocument);
            var controller = CreateInterviewDataController(statefulInterview, questionnaireDocument);

            var section = controller.GetFullSectionInfo(statefulInterview.Id, invalidSectionId);

            Assert.That(section, Is.Null);
        }

        [Test]
        public void GetSectionEntities_returns_null_when_section_id_is_not_a_valid_identity()
        {
            var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocument();
            var statefulInterview = SetUp.StatefulInterview(questionnaireDocument);
            var controller = CreateInterviewDataController(statefulInterview, questionnaireDocument);

            var entities = controller.GetSectionEntities(statefulInterview.Id, "not-a-guid");

            Assert.That(entities, Is.Null);
        }
        
        
        private InterviewDataController CreateInterviewDataController(IStatefulInterview statefulInterview, 
            QuestionnaireDocument questionnaireDocument)
        { 
            var statefulInterviewRepository = Abc.Create.Fake.StatefulInterviewRepositoryWith(statefulInterview);
            var questionnaireStorage = SetUp.QuestionnaireRepositoryWithOneQuestionnaire(Abc.Create.Entity.PlainQuestionnaire(questionnaireDocument));
            var webInterviewInterviewEntityFactory = Web.Create.Service.WebInterviewInterviewEntityFactory();

            var controller = new WB.UI.Headquarters.Controllers.Api.WebInterview.InterviewDataController(questionnaireStorage,
                statefulInterviewRepository,
                Mock.Of<IWebInterviewNotificationService>(),
                webInterviewInterviewEntityFactory,
                Mock.Of<IAuthorizedUser>(),
                Mock.Of<IInterviewOverviewService>(),
                Mock.Of<IStatefulInterviewSearcher>(),
                Mock.Of<IInterviewFactory>(),
                Mock.Of<IChangeStatusFactory>(),
                Mock.Of<IInterviewBrokenPackagesService>(),
                Mock.Of<IQuestionnaireSettings>());

            return controller;
        }
    }
}
