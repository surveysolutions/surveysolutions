using System;
using System.Linq;
using AppDomainToolkit;
using Machine.Specifications;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Ncqrs.Spec;
using WB.Core.SharedKernels.DataCollection.Events.Interview;

namespace WB.Tests.Integration.InterviewTests.EnablementAndValidness
{
    internal class when_condition_uses_three_levels_of_nested_rosters : InterviewTestsContext
    {
        Establish context = () =>
        {
            appDomainContext = AppDomainContext.Create();
        };

        Because of = () =>
            result = Execute.InStandaloneAppDomain(appDomainContext.Domain, () =>
            {
                Setup.MockedServiceLocator();

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

                var questionnaireDocument = Create.QuestionnaireDocumentWithOneChapter(questionnaireId,
                    Create.NumericIntegerQuestion(membersQuestionId, variable: "num"),
                    Create.Roster(familyRosterId, variable: "fam",
                        rosterSizeSourceType: RosterSizeSourceType.Question, rosterSizeQuestionId: membersQuestionId,
                        children: new IComposite[]
                        {
                            Create.NumericIntegerQuestion(petsQuestionId, variable: "pet"),
                            Create.Roster(petsRosterId, variable: "frnd",
                                rosterSizeSourceType: RosterSizeSourceType.Question, rosterSizeQuestionId: petsQuestionId,
                                children: new IComposite[]
                                {
                                    Create.NumericIntegerQuestion(toysCountQuestionId, variable: "toys_count"),
                                    Create.Roster(petToysRoster, variable: "petToys",
                                        rosterSizeSourceType: RosterSizeSourceType.Question, rosterSizeQuestionId: toysCountQuestionId,
                                        children: new IComposite[]
                                        {
                                            Create.NumericIntegerQuestion(toysAgeQuestionId, variable: "toy_age")
                                        })

                                })
                        }),
                    Create.NumericIntegerQuestion(finalQuestionId, variable: "fin",
                        enablementCondition: "fam.Sum(y => y.frnd.Sum(z => z.petToys.Sum(x => x.toy_age))) > 10"));


                var interview = SetupInterview(questionnaireDocument);

                interview.AnswerNumericIntegerQuestion(userId, membersQuestionId, Empty.RosterVector, DateTime.Now, 1);
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

        Cleanup stuff = () =>
        {
            appDomainContext.Dispose();
            appDomainContext = null;
        };

        It should_enable_final_question = () =>
            result.FinalQuestionWasEnabled.ShouldBeTrue();

        static AppDomainContext<AssemblyTargetLoader, PathBasedAssemblyResolver> appDomainContext;
        static InvokeResult result;

        [Serializable]
        private class InvokeResult
        {
            public bool FinalQuestionWasEnabled { get; set; }
        }
    }
}