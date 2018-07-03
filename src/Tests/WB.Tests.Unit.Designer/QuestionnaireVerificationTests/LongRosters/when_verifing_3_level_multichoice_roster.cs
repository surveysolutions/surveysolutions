using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using NUnit.Framework;
using WB.Core.BoundedContexts.Designer.ValueObjects;

namespace WB.Tests.Unit.Designer.QuestionnaireVerificationTests.LongRosters
{
    internal class when_verifing_3_level_multichoice_roster : QuestionnaireVerifierTestsContext
    {
        private List<QuestionnaireVerificationMessage> verificationErrors;

        [OneTimeSetUp]
        public void Setup()
        {
            Guid question1 = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            Guid question2 = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
            Guid question3 = Guid.Parse("cccccccccccccccccccccccccccccccc");

            var questionnaire = Create.QuestionnaireDocumentWithOneChapter(children: new List<IComposite>
            {
                Create.MultipleOptionsQuestion(questionId: question1),
                Create.Roster(rosterSizeQuestionId: question1, rosterType: RosterSizeSourceType.Question, children: new List<IComposite>()
                {
                    Create.MultipleOptionsQuestion(questionId: question2),
                    Create.Roster(rosterSizeQuestionId: question2, rosterType: RosterSizeSourceType.Question, children: new List<IComposite>()
                    {
                        Create.MultipleOptionsQuestion(questionId: question3),
                        Create.Roster(rosterSizeQuestionId: question3, rosterType: RosterSizeSourceType.Question)
                    })
                })
            });

            verificationErrors = CreateQuestionnaireVerifier().CheckForErrors(Create.QuestionnaireView(questionnaire)).ToList();
        }

        [Test]
        public void Should_not_produce_large_rosters_error()
        {
            this.verificationErrors.ShouldNotContainError("WB0261");
        }
    }
}