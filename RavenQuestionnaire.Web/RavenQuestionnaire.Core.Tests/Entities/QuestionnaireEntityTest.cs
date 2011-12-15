using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Mime;
using System.Text;
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
            questionnaire.AddQuestion("question", QuestionType.SingleOption);

            Assert.AreEqual(innerDocument.Questions[0].QuestionText, "question");
            Assert.AreEqual(innerDocument.Questions[0].QuestionType, QuestionType.SingleOption);
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
            questionnaire.AddQuestion("question", QuestionType.SingleOption);

            questionnaire.ClearQuestions();
            Assert.AreEqual(innerDocument.Questions.Count, 0);
        }

        [Test]
        public void RemoveQuestion_RemovesQuestionToDocument()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Questionnaire questionnaire = new Questionnaire(innerDocument);
            var question = questionnaire.AddQuestion("question", QuestionType.SingleOption);

            questionnaire.RemoveQuestion(question.PublicKey);
            Assert.AreEqual(innerDocument.Questions.Count, 0);
        }
        [Test]
        public void UpdateQuestion_UpdatesQuestionWithAnswersToDocument()
        {
            QuestionnaireDocument innerDocument = new QuestionnaireDocument();
            Questionnaire questionnaire = new Questionnaire(innerDocument);
            var question = questionnaire.AddQuestion("old question title", QuestionType.SingleOption);
           
            questionnaire.UpdateQuestion(question.PublicKey, "new question title", QuestionType.MultyOption,
                                         new Answer[]
                                             {

                                                 new Answer()
                                                     {
                                                         AnswerText = "answer 2",
                                                         AnswerType = AnswerType.Text,
                                                         Mandatory = false
                                                     }
                                             });

            Assert.AreEqual(innerDocument.Questions[0].QuestionText, "new question title");
            Assert.AreEqual(innerDocument.Questions[0].QuestionType, QuestionType.MultyOption);
            Assert.AreEqual(innerDocument.Questions[0].Answers.Count, 1);
        }


    }
}
