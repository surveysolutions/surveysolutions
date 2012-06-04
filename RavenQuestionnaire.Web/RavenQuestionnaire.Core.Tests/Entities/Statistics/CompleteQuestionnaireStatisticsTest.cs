using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Documents.Statistics;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.Statistics;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete.Question;

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
            template.Children.Add(new SingleCompleteQuestion("q1") { Children = new List<IComposite>() { new CompleteAnswer() { Selected = true } } });
            template.Children.Add(new SingleCompleteQuestion("q2"));
           CompleteQuestionnaireStatistics statistics = new CompleteQuestionnaireStatistics(template);
            var newDoc = ((IEntity<CompleteQuestionnaireStatisticDocument>)statistics).GetInnerDocument();
            Assert.AreEqual(newDoc.AnsweredQuestions.Count, 1);
        }
        [Test]
        public void Statistics_CollectInvalidQuestions_AllInvalidAreCollected()
        {
            CompleteQuestionnaireDocument template = new CompleteQuestionnaireDocument() { Id = "test", CloseDate = DateTime.Now.AddDays(1), Title = "test" };
            template.Children.Add(new SingleCompleteQuestion("q1") { Valid = false });
            template.Children.Add(new SingleCompleteQuestion("q2") { Valid = true });
            CompleteQuestionnaireStatistics statistics = new CompleteQuestionnaireStatistics(template);
            var newDoc = ((IEntity<CompleteQuestionnaireStatisticDocument>)statistics).GetInnerDocument();
            Assert.AreEqual(newDoc.InvalidQuestions.Count, 1);
        }
        [Test]
        public void Statistics_CollectFeaturedQuestions_AllInvalidAreCollected()
        {
            CompleteQuestionnaireDocument template = new CompleteQuestionnaireDocument() { Id = "test", CloseDate = DateTime.Now.AddDays(1), Title = "test" };
            template.Children.Add(new SingleCompleteQuestion("q1") { Featured = false });
            template.Children.Add(new SingleCompleteQuestion("q2") { Featured = true });
            CompleteQuestionnaireStatistics statistics = new CompleteQuestionnaireStatistics(template);
            var newDoc = ((IEntity<CompleteQuestionnaireStatisticDocument>)statistics).GetInnerDocument();
            Assert.AreEqual(newDoc.FeturedQuestions.Count, 1);
        }
    }
}
