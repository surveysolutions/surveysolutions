using System;
using Main.Core.Entities.Composite;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Substitutions
{
    internal class when_apply_SubstitutionTitlesChanged_for_question_from_deleted_roster_instance
    {
        [Test]
        public void when_should_not_throw_null_reference_exception()
        {
            var rosterSizeQuestionId = Guid.Parse("11111111111111111111111111111111");
            var deletedQuestionId = Guid.Parse("33333333333333333333333333333333");
            
            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(children: new IComposite[]
            {
                Create.Entity.NumericIntegerQuestion(rosterSizeQuestionId),
                Create.Entity.Roster(rosterSizeQuestionId: rosterSizeQuestionId, children: new[]
                {
                    Create.Entity.TextQuestion(deletedQuestionId)
                })
            });
            var interview = Setup.StatefulInterview(questionnaire);

            Assert.DoesNotThrow(() => interview.Apply(Create.Event.SubstitutionTitlesChanged(new[]
            {Identity.Create(deletedQuestionId, Create.Entity.RosterVector(0))})));

        }
    }
}