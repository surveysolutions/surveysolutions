using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;

namespace WB.Tests.Unit.Designer
{
    internal static class QuestionnaireDocumentExtensions
    {
        public static IGroup GetGroup(this QuestionnaireDocument questionnaireDocument, Guid groupId)
        {
            return questionnaireDocument.GetAllGroups().Single(group => group.PublicKey == groupId);
        }

        public static TQuestion GetQuestion<TQuestion>(this QuestionnaireDocument questionnaireDocument, Guid questionId)
            where TQuestion : class, IQuestion
        {
            return questionnaireDocument.GetEntitiesByType<TQuestion>().Single(question => question.PublicKey == questionId);
        }

        public static Group AddChapter(this QuestionnaireDocument document, Guid groupId)
        {
            var group = new Group(string.Format("Chapter {0}", groupId))
                {
                    PublicKey = groupId
                };
            document.Add(@group, null);
            return group;
        }

        public static AbstractQuestion AddQuestion(this Group @group,
            Guid questionId,
            QuestionType type = QuestionType.Text,
            List<Guid> triggers = null,
            int maxValue = 0,
            string variableName = "variableName",
            List<Answer> options = null,
            Order answerOrder = Order.AsIs,
            bool capital = false,
            bool featured = false)
        {

            AbstractQuestion question;
            switch (type)
            {
                case QuestionType.Numeric:
                    question = new NumericQuestion();
                    break;
                case QuestionType.DateTime:
                    question = new DateTimeQuestion();
                    break;
                case QuestionType.SingleOption:
                    question = new SingleQuestion();
                    break;
                case QuestionType.MultyOption:
                    question = new MultyOptionsQuestion();
                    break;
                case QuestionType.GpsCoordinates:
                    question = new GpsCoordinateQuestion();
                    break;
                default:
                    question = new TextQuestion();
                    break;
            }

            question.QuestionText = string.Format("Question {0}", questionId);

            question.PublicKey = questionId;
            question.QuestionType = type;
            question.AnswerOrder = answerOrder;
            question.Capital = capital;
            question.Featured = featured;
            question.Answers = options;

            question.Comments = "no comments";

            question.Instructions = string.Empty;
            question.StataExportCaption = variableName;
            question.ConditionExpression = string.Empty;
            question.ValidationExpression = string.Empty;
            question.ValidationMessage = string.Empty;

            @group.Insert(Int32.MaxValue, question, null);
            return question;
        }
    }
}
