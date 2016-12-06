using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.StaticTextViewModelTests
{
    internal class when_initializing_and_static_text_is_invalid : StaticTextViewModelTestsContext
    {
        Establish context = () =>
        {
            var questionnaire = Create.Entity.PlainQuestionnaire(Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[] {
                Create.Entity.NumericIntegerQuestion(numericId, variable: "n"),
                Create.Entity.StaticText(staticTextId, validationConditions: new List<ValidationCondition> { Create.Entity.ValidationCondition("n == 2")})
            }));

            var questionnaireRepository = Create.Fake.QuestionnaireRepositoryWithOneQuestionnaire(Guid.NewGuid(), questionnaire);

            var statefulInterview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire);
            statefulInterview.AnswerNumericIntegerQuestion(Guid.NewGuid(), numericId, RosterVector.Empty, DateTime.Now, 3);
            statefulInterview.Apply(Create.Event.StaticTextsDeclaredInvalid(Create.Entity.Identity(staticTextId, RosterVector.Empty)));

            var interviewRepository = Create.Fake.StatefulInterviewRepositoryWith(statefulInterview);

            viewModel = CreateViewModel(questionnaireRepository, interviewRepository);
        };

        Because of = () =>
            viewModel.Init("interview", new Identity(staticTextId, RosterVector.Empty), null);

        It should_mark_static_text_view_model_as_invalid = () =>
            viewModel.QuestionState.Validity.IsInvalid.ShouldBeTrue();

        static StaticTextViewModel viewModel;
        static Guid numericId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        static Guid staticTextId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
    }
}