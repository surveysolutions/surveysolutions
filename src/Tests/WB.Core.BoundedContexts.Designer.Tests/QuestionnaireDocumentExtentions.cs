using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;

namespace WB.Core.BoundedContexts.Designer.Tests
{
    public static class QuestionnaireDocumentExtentions
    {
        public static Group AddChapter(this QuestionnaireDocument document, Guid groupId)
        {
            var group = new Group(string.Format("Chapter {0}", groupId))
                {
                    PublicKey = groupId
                };
            document.Children.Add(@group);
            return group;
        }

        public static Group AddGroup(this Group document, Guid groupId, Propagate propagationKind = Propagate.None)
        {
            var group = new Group(string.Format("Group {0}", groupId))
            {
                PublicKey = groupId,
                Propagated = propagationKind
            };
            document.Children.Add(@group);
            return group;
        }

        public static AbstractQuestion AddQuestion(this Group @group,
            Guid questionId,
            QuestionType type = QuestionType.Text,
            List<Guid> triggers = null,
            int maxValue = 0,
            string variableName = "variableName",
            List<IAnswer> options = null,
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
                case QuestionType.AutoPropagate:
                    question = new AutoPropagateQuestion();
                    (question as AutoPropagateQuestion).Triggers = triggers;
                    (question as AutoPropagateQuestion).MaxValue = maxValue;
                    break;
                case QuestionType.SingleOption:
                    question = new SingleQuestion();
                    break;
                case QuestionType.MultyOption:
                    question = new MultyOptionsQuestion();
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

            @group.Children.Add(question);
            return question;
        }
    }
}
