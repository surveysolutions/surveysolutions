using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.QuestionnaireEntities;
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
                        CheckResult = eventContext.AnyEvent<GroupsEnabled>(x => x.Groups.Any(q => q.Id == groupId))
                    };
                }
            });

            Assert.That(results.CheckResult);

            appDomainContext.Dispose();
        }


        [Test]
        public void when_using_IsSectionAnswered_with_Enablement_Condition()
        {
            AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext = AppDomainContext.Create();

            InvokeResults results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Guid geogrphyQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                Guid groupId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
                Guid numericQuestionId = Guid.Parse("BAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

                AssemblyContext.SetupServiceLocator();

                var groupVariable = "group1";
                QuestionnaireDocument questionnaireDocument = 
                    Create.Entity.QuestionnaireDocumentWithOneChapter(Guid.Parse("11111111111111111111111111111111"),
                    new IComposite[]
                    {
                        Create.Entity.Group(groupId, "Group X", groupVariable, null, false, new IComposite[]
                        {
                            Create.Entity.NumericIntegerQuestion(numericQuestionId,
                                variable:"num")
                        }),

                        Create.Entity.GeographyQuestion(geogrphyQuestionId,
                        variable:"geo",enablementCondition:$"IsSectionAnswered({groupVariable})"
                        )
                    });

                var interview = SetupInterviewWithExpressionStorage(questionnaireDocument);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(Create.Command.AnswerNumericIntegerQuestionCommand(interviewId: interview.EventSourceId, questionId: numericQuestionId));

                    return new InvokeResults
                    {
                        CheckResult = eventContext.AnyEvent<QuestionsEnabled>(x => x.Questions.Any(q => q.Id == geogrphyQuestionId))
                    };
                }
            });

            Assert.That(results.CheckResult);

            appDomainContext.Dispose();
        }

        [Test]
        public void when_using_IsSectionAnswered_with_Validation_Condition()
        {
            AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext = AppDomainContext.Create();

            InvokeResults results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Guid geogrphyQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                Guid groupId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
                Guid numericQuestionId = Guid.Parse("BAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

                AssemblyContext.SetupServiceLocator();

                var groupVariable = "group1";
                QuestionnaireDocument questionnaireDocument =
                    Create.Entity.QuestionnaireDocumentWithOneChapter(Guid.Parse("11111111111111111111111111111111"),
                    new IComposite[]
                    {
                        Create.Entity.Group(groupId, "Group X", groupVariable, null, false, new IComposite[]
                        {
                            Create.Entity.NumericIntegerQuestion(numericQuestionId,
                                variable:"num")
                        }),

                        Create.Entity.GeographyQuestion(geogrphyQuestionId,
                        variable:"geo",validationConditions:new List<ValidationCondition>(){new ValidationCondition($"IsSectionAnswered({groupVariable})", "error") }
                        )
                    });

                var interview = SetupInterviewWithExpressionStorage(questionnaireDocument);

                interview.AnswerAreaQuestion(Create.Command.AnswerGeographyQuestionCommand(interviewId: interview.EventSourceId, questionId: geogrphyQuestionId));

                using (var eventContext = new EventContext())
                {

                    interview.AnswerNumericIntegerQuestion(Create.Command.AnswerNumericIntegerQuestionCommand(interviewId: interview.EventSourceId, questionId: numericQuestionId));

                    return new InvokeResults
                    {
                        CheckResult = eventContext.AnyEvent<AnswersDeclaredValid>(x => x.Questions.Any(q => q.Id == geogrphyQuestionId))
                    };
                }
            });

            Assert.That(results.CheckResult);

            appDomainContext.Dispose();
        }


        [Serializable]
        public class InvokeResults
        {
            public bool CheckResult { get; set; }
        }
    }
}
