using System;
using System.Collections.Generic;
using System.Linq;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_using_IsAnswered_with_yes_no_question : in_standalone_app_domain
    {
        Because of = () =>
            results = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Guid yesNoQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                Guid groupId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");

                AssemblyContext.SetupServiceLocator();

                var yesNoQuestionVariable = "cat";
                QuestionnaireDocument questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(Guid.Parse("11111111111111111111111111111111"),
                    children: new IComposite[]
                    {
                        Abc.Create.Entity.MultyOptionsQuestion(id: yesNoQuestionId, 
                            variable:yesNoQuestionVariable,
                            yesNoView: true,
                            options: new List<Answer> {
                                Abc.Create.Entity.Answer("one", 1),
                                Abc.Create.Entity.Answer("two", 2),
                                }
                        ),
                        Abc.Create.Entity.Group(groupId, "Group X", null, $"IsAnswered({yesNoQuestionVariable})", false, null)
                    });

                var interview = SetupInterview(questionnaireDocument);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerYesNoQuestion(Create.Command.AnswerYesNoQuestion(yesNoQuestionId, RosterVector.Empty, Yes(1)));

                    return new InvokeResults
                    {
                        GroupEnabled = eventContext.AnyEvent<GroupsEnabled>(x => x.Groups.Any(q => q.Id == groupId)),
                    };
                }
            });

        It should_enable_related_group = () => results.GroupEnabled.ShouldBeTrue();

        static InvokeResults results;

        [Serializable]
        public class InvokeResults
        {
            public bool GroupEnabled { get; set; }
        }
    }
}