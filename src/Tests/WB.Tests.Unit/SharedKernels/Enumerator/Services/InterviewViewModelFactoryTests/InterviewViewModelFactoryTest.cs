using System;
using System.Linq;
using Main.Core.Entities.Composite;
using Moq;
using MvvmCross;
using MvvmCross.Base;
using MvvmCross.Core;
using MvvmCross.Plugin.Messenger;
using MvvmCross.Tests;
using NUnit.Framework;
using WB.Core.BoundedContexts.Interviewer.Services;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Services.InterviewViewModelFactoryTests
{
    [TestOf(typeof(InterviewViewModelFactory))]
    [TestFixture]
    public class InterviewViewModelFactoryTest: MvxIoCSupportingTest
    {
        [SetUp]
        public void SetupMvx()
        {
            base.Setup();
            
            Ioc.RegisterSingleton<IMvxMainThreadAsyncDispatcher>(Stub.MvxMainThreadAsyncDispatcher());
            Ioc.RegisterSingleton<IMvxMessenger>(Mock.Of<IMvxMessenger>());
        }
    
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
            var questionnaireStorage = Mock.Of<IQuestionnaireStorage>(s => 
                s.GetQuestionnaire(Moq.It.IsAny<QuestionnaireIdentity>(), null) == plainQuestionnaire
                && s.GetQuestionnaireOrThrow(Moq.It.IsAny<QuestionnaireIdentity>(), null) == plainQuestionnaire);

            var statefulInterview = SetUp.StatefulInterview(questionnaire);
            var statefulInterviewRepository = SetUp.StatefulInterviewRepository(statefulInterview);

            var settings = Mock.Of<IInterviewerSettings>(s => s.ShowVariables == false);
            var navigationState = Mock.Of<NavigationState>();

            var serviceLocator = new Mock<IServiceLocator>();
            serviceLocator.Setup(x => x.GetInstance<TextQuestionViewModel>())
                .Returns(Create.ViewModel.TextQuestionViewModel(interviewRepository: statefulInterviewRepository,
                    questionnaireStorage: questionnaireStorage));

            var factory = Create.Service.InterviewViewModelFactory(questionnaireStorage, statefulInterviewRepository, serviceLocator.Object, settings);
            
            
            //act
            var entities = factory.GetEntities(interviewId.ToString(), chapterIdentity, navigationState).ToList();
            
            //assert
            Assert.That(entities.Count, Is.EqualTo(1));
            Assert.IsAssignableFrom<TextQuestionViewModel>(entities[0]);
        }
    }
}
