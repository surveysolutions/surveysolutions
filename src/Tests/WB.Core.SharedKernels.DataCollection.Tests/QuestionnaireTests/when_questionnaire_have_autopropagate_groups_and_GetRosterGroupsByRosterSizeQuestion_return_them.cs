using System;
using System.Collections.Generic;
using Machine.Specifications;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using It = Machine.Specifications.It;

namespace WB.Core.SharedKernels.DataCollection.Tests.QuestionnaireTests
{
    internal class when_questionnaire_have_autopropagate_groups_and_GetRosterGroupsByRosterSizeQuestion_return_them
        : QuestionnaireTestsContext
    {
        Establish context = () =>
        {
            var rosterSizeQuestionId = new Guid("CBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            var rosterGroupId = new Guid("EBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
            IQuestionnaireDocument questionnaireDocument = CreateQuestionnaireDocumentWithOneChapter(new IComposite[]
            {
                new NumericQuestion() { PublicKey = rosterSizeQuestionId, IsInteger = true, MaxValue = 4 },
                new AutoPropagateQuestion()
                {
                    PublicKey = validatedQuestionId,
                    QuestionType = QuestionType.AutoPropagate,
                    Triggers = new List<Guid>() { autopropagateGroupId }
                },
                new Group() { PublicKey = autopropagateGroupId, Propagated = Propagate.AutoPropagated },
                new Group() { PublicKey = rosterGroupId, IsRoster = true, RosterSizeQuestionId = rosterSizeQuestionId },
            });

            questionnaire = CreateQuestionnaire(Guid.NewGuid(), questionnaireDocument);
        };

        private Because of = () =>
            rosterGroups = questionnaire.GetRosterGroupsByRosterSizeQuestion(validatedQuestionId);

        It should_rosterGroups_not_be_empty = () =>
            rosterGroups.ShouldNotBeEmpty();

        It should_rosterGroups_have_only_1_roster_group = () =>
            rosterGroups.ShouldContainOnly(autopropagateGroupId);

        private static IEnumerable<Guid> rosterGroups;
        private static Questionnaire questionnaire;
        private static Guid validatedQuestionId = new Guid("BBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
        private static Guid autopropagateGroupId = new Guid("ABBBBBBBBBBBBBBBBBBBBBBBBBBBBBBB");
    }
}