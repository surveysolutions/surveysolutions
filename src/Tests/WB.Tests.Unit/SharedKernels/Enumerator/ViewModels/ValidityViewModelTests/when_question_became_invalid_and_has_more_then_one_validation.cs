using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using MvvmCross.Base;
using MvvmCross.Plugin.Messenger;
using MvvmCross.Tests;
using MvvmCross.Views;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.ValidityViewModelTests
{
    public class when_question_became_invalid_and_has_more_then_one_validation : MvxIoCSupportingTest
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            base.Setup();
            
            var dispatcher = Create.Fake.MvxMainThreadDispatcher1();
            Ioc.RegisterSingleton<IMvxViewDispatcher>(dispatcher);
            Ioc.RegisterSingleton<IMvxMainThreadAsyncDispatcher>(dispatcher);
            Ioc.RegisterType<ThrottlingViewModel>(() => Create.ViewModel.ThrottlingViewModel());
            Ioc.RegisterSingleton<IMvxMessenger>(Mock.Of<IMvxMessenger>());
        }
     
        [NUnit.Framework.OneTimeSetUp] public void context () {
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextQuestion(questionId: questionIdentity.Id,
                validationConditions: new List<ValidationCondition>
                {
                    new ValidationCondition {Expression = "validation 1", Message = "message 1"},
                    new ValidationCondition {Expression = "validation 2", Message = "message 2"}
                }));
            
            var plainQuestionnaire = Create.Entity.PlainQuestionnaire(questionnaire);

            var interview = SetUp.StatefulInterview(questionnaire);
            interview.Apply(Create.Event.TextQuestionAnswered(questionIdentity.Id, questionIdentity.RosterVector, "aaa"));
            interview.Apply(answersDeclaredInvalid);

            var statefulInterviewRepository = SetUp.StatefulInterviewRepository(interview);

            viewModel = Create.ViewModel.ValidityViewModel(questionnaire: plainQuestionnaire,
                interviewRepository: statefulInterviewRepository,
                entityIdentity: questionIdentity);
            viewModel.Init("interviewid", questionIdentity, Create.Other.NavigationState());
            BecauseOf();
        }

        public void BecauseOf() => viewModel.HandleAsync(answersDeclaredInvalid);

        [NUnit.Framework.Test] public void should_show_all_failed_validation_messages () => viewModel.Error.ValidationErrors.Count.Should().Be(2);

        [NUnit.Framework.Test] public void should_show_error_messages_according_to_failed_validation_indexes_with_postfix_added () 
        {
            viewModel.Error.ValidationErrors.First()?.PlainText.Should().Be("message 2 [2]");
            viewModel.Error.ValidationErrors.Second()?.PlainText.Should().Be("message 1 [1]");
        }

        static Identity questionIdentity = Create.Entity.Identity(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), RosterVector.Empty);
        static AnswersDeclaredInvalid answersDeclaredInvalid =
            Create.Event.AnswersDeclaredInvalid(new Dictionary<Identity, IReadOnlyList<FailedValidationCondition>>
            {
                {
                    questionIdentity,
                    new List<FailedValidationCondition>
                    {
                        new FailedValidationCondition(1),
                        new FailedValidationCondition(0)
                    }
                }
            });
        static ValidityViewModel viewModel;
    }
}
