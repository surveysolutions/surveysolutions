using System;
using System.Collections.Generic;
using Main.Core.Entities.Composite;
using MvvmCross;
using MvvmCross.Core;
using MvvmCross.Tests;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.Services.CompositeCollectionInflationServiceTests
{
    [TestFixture]
    internal class CompositeCollectionInflationServiceTests: MvxIoCSupportingTest
    {
        [SetUp]
        public void SetupMvx()
        {
            base.Setup();
            Mvx.Resolve<IMvxSettings>().AlwaysRaiseInpcOnUserInterfaceThread = false;
        }

        [Test]
        public void When_getting_inflated_composite_collection_with_2_enabled_questions_Then_collection_should_contains__all_composite_items_of_2_enabled_questions()
        {
            //Arrange
            var enabledQuestionIdentity1 = new Identity(Guid.NewGuid(), RosterVector.Empty);
            var enabledQuestionIdentity2 = new Identity(Guid.NewGuid(), RosterVector.Empty);
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextQuestion(enabledQuestionIdentity2.Id, instruction: "some instruction", variable: "q1"),
                Create.Entity.TextQuestion(enabledQuestionIdentity1.Id, variable: "q2")
            });

            var questionnaireId = Guid.NewGuid();
            string interviewId = "interview id";
            var liteEventRegistry = Create.Service.LiteEventRegistry();
            var statefulInterview = Create.AggregateRoot.StatefulInterview(userId: null, questionnaire: questionnaire, questionnaireId: questionnaireId);
            var statefullInterviewRepository = Create.Fake.StatefulInterviewRepositoryWith(statefulInterview);
            var questionnaireRepositoryWithOneQuestionnaire = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);

            var enabledQuestionViewModel1 = Create.ViewModel.TextQuestionViewModel(interviewRepository: statefullInterviewRepository,
                eventRegistry: liteEventRegistry, questionnaireStorage: questionnaireRepositoryWithOneQuestionnaire);
            enabledQuestionViewModel1.Init(interviewId, enabledQuestionIdentity1, Create.Other.NavigationState());

            var enabledQuestionViewModel2 = Create.ViewModel.TextQuestionViewModel(interviewRepository: statefullInterviewRepository,
                eventRegistry: liteEventRegistry, questionnaireStorage: questionnaireRepositoryWithOneQuestionnaire);
            enabledQuestionViewModel2.Init(interviewId, enabledQuestionIdentity2, Create.Other.NavigationState());

            var listOfViewModels = new List<IInterviewEntityViewModel>
            {
                enabledQuestionViewModel2,
                enabledQuestionViewModel1
            };

            var compositeCollectionInflationService = CreateCompositeCollectionInflationService();

            //Act
            var inflatedViewModels = compositeCollectionInflationService.GetInflatedCompositeCollection(listOfViewModels);

            //Assert
            Assert.That(inflatedViewModels, Contains.Item(enabledQuestionViewModel1.QuestionState.Header));
            Assert.That(inflatedViewModels, Contains.Item(enabledQuestionViewModel1));
            Assert.That(inflatedViewModels, Contains.Item(enabledQuestionViewModel1.QuestionState.Validity));
            Assert.That(inflatedViewModels, Contains.Item(enabledQuestionViewModel1.QuestionState.Comments));
            Assert.That(inflatedViewModels, Contains.Item(enabledQuestionViewModel1.Answering));

            Assert.That(inflatedViewModels, Contains.Item(enabledQuestionViewModel2.QuestionState.Header));
            Assert.That(inflatedViewModels, Contains.Item(enabledQuestionViewModel2));
            Assert.That(inflatedViewModels, Contains.Item(enabledQuestionViewModel2.InstructionViewModel));
            Assert.That(inflatedViewModels, Contains.Item(enabledQuestionViewModel2.QuestionState.Validity));
            Assert.That(inflatedViewModels, Contains.Item(enabledQuestionViewModel2.QuestionState.Comments));
            Assert.That(inflatedViewModels, Contains.Item(enabledQuestionViewModel2.Answering));
        }

        [Test]
        public void When_getting_inflated_composite_collection_with_1_enabled_question_and_1_disabled_Then_collection_should_not_contains_composite_items_of_disabled_question()
        {
            //Arrange
            var disabledQuestionIdentity = new Identity(Guid.NewGuid(), RosterVector.Empty);
            var enabledQuestionIdentity = new Identity(Guid.NewGuid(), RosterVector.Empty);
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextQuestion(enabledQuestionIdentity.Id, instruction: "some instruction", variable: "q1"),
                Create.Entity.TextQuestion(disabledQuestionIdentity.Id, variable: "q2")
            });

            var questionnaireId = Guid.NewGuid();
            string interviewId = "interview id";
            var liteEventRegistry = Create.Service.LiteEventRegistry();
            var statefulInterview = Create.AggregateRoot.StatefulInterview(userId: null, questionnaire: questionnaire, questionnaireId: questionnaireId);
            var statefullInterviewRepository = Create.Fake.StatefulInterviewRepositoryWith(statefulInterview);
            var questionnaireRepositoryWithOneQuestionnaire = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);

            var disabledQuestionViewModel = Create.ViewModel.TextQuestionViewModel(interviewRepository: statefullInterviewRepository,
                eventRegistry: liteEventRegistry, questionnaireStorage: questionnaireRepositoryWithOneQuestionnaire);
            disabledQuestionViewModel.Init(interviewId, disabledQuestionIdentity, Create.Other.NavigationState());

            var enabledQuestionViewModel = Create.ViewModel.TextQuestionViewModel(interviewRepository: statefullInterviewRepository,
                eventRegistry: liteEventRegistry, questionnaireStorage: questionnaireRepositoryWithOneQuestionnaire);
            enabledQuestionViewModel.Init(interviewId, enabledQuestionIdentity, Create.Other.NavigationState());

            var listOfViewModels = new List<IInterviewEntityViewModel>
            {
                enabledQuestionViewModel,
                disabledQuestionViewModel
            };

            var compositeCollectionInflationService = CreateCompositeCollectionInflationService();

            //Act
            var inflatedViewModels = compositeCollectionInflationService.GetInflatedCompositeCollection(listOfViewModels);

            var questionDisabledEvent = Create.Event.QuestionsDisabled(new[] {disabledQuestionIdentity}); 
            statefulInterview.Apply(questionDisabledEvent);
            disabledQuestionViewModel.QuestionState.Enablement.Handle(questionDisabledEvent);

            //Assert
            Assert.That(inflatedViewModels, !Contains.Item(disabledQuestionViewModel.QuestionState.Validity));
            Assert.That(inflatedViewModels, !Contains.Item(disabledQuestionViewModel.InstructionViewModel));
            Assert.That(inflatedViewModels, !Contains.Item(disabledQuestionViewModel.QuestionState.Comments));
            Assert.That(inflatedViewModels, !Contains.Item(disabledQuestionViewModel));
            Assert.That(inflatedViewModels, !Contains.Item(disabledQuestionViewModel.Answering));
        }

        [Test]
        public void When_getting_inflated_composite_collection_with_disabled_question_and_hideifdisabled_is_true_Then_collection_should_not_contains_title_and_composite_items_of_disabled_question()
        {
            //Arrange
            var disabledQuestionIdentity = new Identity(Guid.NewGuid(), RosterVector.Empty);
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextQuestion(disabledQuestionIdentity.Id, hideIfDisabled: true, variable: "q1")
            });

            var questionnaireId = Guid.NewGuid();
            string interviewId = "interview id";
            var liteEventRegistry = Create.Service.LiteEventRegistry();
            var statefulInterview = Create.AggregateRoot.StatefulInterview(userId: null, questionnaire: questionnaire, questionnaireId: questionnaireId);
            var statefullInterviewRepository = Create.Fake.StatefulInterviewRepositoryWith(statefulInterview);
            var questionnaireRepositoryWithOneQuestionnaire = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);

            var disabledQuestionViewModel = Create.ViewModel.TextQuestionViewModel(interviewRepository: statefullInterviewRepository,
                eventRegistry: liteEventRegistry, questionnaireStorage: questionnaireRepositoryWithOneQuestionnaire);
            disabledQuestionViewModel.Init(interviewId, disabledQuestionIdentity, Create.Other.NavigationState());

            var listOfViewModels = new List<IInterviewEntityViewModel>
            {
                disabledQuestionViewModel
            };

            var compositeCollectionInflationService = CreateCompositeCollectionInflationService();

            //Act
            var inflatedViewModels = compositeCollectionInflationService.GetInflatedCompositeCollection(listOfViewModels);

            var questionDisabledEvent = Create.Event.QuestionsDisabled(new[] { disabledQuestionIdentity });
            statefulInterview.Apply(questionDisabledEvent);
            disabledQuestionViewModel.QuestionState.Enablement.Handle(questionDisabledEvent);

            //Assert
            Assert.That(inflatedViewModels, !Contains.Item(disabledQuestionViewModel.QuestionState.Header));
            Assert.That(inflatedViewModels, !Contains.Item(disabledQuestionViewModel.QuestionState.Validity));
            Assert.That(inflatedViewModels, !Contains.Item(disabledQuestionViewModel.InstructionViewModel));
            Assert.That(inflatedViewModels, !Contains.Item(disabledQuestionViewModel.QuestionState.Comments));
            Assert.That(inflatedViewModels, !Contains.Item(disabledQuestionViewModel));
            Assert.That(inflatedViewModels, !Contains.Item(disabledQuestionViewModel.Answering));
        }

        [Test]
        public void when_getting_inflated_composite_collection_with_1_valid_question_and_1_invalid_Then_collection_should_contains_validity_view_models_for_valid_and_invalid_questions()
        {
            //Arrange
            var invalidQuestionIdentity = new Identity(Guid.NewGuid(), RosterVector.Empty);
            var validQuestionIdentity = new Identity(Guid.NewGuid(), RosterVector.Empty);
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextQuestion(validQuestionIdentity.Id, variable: "q1"),
                Create.Entity.TextQuestion(invalidQuestionIdentity.Id, variable: "q2")
            });

            var questionnaireId = Guid.NewGuid();
            string interviewId = "interview id";
            var liteEventRegistry = Create.Service.LiteEventRegistry();
            var statefulInterview = Create.AggregateRoot.StatefulInterview(questionnaireId: questionnaireId, userId: null, questionnaire: questionnaire);
            var statefullInterviewRepository = Create.Fake.StatefulInterviewRepositoryWith(statefulInterview);
            var questionnaireRepositoryWithOneQuestionnaire = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);

            var invalidQuestionViewModel = Create.ViewModel.TextQuestionViewModel(interviewRepository: statefullInterviewRepository,
                eventRegistry: liteEventRegistry, questionnaireStorage: questionnaireRepositoryWithOneQuestionnaire);
            invalidQuestionViewModel.Init(interviewId, invalidQuestionIdentity, Create.Other.NavigationState());

            var validQuestionViewModel = Create.ViewModel.TextQuestionViewModel(interviewRepository: statefullInterviewRepository,
                eventRegistry: liteEventRegistry, questionnaireStorage: questionnaireRepositoryWithOneQuestionnaire);
            validQuestionViewModel.Init(interviewId, validQuestionIdentity, Create.Other.NavigationState());

            var listOfViewModels = new List<IInterviewEntityViewModel>
            {
                validQuestionViewModel,
                invalidQuestionViewModel
            };

            var compositeCollectionInflationService = CreateCompositeCollectionInflationService();

            //Act
            var inflatedViewModels = compositeCollectionInflationService.GetInflatedCompositeCollection(listOfViewModels);

            var questionInvalidEvent = Create.Event.AnswersDeclaredInvalid(new[] { invalidQuestionIdentity });
            statefulInterview.Apply(questionInvalidEvent);
            statefulInterview.Apply(Create.Event.TextQuestionAnswered(invalidQuestionIdentity.Id, invalidQuestionIdentity.RosterVector, "some answer"));
            invalidQuestionViewModel.QuestionState.Validity.Handle(questionInvalidEvent);

            //Assert
            Assert.That(inflatedViewModels, Contains.Item(invalidQuestionViewModel.QuestionState.Validity));
            Assert.That(inflatedViewModels, Contains.Item(validQuestionViewModel.QuestionState.Validity));
        }

        [Test]
        public void When_getting_inflated_composite_collection_and_1_question_with_progress_and_1_not_Then_collection_should_contains_answering_progress_for_question_with_progress_and_without()
        {
            //Arrange
            var questionWithProgressIdentity = new Identity(Guid.NewGuid(), RosterVector.Empty);
            var questionWithoutProgressIdentity = new Identity(Guid.NewGuid(), RosterVector.Empty);
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.TextQuestion(questionWithoutProgressIdentity.Id, variable: "q1"),
                Create.Entity.TextQuestion(questionWithProgressIdentity.Id, variable: "q2")
            });

            var questionnaireId = Guid.NewGuid();
            string interviewId = "interview id";
            var liteEventRegistry = Create.Service.LiteEventRegistry();
            var statefulInterview = Create.AggregateRoot.StatefulInterview(userId: null, questionnaire: questionnaire, questionnaireId: questionnaireId);
            var statefullInterviewRepository = Create.Fake.StatefulInterviewRepositoryWith(statefulInterview);
            var questionnaireRepositoryWithOneQuestionnaire = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaire);

            var answeringViewModel = Create.ViewModel.AnsweringViewModel();

            var questionWithProgressQuestionViewModel = Create.ViewModel.TextQuestionViewModel(interviewRepository: statefullInterviewRepository,
                eventRegistry: liteEventRegistry, questionnaireStorage: questionnaireRepositoryWithOneQuestionnaire, answering: answeringViewModel);
            questionWithProgressQuestionViewModel.Init(interviewId, questionWithProgressIdentity, Create.Other.NavigationState());

            var questionWithoutProgressViewModel = Create.ViewModel.TextQuestionViewModel(interviewRepository: statefullInterviewRepository,
                eventRegistry: liteEventRegistry, questionnaireStorage: questionnaireRepositoryWithOneQuestionnaire);
            questionWithoutProgressViewModel.Init(interviewId, questionWithoutProgressIdentity, Create.Other.NavigationState());

            var listOfViewModels = new List<IInterviewEntityViewModel>
            {
                questionWithoutProgressViewModel,
                questionWithProgressQuestionViewModel
            };

            var compositeCollectionInflationService = CreateCompositeCollectionInflationService();

            //Act
            var inflatedViewModels = compositeCollectionInflationService.GetInflatedCompositeCollection(listOfViewModels);

            questionWithProgressQuestionViewModel.Answering.StartInProgressIndicator();

            //Assert
            Assert.That(inflatedViewModels, Contains.Item(questionWithoutProgressViewModel.Answering));
            Assert.That(inflatedViewModels, Contains.Item(questionWithProgressQuestionViewModel.Answering));
        }

        private static CompositeCollectionInflationService CreateCompositeCollectionInflationService()
            => new CompositeCollectionInflationService(Stub.MvxMainThreadDispatcher());
    }
}
