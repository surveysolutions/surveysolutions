using System;
using System.Linq;

using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.V4.CustomFunctions;
using WB.Core.SharedKernels.QuestionnaireEntities;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.LanguageTests
{
    internal class when_answering_on_text_question_having_validation_containing_IRnd : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        public void BecauseOf() =>
            result = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var id = new Guid("CBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
                var userId = Guid.NewGuid();

                var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(id, children: new IComposite[]
                {
                    Create.Entity.TextQuestion(questionId: questionId, 
                        variable: "test",
                        validationExpression: "Quest.IRnd() > 2"),
                    Create.Entity.Variable(variableId, VariableType.Double, "v1", "Quest.IRnd()"),
                    Create.Entity.Variable(userId, VariableType.Double, "v2", $"(new Guid(\"{precalculatedId}\")).GetRandomDouble()")
                });
                
                using (var eventContext = new EventContext())
                {
                    var interview = SetupInterview(appDomainContext.AssemblyLoadContext, questionnaireDocument);
                    interview.AnswerTextQuestion(userId, questionId, RosterVector.Empty, DateTime.Now, "test");

                    return new InvokeResult
                    {
                        CalculatedRandom = ExtentionsV4.GetRandomDouble(interview.EventSourceId),
                        AnswerDeclaredInvalidEventCount = eventContext.Count<AnswersDeclaredInvalid>(),
                        IRndValue = (double)GetFirstEventByType<VariablesChanged>(eventContext.Events).ChangedVariables.First().NewValue,
                        PrecalculatedIRndValue = (double)GetFirstEventByType<VariablesChanged>(eventContext.Events).ChangedVariables.Skip(1).First().NewValue
                    };
                }
            });

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }
        
        [NUnit.Framework.Test] public void should_raise_AnswerDeclaredInvalidEvent_event () =>
            result.AnswerDeclaredInvalidEventCount.Should().Be(1);

        [NUnit.Framework.Test] public void should_raise_VariablesChanged_event () =>
            result.IRndValue.Should().Be(result.CalculatedRandom);

        [NUnit.Framework.Test]
        public void should_raise_VariablesChanged_event_for_Precalculated() =>
            result.PrecalculatedIRndValue.Should().Be(precalculatedRandom);

        private static AppDomainContext appDomainContext;
        private static InvokeResult result;
        private static readonly Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid variableId = Guid.Parse("21111111111111111111111111111111");

        private static readonly string precalculatedId = "13d0572ed4724d97bdf0fc09ca53402f";
        private static readonly double precalculatedRandom = 0.71404480222335309;

        [Serializable]
        private class InvokeResult
        {
            public int AnswerDeclaredInvalidEventCount { get; set; }
            public double IRndValue { get; set; }
            public double PrecalculatedIRndValue { get; set; }
            public double CalculatedRandom { get; set; }
        }
    }
}
