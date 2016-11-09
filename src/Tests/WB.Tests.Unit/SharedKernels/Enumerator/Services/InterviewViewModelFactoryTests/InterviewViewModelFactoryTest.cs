using System;
using System.Linq;
using Main.Core.Entities.Composite;
using Moq;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Services.InterviewViewModelFactoryTests
{
    [TestOf(typeof(InterviewViewModelFactory))]
    [TestFixture]
    public class InterviewViewModelFactoryTest
    {
        [Test]
        public void When_GetEntities_with_settings_dont_show_variables_Then_should_return_all_entities_except_variables()
        {
            //arrange
            var interviewId = Guid.Parse("11111111111111111111111111111111");
            var chapterId = Guid.Parse("22222222222222222222222222222222");
            var chapterIdentity = Create.Entity.Identity(chapterId);

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(chapterId, interviewId,
                new IComposite[]
                {
                    Create.Entity.TextQuestion(),
                    Create.Entity.Variable(),
                });
            var plainQuestionnaire = Create.Entity.PlainQuestionnaire(questionnaire);
            var questionnaireStorage = Mock.Of<IQuestionnaireStorage>(s => s.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), null) == plainQuestionnaire);

            var statefulInterview = Setup.StatefulInterview(questionnaire);
            var statefulInterviewRepository = Setup.StatefulInterviewRepository(statefulInterview);

            var settings = Mock.Of<IInterviewerSettings>(s => s.ShowVariables == false);
            var navigationState = Mock.Of<NavigationState>();

            Setup.InstanceToMockedServiceLocator(Create.ViewModel.TextQuestionViewModel());

            var factory = Create.Service.InterviewViewModelFactory(questionnaireStorage, statefulInterviewRepository, settings);
            
            
            //act
            var entities = factory.GetEntities(interviewId.ToString(), chapterIdentity, navigationState).ToList();
            
            
            //assert
            Assert.That(entities.Count, Is.EqualTo(1));
            Assert.IsAssignableFrom<TextQuestionViewModel>(entities[0]);
        }
    }
}