using System;
using System.Linq;
using AppDomainToolkit;
using FluentAssertions;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_condition_uses_three_levels_of_nested_rosters : InterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            appDomainContext = AppDomainContext.Create();
            BecauseOf();
        }

        public void BecauseOf() =>
            result = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                SetUp.MockedServiceLocator();

                var userId = Guid.NewGuid();

                var questionnaireId = Guid.Parse("11111111111111111111111111111111");
                var membersQuestionId = Guid.Parse("22222222222222222222222222222222");
                var textQuestionId = Guid.Parse("33333333333333333333333333333333");
                var petsQuestionId = Guid.Parse("44444444444444444444444444444444");
                var familyRosterId = Guid.Parse("55555555555555555555555555555555");
                var petsRosterId = Guid.Parse("66666666666666666666666666666666");
                var toysCountQuestionId = Guid.Parse("77777777777777777777777777777777");
                var finalQuestionId = Guid.Parse("88888888888888888888888888888888");
                var petToysRoster = Guid.Parse("99999999999999999999999999999999");
                var toysAgeQuestionId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

                var questionnaireDocument = Abc.Create.Entity.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Abc.Create.Entity.NumericIntegerQuestion(membersQuestionId, variable: "num"),
                    Abc.Create.Entity.Roster(familyRosterId, variable: "fam",
                        rosterSizeSourceType: RosterSizeSourceType.Question, rosterSizeQuestionId: membersQuestionId,
                        children: new IComposite[]
                        {
                            Abc.Create.Entity.NumericIntegerQuestion(petsQuestionId, variable: "pet"),
                            Abc.Create.Entity.Roster(petsRosterId, variable: "frnd",
                                rosterSizeSourceType: RosterSizeSourceType.Question, rosterSizeQuestionId: petsQuestionId,
                                children: new IComposite[]
                                {
                                    Abc.Create.Entity.NumericIntegerQuestion(toysCountQuestionId, variable: "toys_count"),
                                    Abc.Create.Entity.Roster(petToysRoster, variable: "petToys",
                                        rosterSizeSourceType: RosterSizeSourceType.Question, rosterSizeQuestionId: toysCountQuestionId,
                                        children: new IComposite[]
                                        {
                                            Abc.Create.Entity.NumericIntegerQuestion(toysAgeQuestionId, variable: "toy_age")
                                        })

                                })
                        }),
                    Abc.Create.Entity.NumericIntegerQuestion(finalQuestionId, variable: "fin",
                        enablementCondition: "fam.Sum(y => y.frnd.Sum(z => z.petToys.Sum(x => x.toy_age))) > 10"));


                var interview = SetupInterviewWithExpressionStorage(questionnaireDocument);

                interview.AnswerNumericIntegerQuestion(userId, membersQuestionId, RosterVector.Empty, DateTime.Now, 1);
                interview.AnswerNumericIntegerQuestion(userId, petsQuestionId, new decimal[] { 0 }, DateTime.Now, 1);
                interview.AnswerNumericIntegerQuestion(userId, toysCountQuestionId, new[] { 0m, 0m }, DateTime.Now, 1);

                using (var eventContext = new EventContext())
                {
                    interview.AnswerNumericIntegerQuestion(userId, toysAgeQuestionId, new[] { 0m, 0m, 0m }, DateTime.Now, 11);

                    return new InvokeResult
                    {
                        FinalQuestionWasEnabled = eventContext.AnyEvent<QuestionsEnabled>(x => x.Questions.Any(g => g.Id == finalQuestionId))
                    };
                }
            });

        [NUnit.Framework.OneTimeTearDown] public void CleanUp()
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        }

        [NUnit.Framework.Test] public void should_enable_final_question () =>
            result.FinalQuestionWasEnabled.Should().BeTrue();

        static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        static InvokeResult result;

        [Serializable]
        private class InvokeResult
        {
            public bool FinalQuestionWasEnabled { get; set; }
        }
    }
}
