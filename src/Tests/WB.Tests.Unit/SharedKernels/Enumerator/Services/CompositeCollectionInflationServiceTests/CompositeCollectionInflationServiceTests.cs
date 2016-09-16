using System;
using System.Collections.Generic;
using Main.Core.Entities.Composite;
using MvvmCross.Core.Platform;
using MvvmCross.Platform;
using MvvmCross.Test.Core;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.Enumerator.Implementation.Services;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;

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
        public void when_getting_inflated_composite_collection_with_1_enabled_question_and_1_disabled()
        {
            var disabledQuestionIdentity = new Identity(Guid.NewGuid(), RosterVector.Empty);
            var enabledQuestionIdentity = new Identity(Guid.NewGuid(), RosterVector.Empty);
            IQuestionnaire questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Entity.TextQuestion(enabledQuestionIdentity.Id, instruction: "some instruction"),
                Create.Entity.TextQuestion(disabledQuestionIdentity.Id)
            }));

            var questionnaireId = Guid.NewGuid();
            string interviewId = "interview id";
            var liteEventRegistry = Create.Service.LiteEventRegistry();
            var statefulInterview = Create.AggregateRoot.StatefulInterview(userId: null, questionnaire: questionnaire, questionnaireId: questionnaireId);
            var statefullInterviewRepository = Create.Fake.StatefulInterviewRepositoryWith(statefulInterview);
            var questionnaireRepositoryWithOneQuestionnaire = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId, questionnaire);

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

            var inflatedViewModels = compositeCollectionInflationService.GetInflatedCompositeCollection(listOfViewModels);

            var questionDisabledEvent = Create.Event.QuestionsDisabled(new[] {disabledQuestionIdentity}); 
            statefulInterview.Apply(questionDisabledEvent);
            disabledQuestionViewModel.QuestionState.Enablement.Handle(questionDisabledEvent);

            Assert.That(inflatedViewModels, Contains.Item(disabledQuestionViewModel.QuestionState.Header));
            Assert.That(inflatedViewModels, !Contains.Item(disabledQuestionViewModel.QuestionState.Validity));
            Assert.That(inflatedViewModels, !Contains.Item(disabledQuestionViewModel.InstructionViewModel));
            Assert.That(inflatedViewModels, !Contains.Item(disabledQuestionViewModel.QuestionState.Comments));
            Assert.That(inflatedViewModels, !Contains.Item(disabledQuestionViewModel));

            Assert.That(inflatedViewModels, Contains.Item(enabledQuestionViewModel.QuestionState.Header));
            Assert.That(inflatedViewModels, Contains.Item(enabledQuestionViewModel.QuestionState.Comments));
            Assert.That(inflatedViewModels, Contains.Item(enabledQuestionViewModel.InstructionViewModel));
            Assert.That(inflatedViewModels, Contains.Item(enabledQuestionViewModel));
        }

        [Test]
        public void when_getting_inflated_composite_collection_with_1_valid_question_and_1_invalid()
        {
            var invalidQuestionIdentity = new Identity(Guid.NewGuid(), RosterVector.Empty);
            var validQuestionIdentity = new Identity(Guid.NewGuid(), RosterVector.Empty);
            IQuestionnaire questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Entity.TextQuestion(validQuestionIdentity.Id),
                Create.Entity.TextQuestion(invalidQuestionIdentity.Id)
            }));

            var questionnaireId = Guid.NewGuid();
            string interviewId = "interview id";
            var liteEventRegistry = Create.Service.LiteEventRegistry();
            var statefulInterview = Create.AggregateRoot.StatefulInterview(userId: null, questionnaire: questionnaire, questionnaireId: questionnaireId);
            var statefullInterviewRepository = Create.Fake.StatefulInterviewRepositoryWith(statefulInterview);
            var questionnaireRepositoryWithOneQuestionnaire = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId, questionnaire);

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

            var inflatedViewModels = compositeCollectionInflationService.GetInflatedCompositeCollection(listOfViewModels);

            var questionInvalidEvent = Create.Event.AnswersDeclaredInvalid(new[] { invalidQuestionIdentity });
            statefulInterview.Apply(questionInvalidEvent);
            statefulInterview.Apply(Create.Event.TextQuestionAnswered(invalidQuestionIdentity.Id, invalidQuestionIdentity.RosterVector, "some answer"));
            invalidQuestionViewModel.QuestionState.Validity.Handle(questionInvalidEvent);
    
            Assert.That(inflatedViewModels, Contains.Item(invalidQuestionViewModel.QuestionState.Validity));
            Assert.That(inflatedViewModels, Contains.Item(validQuestionViewModel.QuestionState.Validity));
        }

        [Test]
        public void when_getting_inflated_composite_collection_and_1_question_with_progress_and_1_not()
        {
            var questionWithProgressIdentity = new Identity(Guid.NewGuid(), RosterVector.Empty);
            var questionWithoutProgressIdentity = new Identity(Guid.NewGuid(), RosterVector.Empty);
            IQuestionnaire questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocument(children: new IComposite[]
            {
                Create.Entity.TextQuestion(questionWithoutProgressIdentity.Id),
                Create.Entity.TextQuestion(questionWithProgressIdentity.Id)
            }));

            var questionnaireId = Guid.NewGuid();
            string interviewId = "interview id";
            var liteEventRegistry = Create.Service.LiteEventRegistry();
            var statefulInterview = Create.AggregateRoot.StatefulInterview(userId: null, questionnaire: questionnaire, questionnaireId: questionnaireId);
            var statefullInterviewRepository = Create.Fake.StatefulInterviewRepositoryWith(statefulInterview);
            var questionnaireRepositoryWithOneQuestionnaire = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(questionnaireId, questionnaire);

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

            var inflatedViewModels = compositeCollectionInflationService.GetInflatedCompositeCollection(listOfViewModels);

            questionWithProgressQuestionViewModel.Answering.StartInProgressIndicator();
            
            Assert.That(inflatedViewModels, Contains.Item(questionWithProgressQuestionViewModel.Answering));
        }

        private static CompositeCollectionInflationService CreateCompositeCollectionInflationService()
            => new CompositeCollectionInflationService(Stub.MvxMainThreadDispatcher());
    }
}