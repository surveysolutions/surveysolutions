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
using WebInterviewEntity = WB.Enumerator.Native.WebInterview.Models.InterviewEntity;

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

        [Test]
        public void GetFullSectionInfo_should_extract_variable_names_and_clear_names_in_details()
        {
            var interviewId = Guid.NewGuid();
            var sectionId = Guid.NewGuid().ToString();
            var variableId1 = Guid.NewGuid().ToString();
            var variableId2 = Guid.NewGuid().ToString();

            var details = new WebInterviewEntity[]
            {
                new InterviewVariable { Id = variableId1, Name = "first_var" },
                new InterviewVariable { Id = variableId2, Name = "second_var" },
            };

            var sectionEntities = new[]
            {
                new InterviewEntityWithType { Identity = variableId1, EntityType = InterviewEntityType.Variable.ToString() },
                new InterviewEntityWithType { Identity = variableId2, EntityType = InterviewEntityType.Variable.ToString() },
            };

            var controller = new Mock<WB.UI.Headquarters.Controllers.Api.WebInterview.InterviewDataController>(
                Mock.Of<IQuestionnaireStorage>(),
                Mock.Of<IStatefulInterviewRepository>(),
                Mock.Of<IWebInterviewNotificationService>(),
                Mock.Of<IWebInterviewInterviewEntityFactory>(),
                Mock.Of<IAuthorizedUser>(),
                Mock.Of<IInterviewOverviewService>(),
                Mock.Of<IStatefulInterviewSearcher>(),
                Mock.Of<IInterviewFactory>(),
                Mock.Of<IChangeStatusFactory>(),
                Mock.Of<IInterviewBrokenPackagesService>(),
                Mock.Of<IQuestionnaireSettings>())
            {
                CallBase = true
            };

            controller.Setup(x => x.GetSectionEntities(interviewId, sectionId)).Returns(sectionEntities);
            controller.Setup(x => x.GetEntitiesDetails(interviewId, It.IsAny<string[]>(), sectionId)).Returns(details);

            var result = controller.Object.GetFullSectionInfo(interviewId, sectionId);

            Assert.That(result.VariableNames[variableId1], Is.EqualTo("first_var"));
            Assert.That(result.VariableNames[variableId2], Is.EqualTo("second_var"));
            Assert.That(result.Details.All(x => x.Name == null), Is.True);
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
