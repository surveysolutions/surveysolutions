using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Documents.Statistics;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.Statistics;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Tests.Entities.Statistics
{
    [TestFixture]
    public class CompleteQuestionnaireStatisticsTest
    {
        [Test]
        public void Statistics_GenerateStatisticFromEmptyTemplate_IdTitleAndDatesAreAdded()
        {
            CompleteQuestionnaireDocument template=new CompleteQuestionnaireDocument(){ Id = "test", CloseDate = DateTime.Now.AddDays(1), Title = "test"};
            CompleteQuestionnaireStatistics statistics = new CompleteQuestionnaireStatistics(template);
            var newDoc = ((IEntity<CompleteQuestionnaireStatisticDocument>)statistics).GetInnerDocument();
            Assert.AreEqual(template.Id,newDoc.CompleteQuestionnaireId);
            Assert.AreEqual(template.CreationDate, newDoc.StartDate);
            Assert.AreEqual(template.CloseDate, newDoc.EndDate);
            Assert.AreEqual(template.Title, newDoc.Title);
        }
        [Test]
        public void Statistics_CollectAnsweredQuestions_AllAnswersAreCollected()
        {
            CompleteQuestionnaireDocument template = new CompleteQuestionnaireDocument() { Id = "test", CloseDate = DateTime.Now.AddDays(1), Title = "test" };
            template.Questions.Add(new CompleteQuestion("q1", QuestionType.Text)
                                       {Answers = new List<ICompleteAnswer>() {new CompleteAnswer() {Selected = true}}});
           template.Questions.Add(new CompleteQuestion("q2", QuestionType.Text));
           CompleteQuestionnaireStatistics statistics = new CompleteQuestionnaireStatistics(template);
            var newDoc = ((IEntity<CompleteQuestionnaireStatisticDocument>)statistics).GetInnerDocument();
            Assert.AreEqual(newDoc.AnsweredQuestions.Count, 1);
        }
        [Test]
        public void Statistics_CollectInvalidQuestions_AllInvalidAreCollected()
        {
            CompleteQuestionnaireDocument template = new CompleteQuestionnaireDocument() { Id = "test", CloseDate = DateTime.Now.AddDays(1), Title = "test" };
            template.Questions.Add(new CompleteQuestion("q1", QuestionType.Text) { Valid = false});
            template.Questions.Add(new CompleteQuestion("q2", QuestionType.Text) { Valid = true});
            CompleteQuestionnaireStatistics statistics = new CompleteQuestionnaireStatistics(template);
            var newDoc = ((IEntity<CompleteQuestionnaireStatisticDocument>)statistics).GetInnerDocument();
            Assert.AreEqual(newDoc.InvalidQuestions.Count, 1);
        }
        [Test]
        public void Statistics_CollectFeaturedQuestions_AllInvalidAreCollected()
        {
            CompleteQuestionnaireDocument template = new CompleteQuestionnaireDocument() { Id = "test", CloseDate = DateTime.Now.AddDays(1), Title = "test" };
            template.Questions.Add(new CompleteQuestion("q1", QuestionType.Text) { Featured = false });
            template.Questions.Add(new CompleteQuestion("q2", QuestionType.Text) { Featured = true });
            CompleteQuestionnaireStatistics statistics = new CompleteQuestionnaireStatistics(template);
            var newDoc = ((IEntity<CompleteQuestionnaireStatisticDocument>)statistics).GetInnerDocument();
            Assert.AreEqual(newDoc.FeturedQuestions.Count, 1);
        }
    }
}
