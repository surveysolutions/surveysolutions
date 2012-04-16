using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Mime;
using System.Text;
using NCalc;
using NUnit.Framework;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Tests.Entities
{
    [TestFixture]
    public class QuestionnaireEntityTest
    {
        [Test]
        public void AddQuestion_AddsQuestionToDocument()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Questionnaire questionnaire = new Questionnaire(innerDocument);
            questionnaire.AddQuestion("question", "Exportquestion", QuestionType.SingleOption, string.Empty, "validation", Order.AsIs, null);

            Assert.AreEqual(innerDocument.Questions[0].QuestionText, "question");
            Assert.AreEqual(innerDocument.Questions[0].QuestionType, QuestionType.SingleOption);
            Assert.AreEqual(innerDocument.Questions[0].ValidationExpression, "validation");
        }
        [Test]
        public void AddGroup_Root_GroupIsAddedToDocument()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Questionnaire questionnaire = new Questionnaire(innerDocument);
            questionnaire.AddGroup("group", Propagate.None, null);

            Assert.AreEqual(innerDocument.Groups[0].Title, "group");
        }
        [Test]
        public void AddGroup_FirstLevel_GroupIsAddedToDocument()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Questionnaire questionnaire = new Questionnaire(innerDocument);
            Group parent= new Group();
            innerDocument.Groups.Add(parent);
            questionnaire.AddGroup("group", Propagate.None, parent.PublicKey);

            Assert.AreEqual((innerDocument.Groups[0] as Group).Groups[0].Title, "group");
            Assert.AreEqual(innerDocument.Groups[0], parent);
        }
        [Test]
        public void AddGroup_SubLevel_GroupIsAddedToDocument()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Questionnaire questionnaire = new Questionnaire(innerDocument);
            Group topParent = new Group();
            innerDocument.Groups.Add(topParent);
            Group subParent = new Group();
            topParent.Groups.Add(subParent);
            questionnaire.AddGroup("group", Propagate.None, subParent.PublicKey);

            Assert.AreEqual(((innerDocument.Groups[0] as Group).Groups[0] as Group).Groups[0].Title, "group");
            Assert.AreEqual((innerDocument.Groups[0] as Group).Groups[0], subParent);
        }
        [Test]
        public void AddGroup_InvalidParentPublicKey_ArgumentException()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Questionnaire questionnaire = new Questionnaire(innerDocument);
            Assert.Throws<ArgumentException>(() => questionnaire.AddGroup("group", Propagate.None, Guid.NewGuid()));
        }
        [Test]
        public void UpdateGroup_GroupIsUpdated()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Questionnaire questionnaire = new Questionnaire(innerDocument);
            Group group = new Group();
            innerDocument.Groups.Add(group);
            questionnaire.UpdateGroup("group", Propagate.None, group.PublicKey);
            Assert.AreEqual(group.Title, "group");
        }
        [Test]
        public void UpdateGroup_InvalidgroupPublicKey_ArgumentException()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Questionnaire questionnaire = new Questionnaire(innerDocument);
            Assert.Throws<ArgumentException>(() => questionnaire.UpdateGroup("group", Propagate.None, Guid.NewGuid()));
        }
        [Test]
        public void UpdateText_UpdatesTextToDocument()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            innerDocument.Title = "old title";
            Questionnaire questionnaire = new Questionnaire(innerDocument);
            questionnaire.UpdateText("new title");
            Assert.AreEqual(innerDocument.Title, "new title");
        }
        [Test]
        public void ClearQuestions_ClearsQuestionsToDocument()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Questionnaire questionnaire = new Questionnaire(innerDocument);
            questionnaire.AddQuestion("question", "stataCap", QuestionType.SingleOption, string.Empty, string.Empty, Order.AsIs, null);

            questionnaire.ClearQuestions();
            Assert.AreEqual(innerDocument.Questions.Count, 0);
        }

        [Test]
        public void RemoveQuestion_RemovesQuestionToDocument()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Questionnaire questionnaire = new Questionnaire(innerDocument);
            var question = questionnaire.AddQuestion("question", "stataCap", QuestionType.SingleOption, string.Empty, string.Empty, Order.AsIs, null);

            questionnaire.Remove<Question>(question.PublicKey);
            Assert.AreEqual(innerDocument.Questions.Count, 0);
        }
        [Test]
        public void UpdateQuestion_UpdatesQuestionWithAnswersToDocument()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Questionnaire questionnaire = new Questionnaire(innerDocument);
            var question = questionnaire.AddQuestion("old question title", "stataCap", QuestionType.SingleOption, string.Empty, string.Empty, Order.AsIs, null);

            questionnaire.UpdateQuestion(question.PublicKey, "new question title", "stataCap", QuestionType.MultyOption, string.Empty, string.Empty,
                string.Empty, Order.AsIs, new Answer[]
                                             {

                                                 new Answer()
                                                     {
                                                         AnswerText = "answer 2",
                                                         AnswerType = AnswerType.Select,
                                                         Mandatory = false
                                                     }
                                             });

            Assert.AreEqual(innerDocument.Questions[0].QuestionText, "new question title");
            Assert.AreEqual(innerDocument.Questions[0].QuestionType, QuestionType.MultyOption);
            Assert.AreEqual((innerDocument.Questions[0] as Question).Answers.Count, 1);
        }
       
        [Test]
        public void GetAllQuestions_ListOfUngroupedQuestionsIsReturned()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Questionnaire questionnaire = new Questionnaire(innerDocument);
            innerDocument.Questions.Add(new Question("top", QuestionType.SingleOption));
            innerDocument.Groups.Add(new Group("g1"));
            (innerDocument.Groups[0] as Group).Questions.Add(new Question("first level", QuestionType.MultyOption));
            Assert.AreEqual(questionnaire.GetAllQuestions().Count, 2);
        }
    }
}
