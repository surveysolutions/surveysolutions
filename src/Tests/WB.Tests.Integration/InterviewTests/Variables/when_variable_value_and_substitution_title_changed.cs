using System;
using FluentAssertions;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview.Dtos;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.Variables
{
    internal class when_variable_value_and_substitution_title_changed : InterviewTestsContext
    {
        [Test]
        public void should_change_item_title()
        {
            staticTextWithSubstitutionIdentity =
                Identity.Create(Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"), RosterVector.Empty);
            var substitutedVariableIdentity =
                Identity.Create(Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC"), RosterVector.Empty);

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.StaticText(publicKey: staticTextWithSubstitutionIdentity.Id,
                    text: "Your answer on question is %var1% and variable is %var2%"),
                Create.Entity.NumericIntegerQuestion(variable: "var1", id: substitutedQuestionId),
                Create.Entity.Variable(variableName: "var2", id: substitutedVariableIdentity.Id,
                    expression: "(var1*100)/20")
            );

            using (var appDomainContext = AppDomainContext.Create())
            {
                statefullInterview = SetupStatefullInterview(appDomainContext.AssemblyLoadContext, questionnaire);

                statefullInterview.AnswerNumericIntegerQuestion(interviewerId, substitutedQuestionId, RosterVector.Empty,
                    DateTime.UtcNow, 2);

                statefullInterview.GetTitleText(staticTextWithSubstitutionIdentity).Should().Be($"Your answer on question is 2 and variable is 10");
            }
        }

        static StatefulInterview statefullInterview;
        static Guid interviewerId = Guid.Parse("11111111111111111111111111111111");
        static Guid substitutedQuestionId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        static Identity staticTextWithSubstitutionIdentity;
    }
}

