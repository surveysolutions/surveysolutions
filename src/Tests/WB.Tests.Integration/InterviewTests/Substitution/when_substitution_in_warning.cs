using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.Substitution
{
    public class when_substitution_in_warning : InterviewTestsContext
    {
        private AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;

        [SetUp]
        public void SetupTest()
        {
            appDomainContext = AppDomainContext.Create();
        }

        [Test]
        public void should_calculate_substitutions()
        {
            var results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var questionId = Id.g1;

                var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                    Create.Entity.NumericIntegerQuestion(questionId, variable: "q1",
                        validationConditions: new List<ValidationCondition>
                        {
                            Create.Entity.ValidationCondition(expression: "self > 5", message: "%q1%", severity: ValidationSeverity.Warning)
                        })
                );

                var result = new InvokeResults();

                var interview = SetupStatefullInterview(questionnaireDocument);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(Create.Command.AnswerNumericIntegerQuestionCommand(questionId: questionId, answer: 5));

                    var questionsWithChangedSubstitutions = eventContext.GetSingleEvent<SubstitutionTitlesChanged>().Questions;

                    result.WarningTextWasChanged = questionsWithChangedSubstitutions.Contains(Create.Identity(questionId));
                    result.SubstitutedWarningText = interview.GetFailedWarningMessages(Create.Identity(questionId), null).FirstOrDefault();
                }

                return result;
            });


            results.WarningTextWasChanged.Should().BeTrue();
            results.SubstitutedWarningText.Should().Be("5");
        }

        [TearDown]
        public void TearDown()
        {
            appDomainContext.Dispose();
        }

        [Serializable]
        internal class InvokeResults
        {
            public bool WarningTextWasChanged { get; set; }
            public string SubstitutedWarningText { get; set; }
        }
    }
}
