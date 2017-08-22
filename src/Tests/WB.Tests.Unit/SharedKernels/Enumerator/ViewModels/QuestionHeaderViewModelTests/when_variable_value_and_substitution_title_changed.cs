using System;
using Machine.Specifications;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;
using It = Machine.Specifications.It;

namespace WB.Tests.Unit.SharedKernels.Enumerator.ViewModels.QuestionHeaderViewModelTests
{
    internal class when_variable_value_and_substitution_title_changed : QuestionHeaderViewModelTestsContext
    {
        private Establish context = () =>
        {
            staticTextWithSubstitutionIdentity = Identity.Create(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), RosterVector.Empty);
            var substitutedVariableIdentity = Identity.Create(Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC"), RosterVector.Empty);

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.StaticText(publicKey: staticTextWithSubstitutionIdentity.Id, text: "Your answer on question is %var1% and variable is %var2%"),
                Create.Entity.NumericIntegerQuestion(variable: "var1", id: substitutedQuestionId),
                Create.Entity.Variable(variableName: "var2",id: substitutedVariableIdentity.Id, expression: "(var1*100)/20")
            );

            statefullInterview = Create.AggregateRoot.StatefulInterview(questionnaire: questionnaire, setupLevel: level =>
            {
                level.Setup(x => x.GetVariableExpression(Create.Identity(substitutedVariableIdentity.Id))).Returns(() => 10);
            });
            statefullInterview.Apply(Create.Event.VariablesChanged(new ChangedVariable(substitutedVariableIdentity, 10)));
        };

        Because of = () => 
            statefullInterview.AnswerNumericIntegerQuestion(interviewerId, substitutedQuestionId, RosterVector.Empty, DateTime.UtcNow, 2);

        It should_change_item_title = () => statefullInterview.GetTitleText(staticTextWithSubstitutionIdentity)
            .ShouldEqual($"Your answer on question is 2 and variable is 10");
        
        static StatefulInterview statefullInterview;
        static Guid interviewerId = Guid.Parse("11111111111111111111111111111111");
        static Guid substitutedQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        static Identity staticTextWithSubstitutionIdentity;
    }
}

