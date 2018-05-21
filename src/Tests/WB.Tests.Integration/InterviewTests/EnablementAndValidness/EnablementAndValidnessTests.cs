using System;
using System.Linq;
using AppDomainToolkit;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class EnablementAndValidnessTests : InterviewTestsContext
    {
        [Test]
        public void when_using_IsAnswered_with_Geography_question()
        {
            AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext = AppDomainContext.Create();

            InvokeResults results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Guid geogrphyQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                Guid groupId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

                AssemblyContext.SetupServiceLocator();

                var geogrphyQuestionVariable = "geo";
                QuestionnaireDocument questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(Guid.Parse("11111111111111111111111111111111"),
                    new IComposite[]
                    {
                        Create.Entity.GeographyQuestion(geogrphyQuestionId,
                            variable:geogrphyQuestionVariable
                        ),
                        Create.Entity.Group(groupId, "Group X", null, $"IsAnswered({geogrphyQuestionVariable})", false, null)
                    });

                var interview = SetupInterviewWithExpressionStorage(questionnaireDocument);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerAreaQuestion(Create.Command.AnswerGeographyQuestionCommand(interviewId: interview.EventSourceId, questionId: geogrphyQuestionId));

                    return new InvokeResults
                    {
                        GroupEnabled = eventContext.AnyEvent<GroupsEnabled>(x => x.Groups.Any(q => q.Id == groupId))
                    };
                }
            });

            Assert.That(results.GroupEnabled);

            appDomainContext.Dispose();
        }

        [Serializable]
        public class InvokeResults
        {
            public bool GroupEnabled { get; set; }
        }
    }
}
