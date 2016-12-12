using System;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.Enumerator.Implementation.Aggregates;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.QuestionHeaderViewModelTests
{
    internal class when_variable_value_and_substitution_title_changed : QuestionHeaderViewModelTestsContext
    {
        Establish context = () =>
        {
            staticTextWithSubstitutionIdentity = Identity.Create(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), RosterVector.Empty);
            var substitutedVariableIdentity = Identity.Create(Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC"), RosterVector.Empty);

            var substitutedVariable1Name = "var1";
            var substitutedVariable2Name = "var2";

            var questionnaireRepository = Setup.QuestionnaireRepositoryWithOneQuestionnaire(new QuestionnaireIdentity(Guid.NewGuid(), 1),
                    Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
                    {
                        Create.Entity.StaticText(publicKey: staticTextWithSubstitutionIdentity.Id,
                            text: $"Your answer on question is %{substitutedVariable1Name}% and variable is %{substitutedVariable2Name}%"),
                        Create.Entity.NumericIntegerQuestion(variable: substitutedVariable1Name, id: substitutedQuestionId),
                        Create.Entity.Variable(variableName: substitutedVariable2Name,
                            id: substitutedVariableIdentity.Id, expression: $"({substitutedVariable1Name}*100)/20")
                    }));

            statefullInterview = Create.AggregateRoot.StatefulInterview(questionnaireRepository: questionnaireRepository);
            statefullInterview.Apply(Create.Event.VariablesChanged(new ChangedVariable(substitutedVariableIdentity, 10)));
        };

        Because of = () => statefullInterview.AnswerNumericIntegerQuestion(interviewerId, substitutedQuestionId, RosterVector.Empty, DateTime.UtcNow, answerOnIntegerQuestion);

        It should_change_item_title = () => statefullInterview.GetTitleText(staticTextWithSubstitutionIdentity)
                    .ShouldEqual($"Your answer on question is {answerOnIntegerQuestion} and variable is {(answerOnIntegerQuestion*100)/20}");
        
        static StatefulInterview statefullInterview;
        static int answerOnIntegerQuestion = 2;
        static Guid interviewerId = Guid.Parse("11111111111111111111111111111111");
        static Guid substitutedQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        static Identity staticTextWithSubstitutionIdentity;
    }
}

