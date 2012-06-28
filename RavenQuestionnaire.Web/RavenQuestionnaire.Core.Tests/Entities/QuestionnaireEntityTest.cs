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
using RavenQuestionnaire.Core.Entities.SubEntities.Question;

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

            questionnaire.AddQuestion(Guid.NewGuid(),"question", "Exportquestion", QuestionType.SingleOption, string.Empty, "validation", false, Order.AsIs, null, null);

            Assert.AreEqual(((IQuestion)innerDocument.Children[0]).QuestionText, "question");
            Assert.AreEqual(((IQuestion)innerDocument.Children[0]).QuestionType, QuestionType.SingleOption);
            Assert.AreEqual(((IQuestion)innerDocument.Children[0]).ValidationExpression, "validation");
        }
        [Test]
        public void AddGroup_Root_GroupIsAddedToDocument()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Questionnaire questionnaire = new Questionnaire(innerDocument);
            Guid newItemKey = Guid.NewGuid();
            questionnaire.AddGroup("group", newItemKey, Propagate.None, null);

            Assert.AreEqual(((IGroup)innerDocument.Children[0]).Title, "group");
        }
        [Test]
        public void AddGroup_FirstLevel_GroupIsAddedToDocument()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Questionnaire questionnaire = new Questionnaire(innerDocument);
            Group parent= new Group();
            innerDocument.Children.Add(parent);
            questionnaire.AddGroup("group", Guid.NewGuid(), Propagate.None, parent.PublicKey);

            Assert.AreEqual(((Group)(innerDocument.Children[0] as Group).Children[0]).Title, "group");
            Assert.AreEqual(innerDocument.Children[0], parent);
        }
        [Test]
        public void AddGroup_SubLevel_GroupIsAddedToDocument()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Questionnaire questionnaire = new Questionnaire(innerDocument);
            Group topParent = new Group();
            innerDocument.Children.Add(topParent);
            Group subParent = new Group();
            topParent.Children.Add(subParent);
            questionnaire.AddGroup("group", Guid.NewGuid(), Propagate.None, subParent.PublicKey);

            Assert.AreEqual(((Group)((innerDocument.Children[0] as Group).Children[0] as Group).Children[0]).Title, "group");
            Assert.AreEqual((innerDocument.Children[0] as Group).Children[0], subParent);
        }
        [Test]
        public void AddGroup_InvalidParentPublicKey_ArgumentException()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Questionnaire questionnaire = new Questionnaire(innerDocument);
            Assert.Throws<ArgumentException>(() => questionnaire.AddGroup("group", Guid.NewGuid(), Propagate.None, Guid.NewGuid()));
        }
        [Test]
        public void UpdateGroup_GroupIsUpdated()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Questionnaire questionnaire = new Questionnaire(innerDocument);
            Group group = new Group();
            innerDocument.Children.Add(group);
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
            questionnaire.AddQuestion(Guid.NewGuid(), "question", "stataCap", QuestionType.SingleOption, string.Empty, string.Empty, false, Order.AsIs, null, null);

            questionnaire.ClearQuestions();
            Assert.AreEqual(innerDocument.Children.Count, 0);
        }

        [Test]
        public void RemoveQuestion_RemovesQuestionToDocument()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Questionnaire questionnaire = new Questionnaire(innerDocument);
            var question = questionnaire.AddQuestion(Guid.NewGuid(), "question", "stataCap", QuestionType.SingleOption, string.Empty, string.Empty, false, Order.AsIs, null, null);

            questionnaire.Remove(question.PublicKey);
            Assert.AreEqual(innerDocument.Children.Count, 0);
        }
        [Test]
        public void UpdateQuestion_UpdatesQuestionWithAnswersToDocument()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Questionnaire questionnaire = new Questionnaire(innerDocument);
            var question = questionnaire.AddQuestion(Guid.NewGuid(), "old question title", "stataCap", QuestionType.SingleOption, string.Empty, string.Empty, false, Order.AsIs, null, null);

            questionnaire.UpdateQuestion(question.PublicKey, "new question title", "stataCap", QuestionType.MultyOption, string.Empty, string.Empty,
                string.Empty, false, Order.AsIs, new Answer[]
                                             {

                                                 new Answer()
                                                     {
                                                         AnswerText = "answer 2",
                                                         AnswerType = AnswerType.Select,
                                                         Mandatory = false
                                                     }
                                             });

            Assert.AreEqual(((IQuestion)innerDocument.Children[0]).QuestionText, "new question title");
            Assert.AreEqual(((IQuestion)innerDocument.Children[0]).QuestionType, QuestionType.MultyOption);
            Assert.AreEqual((innerDocument.Children[0] as AbstractQuestion).Children.Count, 1);
        }
       
        [Test]
        public void GetAllQuestions_ListOfUngroupedQuestionsIsReturned()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Questionnaire questionnaire = new Questionnaire(innerDocument);
            innerDocument.Children.Add(new SingleQuestion(Guid.NewGuid(), "top"));
            var group = new Group("g1");
            innerDocument.Children.Add(group);
            group.Children.Add(new SingleQuestion(Guid.NewGuid(), "first level"));
            Assert.AreEqual(questionnaire.GetAllQuestions().Count, 2);
        }


        [Test]
        public void InserQuestionAfter_QuestionNOtExists_ArgumentException()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Questionnaire questionnaire = new Questionnaire(innerDocument);
            Assert.Throws<ArgumentException>(() => questionnaire.MoveItem(Guid.NewGuid(), null, null));
        }
        [Test]
        public void InserQuestionAfter_InserAfterNOtExists_ArgumentException()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Questionnaire questionnaire = new Questionnaire(innerDocument);
            var question = new SingleQuestion(Guid.NewGuid(), "top");
            innerDocument.Children.Add(question);
            Assert.Throws<ArgumentException>(() => questionnaire.MoveItem(question.PublicKey, Guid.NewGuid(), Guid.NewGuid()));
        }

        [Test]
        public void InserQuestionAfter_InserAfterIsNull_InsertedInFirstPlace()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Questionnaire questionnaire = new Questionnaire(innerDocument);
            var question1 = new SingleQuestion(Guid.NewGuid(), "top");
            innerDocument.Children.Add(question1);
            var question2 = new SingleQuestion(Guid.NewGuid(), "sub");
            innerDocument.Children.Add(question2);
            questionnaire.MoveItem( question2.PublicKey, null, null);
            Assert.AreEqual(innerDocument.Children.Count, 2);
            Assert.AreEqual(innerDocument.Children[0], question2);
            Assert.AreEqual(innerDocument.Children[1], question1);
        }
        [Test]
        public void InserQuestionAfter_InserAfterIsGuid_InsertedAfterItemWithGuid()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Questionnaire questionnaire = new Questionnaire(innerDocument);
            var question1 = new SingleQuestion(Guid.NewGuid(),"top");
            innerDocument.Children.Add(question1);
            var question2 = new SingleQuestion(Guid.NewGuid(), "sub");
            innerDocument.Children.Add(question2);
            var question3 = new SingleQuestion(Guid.NewGuid(), "third");
            innerDocument.Children.Add(question3);
            questionnaire.MoveItem(question3.PublicKey, null, question1.PublicKey);
            Assert.AreEqual(innerDocument.Children.Count, 3);
            Assert.AreEqual(innerDocument.Children[0], question1);
            Assert.AreEqual(innerDocument.Children[1], question3);
            Assert.AreEqual(innerDocument.Children[2], question2);
        }

        [Test]
        public void InserQuestionAfter_InserAfterLastItemWithGuid_InsertedAfterItemWLastItem()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Questionnaire questionnaire = new Questionnaire(innerDocument);
            var question1 = new SingleQuestion(Guid.NewGuid(), "top");
            innerDocument.Children.Add(question1);
            var question2 = new SingleQuestion(Guid.NewGuid(), "sub");
            innerDocument.Children.Add(question2);
            var question3 = new SingleQuestion(Guid.NewGuid(), "third");
            innerDocument.Children.Add(question3);
            questionnaire.MoveItem(question1.PublicKey, null, question3.PublicKey);
            Assert.AreEqual(innerDocument.Children.Count, 3);
            Assert.AreEqual(innerDocument.Children[0], question2);
            Assert.AreEqual(innerDocument.Children[1], question3);
            Assert.AreEqual(innerDocument.Children[2], question1);
        }
    }
}
