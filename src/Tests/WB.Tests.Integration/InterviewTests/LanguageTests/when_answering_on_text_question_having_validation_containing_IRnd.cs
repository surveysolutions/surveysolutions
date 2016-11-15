using System;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Tests.Integration.InterviewTests.LanguageTests
{
    internal class when_answering_on_text_question_having_validation_containing_IRnd : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        private Because of = () =>
            result = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

                var id = new Guid("CBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");

                var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(id, children: new IComposite[]
                {
                    Create.TextQuestion(variable: "test", id: questionId, validationExpression: "Quest.IRnd() > 2"),
                    Create.Variable(id: variableId, type: VariableType.Double, variableName: "v1", expression: "Quest.IRnd()")
                });

                var userId = Guid.NewGuid();
                ILatestInterviewExpressionState interviewState = GetInterviewExpressionState(questionnaireDocument);
                
                using (var eventContext = new EventContext())
                {
                    var interview = SetupInterview(questionnaireDocument, precompiledState: interviewState);
                    interview.AnswerTextQuestion(userId, questionId, Empty.RosterVector, DateTime.Now, "test");

                    return new InvokeResult
                    {
                        CalculatedRandom = new Random(interview.EventSourceId.GetHashCode()).NextDouble(),
                        AnswerDeclaredInvalidEventCount = eventContext.Count<AnswersDeclaredInvalid>(),
                        IRndValue = (double)GetFirstEventByType<VariablesChanged>(eventContext.Events).ChangedVariables.First().NewValue
                    };
                }
            });

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };
        
        It should_raise_AnswerDeclaredInvalidEvent_event = () =>
            result.AnswerDeclaredInvalidEventCount.ShouldEqual(1);

        It should_raise_VariablesChanged_event = () =>
            result.IRndValue.ShouldEqual(result.CalculatedRandom);

        private static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        private static InvokeResult result;
        private static readonly Guid questionId = Guid.Parse("11111111111111111111111111111111");
        private static readonly Guid variableId = Guid.Parse("21111111111111111111111111111111");
        
        [Serializable]
        private class InvokeResult
        {
            public int AnswerDeclaredInvalidEventCount { get; set; }
            public double IRndValue { get; set; }
            public double CalculatedRandom { get; set; }
        }
    }
}