using System;
using System.Linq;
using Ncqrs.Spec;
using NUnit.Framework;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Tests.Abc;

namespace WB.Tests.Unit.SharedKernels.DataCollection.InterviewTests.Substitutions
{
    internal class when_removing_answer_from_text_list_question_that_is_used_as_source : InterviewTestsContext
    {
        private EventContext eventContext;
        private readonly Guid questionWithSubstitutionId = Guid.Parse("cccccccccccccccccccccccccccccccc");


        [OneTimeSetUp]
        public void Setup()
        {
            Guid listQuestionId = Guid.Parse("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            Guid linkedSingleOptionQuestionId = Guid.Parse("bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb");
            Guid userId = Guid.NewGuid();

            var questionnaire = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.TextListQuestion(listQuestionId),
                Create.Entity.SingleOptionQuestion(linkedSingleOptionQuestionId, variable: "sgl", linkedToQuestionId: listQuestionId),
                Create.Entity.TextQuestion(questionWithSubstitutionId, text: "with subst %sgl%"));

            var interview = CreateInterview(questionnaire);

            using (new EventContext())
            {
                interview.AnswerTextListQuestion(userId, listQuestionId, RosterVector.Empty, DateTime.Now, new [] { Tuple.Create(1m, "one") });
                interview.AnswerSingleOptionQuestion(userId, linkedSingleOptionQuestionId, RosterVector.Empty, DateTime.Now, 1m);
            }

            using (this.eventContext = new EventContext())
            {
                interview.AnswerTextListQuestion(userId, listQuestionId, RosterVector.Empty, DateTime.Now, new Tuple<decimal, string>[0]);
            }
        }

        [Test]
        public void should_publish_substitusions_changed_for_text_question()
        {
            this.eventContext.ShouldContainEvent<SubstitutionTitlesChanged>(x => x.Questions.Any(y => y.Id == this.questionWithSubstitutionId));
        }
    }
}