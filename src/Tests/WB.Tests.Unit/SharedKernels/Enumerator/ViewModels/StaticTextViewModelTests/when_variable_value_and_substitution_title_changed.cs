using System;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.StaticTextViewModelTests
{
    [Ignore("KP-8159")]
    internal class when_variable_value_and_substitution_title_changed : StaticTextViewModelTestsContext
    {
        Establish context = () =>
        {
            var interviewId = "interviewId";
            var staticTextWithSubstitutionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
            var substitutedVariable1Name = "var1";
            var substitutedVariable2Name = "var2";
            
            var substitutedVariableIdentity = new Identity(Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC"), RosterVector.Empty);
            
            var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(new QuestionnaireIdentity(Guid.NewGuid(), 1),
                    Create.Entity.QuestionnaireDocument(children: new IComposite[]
                    {
                        Create.Entity.StaticText(publicKey: staticTextWithSubstitutionId,
                            text: $"Your answer on question is %{substitutedVariable1Name}% and variable is %{substitutedVariable2Name}%"),
                        Create.Entity.NumericRealQuestion(variable: substitutedVariable1Name, id: substitutedQuestionId),
                        Create.Entity.Variable(variableName: substitutedVariable2Name,
                            id: substitutedVariableIdentity.Id, expression: $"({substitutedVariable1Name}*100)/20")
                    }));

            var interviewRepository = Setup.StatefulInterviewRepository(
                    statefullInterview = Create.AggregateRoot.StatefulInterview(questionnaireRepository: questionnaireRepository));
            
            viewModel = CreateViewModel(questionnaireRepository, interviewRepository, Create.Service.LiteEventRegistry());

            Identity id = new Identity(staticTextWithSubstitutionId, Empty.RosterVector);
            viewModel.Init(interviewId, id, null);
        };

        Because of = () => statefullInterview.AnswerNumericIntegerQuestion(interviewerId, substitutedQuestionId, RosterVector.Empty, DateTime.UtcNow, answerOnIntegerQuestion);

        It should_change_item_title = () => viewModel.Text.PlainText.ShouldEqual($"Your answer on question is {answerOnIntegerQuestion} and variable is {(answerOnIntegerQuestion * 100)/20}");

        static StaticTextViewModel viewModel;
        static StatefulInterview statefullInterview;
        static int answerOnIntegerQuestion = 2;
        static Guid interviewerId = Guid.Parse("11111111111111111111111111111111");
        static Guid substitutedQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
    }
}

