using NUnit.Framework;
using System.Collections.Generic;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities.Answers;
using WB.Tests.Abc;

using WB.UI.Headquarters.API.WebInterview;
using WB.UI.Headquarters.API.WebInterview.Services;
using WB.UI.Headquarters.Services.Impl;

namespace WB.Tests.Unit.Applications.Headquarters.WebInterview.Review.Filtering
{
    public class when_interview_has_prefilled_readonly_question
    {
        [Test]
        [TestOf(typeof(StatefulInterviewSearcher))]
        public void should_not_include_it_in_ForInterviewer_filter()
        {
            var interviewerQuestionId = Id.g1;
            var prefilledReadonlyQuestionId = Id.g2;

            var questionnaireDocument = Create.Entity.QuestionnaireDocumentWithOneChapter(
                Create.Entity.NumericIntegerQuestion(interviewerQuestionId, variable: "interviwer"),
                Create.Entity.NumericIntegerQuestion(prefilledReadonlyQuestionId, variable: "prefilled", isPrefilled: true));
            var questionnaire = Create.Entity.PlainQuestionnaire(questionnaireDocument);
            var interview = Create.AggregateRoot.StatefulInterview(interviewId: Id.gA,
                questionnaire: questionnaireDocument,
                shouldBeInitialized: false);

            interview.CreateInterview(Create.Command.CreateInterview(
                assignmentId: 5,
                interviewId: Id.gA, answers: new List<InterviewAnswer>()
                {
                    Create.Entity.InterviewAnswer(Create.Identity(prefilledReadonlyQuestionId), Create.Entity.NumericIntegerAnswer(5))
                }));

            var searcher = Web.Create.Service.StatefullInterviewSearcher();

            // Act
            SearchResults searchResult = searcher.Search(interview, questionnaire, new[] {FilterOption.ForInterviewer}, 0, 10);
           
            // Assert
            Assert.That(searchResult, Has.Property(nameof(searchResult.TotalCount)).EqualTo(1));
            Assert.That(searchResult.Stats[FilterOption.ForInterviewer], Is.EqualTo(1));

            var foundQuestions = searchResult.Results[0].Questions;
            Assert.That(foundQuestions, Has.Count.EqualTo(1));
            Assert.That(foundQuestions[0].Target, Is.EqualTo(interviewerQuestionId.FormatGuid()));
        }
    }
}
