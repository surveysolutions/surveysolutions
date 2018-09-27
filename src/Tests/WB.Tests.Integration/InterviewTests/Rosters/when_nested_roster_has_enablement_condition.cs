using System;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Integration.InterviewTests.Rosters
{
    internal class when_nested_roster_has_enablement_condition : InterviewTestsContext
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
                SetUp.MockedServiceLocator();

                var questionnaireId = Guid.Parse("77778888000000000000000000000000");
                var rosterSizeQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                var ageId = Guid.Parse("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
                var genderId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
                var kidsId = Guid.Parse("DDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDD");
                var userId = Guid.NewGuid();
                var roster1Id = Guid.Parse("11111111111111111111111111111111");
                var roster2Id = Guid.Parse("22222222222222222222222222222222");
                var roster3Id = Guid.Parse("33333333333333333333333333333333");

                var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.Entity.TextListQuestion(rosterSizeQuestionId, variable: "list"),
                    Create.Entity.ListRoster(
                        rosterId: roster1Id,
                        rosterSizeQuestionId: rosterSizeQuestionId,
                        variable: "members",
                        children: new IComposite[]
                        {
                            Create.Entity.NumericIntegerQuestion(ageId, variable: "age"),
                            Create.Entity.NumericIntegerQuestion(genderId, variable: "gender")
                        }),
                    Create.Entity.ListRoster(
                        rosterId: roster2Id,
                        rosterSizeQuestionId: rosterSizeQuestionId,
                        variable: "woman",
                        enablementCondition: "gender==2 && age.InRange(15,49)",
                        children: new IComposite[]
                        {
                            Create.Entity.MultipleOptionsQuestion(kidsId,
                                linkedToRosterId: roster1Id, 
                                variable: "select_children",
                                linkedFilterExpression: "age < 5"),
                            Create.Entity.ListRoster(
                                rosterId: roster3Id,
                                rosterSizeQuestionId: rosterSizeQuestionId,
                                variable: "about_child",
                                enablementCondition: "select_children.Any(x=>x.Last()==@rowcode)")
                        })
                );

                var result = new InvokeResults();

                var interview = SetupInterview(questionnaireDocument);
                
                interview.AnswerTextListQuestion(userId, rosterSizeQuestionId, RosterVector.Empty, DateTime.Now, new Tuple<decimal, string>[]
                {
                    new Tuple<decimal, string>(1, "Mother"), 
                    new Tuple<decimal, string>(2, "Father"), 
                    new Tuple<decimal, string>(3, "Son"), 
                    new Tuple<decimal, string>(4, "Daughter"), 
                });
                interview.AnswerNumericIntegerQuestion(userId, ageId, Create.RosterVector(1), DateTime.Now, 30);
                interview.AnswerNumericIntegerQuestion(userId, ageId, Create.RosterVector(2), DateTime.Now, 40);
                interview.AnswerNumericIntegerQuestion(userId, ageId, Create.RosterVector(3), DateTime.Now, 3);
                interview.AnswerNumericIntegerQuestion(userId, ageId, Create.RosterVector(4), DateTime.Now, 2);
                interview.AnswerNumericIntegerQuestion(userId, genderId, Create.RosterVector(1), DateTime.Now, 2);
                interview.AnswerNumericIntegerQuestion(userId, ageId, Create.RosterVector(2), DateTime.Now, 1);
                interview.AnswerNumericIntegerQuestion(userId, ageId, Create.RosterVector(3), DateTime.Now, 1);
                interview.AnswerNumericIntegerQuestion(userId, ageId, Create.RosterVector(4), DateTime.Now, 2);
                

                using (var eventContext = new EventContext())
                {
                    interview.AnswerMultipleOptionsLinkedQuestion(userId, kidsId, Create.RosterVector(1), DateTime.Now,
                    new[] { Create.RosterVector(3), Create.RosterVector(4) });

                    var enabledRosters = eventContext.GetSingleEvent<GroupsEnabled>().Groups;

                    result.MotherRosterEnabled = enabledRosters.Contains(Create.Identity(roster3Id, Create.RosterVector(1, 1)));
                    result.FatherRosterEnabled = enabledRosters.Contains(Create.Identity(roster3Id, Create.RosterVector(1, 2)));
                    result.SonRosterEnabled = enabledRosters.Contains(Create.Identity(roster3Id, Create.RosterVector(1, 3)));
                    result.DaughterRosterEnabled = enabledRosters.Contains(Create.Identity(roster3Id, Create.RosterVector(1, 4)));
                }

                return result;
            });

        [NUnit.Framework.Test] public void should_not_enable_mother_roster () =>
            results.MotherRosterEnabled.Should().BeFalse();

        [NUnit.Framework.Test] public void should_not_enable_father_roster () =>
            results.FatherRosterEnabled.Should().BeFalse();

        [NUnit.Framework.Test] public void should_not_enable_son_roster () =>
           results.SonRosterEnabled.Should().BeTrue();

        [NUnit.Framework.Test] public void should_not_enable_daughter_roster () =>
           results.DaughterRosterEnabled.Should().BeTrue();

        private static InvokeResults results;

        [Serializable]
        internal class InvokeResults
        {
            public bool MotherRosterEnabled { get; set; }
            public bool FatherRosterEnabled { get; set; }
            public bool SonRosterEnabled { get; set; }
            public bool DaughterRosterEnabled { get; set; }
        }
    }
}
