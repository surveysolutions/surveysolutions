using System;
using System.Collections.Generic;
using FluentAssertions;
using Main.Core.Entities.Composite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.DataTransferObjects.Synchronization;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.Core.SharedKernels.SurveySolutions.Documents;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.Enumerator.StatefulInterviewTests
{

    internal class when_synchronizing_interview_with_fixed_rosters: StatefulInterviewTestsContext
    {
        [NUnit.Framework.OneTimeSetUp] public void context () {
            Guid questionnaireId = Guid.Parse("CCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCC");
            substitutedQuestionId = Guid.Parse("00000000000000000000000000000001");
            rosterTitle = "item1";
            var rosterId = Guid.Parse("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(id : questionnaireId,
                children: Create.Entity.FixedRoster(
                    rosterId: rosterId, 
                    fixedTitles: new[] {new FixedRosterTitle(0, rosterTitle) },
                    children: new IComposite[]
                    {
                        Create.Entity.TextQuestion(questionId: substitutedQuestionId,
                            text: "uses %rostertitle%")
                    }));

            IQuestionnaireStorage questionnaireRepository = SetUp.QuestionnaireRepositoryWithOneQuestionnaire(Create.Entity.QuestionnaireIdentity(questionnaireId), questionnaire);

            interview = Create.AggregateRoot.StatefulInterview(questionnaireId: questionnaireId, 
                questionnaireRepository: questionnaireRepository, shouldBeInitialized: false
                );

            
            syncDto = Create.Entity.InterviewSynchronizationDto(questionnaireId);
            BecauseOf();
        }

        private void BecauseOf() => interview.Synchronize(Create.Command.Synchronize(Guid.NewGuid(), syncDto));

        [NUnit.Framework.Test] public void should_recalculate_roster_titles () => interview.GetTitleText(Identity.Create(substitutedQuestionId, Create.Entity.RosterVector(0))).Should().Be($"uses {rosterTitle}");

        static StatefulInterview interview;
        static InterviewSynchronizationDto syncDto;
        static Guid substitutedQuestionId;
        static string rosterTitle;
    }
}
