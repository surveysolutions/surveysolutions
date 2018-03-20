using System;
using System.Collections.Generic;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_using_IsAnswered_with_yes_no_question : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        protected static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;

        public void BecauseOf() =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Guid yesNoQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                Guid groupId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

                AssemblyContext.SetupServiceLocator();

                var yesNoQuestionVariable = "cat";
                QuestionnaireDocument questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(Guid.Parse("11111111111111111111111111111111"), 
                    new IComposite[]
                    {
                        Create.Entity.MultyOptionsQuestion(yesNoQuestionId, 
                            variable:yesNoQuestionVariable,
                            yesNoView: true,
                            options: new List<Answer> {
                                Create.Entity.Answer("one", 1),
                                Create.Entity.Answer("two", 2)
                                }
                        ),
                        Create.Entity.Group(groupId, "Group X", null, $"IsAnswered({yesNoQuestionVariable})", false, null)
                    });

                var interview = SetupInterviewWithExpressionStorage(questionnaireDocument);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerYesNoQuestion(Create.Command.AnswerYesNoQuestion(questionId: yesNoQuestionId, answeredOptions: new[] { Yes(1) }));

                    return new InvokeResults
                    {
                        GroupEnabled = eventContext.AnyEvent<GroupsEnabled>(x => x.Groups.Any(q => q.Id == groupId))
                    };
                }
            });

        [NUnit.Framework.Test] public void should_enable_related_group () => results.GroupEnabled.Should().BeTrue();

        static InvokeResults results;

        [Serializable]
        public class InvokeResults
        {
            public bool GroupEnabled { get; set; }
        }
    }
}
