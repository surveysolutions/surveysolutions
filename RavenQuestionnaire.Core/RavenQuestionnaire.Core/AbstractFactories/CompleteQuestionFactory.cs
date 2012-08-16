using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Utility.OrderStrategy;
using RavenQuestionnaire.Core.Entities.SubEntities.Question;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete.Question;

namespace RavenQuestionnaire.Core.AbstractFactories
{
    public class CompleteQuestionFactory : ICompleteQuestionFactory
    {
        #region Implementation of ICompleteQuestionFactory

        public  AbstractQuestion Create(QuestionType type)
        {
            switch (type)
            {
                case QuestionType.MultyOption:
                    return new MultyOptionsQuestion();
                case QuestionType.DropDownList:
                    return new SingleQuestion();
                case QuestionType.SingleOption:
                    return new SingleQuestion();
                case QuestionType.YesNo:
                    return new SingleQuestion();
                case QuestionType.Text:
                    return new TextQuestion();
                case QuestionType.DateTime:
                    return new DateTimeQuestion();
                case QuestionType.Numeric:
                    return new NumericQuestion();
                case QuestionType.AutoPropagate:
                    return new AutoPropagateQuestion();
                case QuestionType.GpsCoordinates:
                    return new GpsCoordinateQuestion();
            }
            return new TextQuestion();
        }


        public CompleteQuestionView CreateQuestion(CompleteQuestionnaireDocument doc, ICompleteQuestion question)
        {
            return new CompleteQuestionView(doc, question);
        }

        public ICompleteQuestion ConvertToCompleteQuestion(IQuestion question)
        {
            var bindedQuestion = question as BindedQuestion;
            if (bindedQuestion != null)
                return (BindedCompleteQuestion) bindedQuestion;
            if (question is IBinded)
                return new BindedCompleteQuestion(question.PublicKey, (IBinded)question);
            AbstractCompleteQuestion completeQuestion;
            if (question is IMultyOptionsQuestion)
                completeQuestion = new MultyOptionsCompleteQuestion();
            else if (question is ISingleQuestion)
                completeQuestion = new SingleCompleteQuestion();
            else if (question is IDateTimeQuestion)
                completeQuestion = new DateTimeCompleteQuestion();
            else if (question is INumericQuestion)
                completeQuestion = new NumericCompleteQuestion();
            else if (question is IAutoPropagate)
                completeQuestion = new AutoPropagateCompleteQuestion(question as IAutoPropagate);
            else if (question is IGpsCoordinatesQuestion)
                completeQuestion = new GpsCoordinateCompleteQuestion();
            else completeQuestion = new TextCompleteQuestion();
            completeQuestion.PublicKey = question.PublicKey;
            completeQuestion.ConditionExpression = question.ConditionExpression;
            completeQuestion.QuestionText = question.QuestionText;
            completeQuestion.QuestionType = question.QuestionType;
            completeQuestion.StataExportCaption = question.StataExportCaption;
            completeQuestion.Instructions = question.Instructions;
            completeQuestion.Comments = question.Comments;
            completeQuestion.Triggers = question.Triggers;
            completeQuestion.ValidationExpression = question.ValidationExpression;
            completeQuestion.ValidationMessage = question.ValidationMessage;
            completeQuestion.AnswerOrder = question.AnswerOrder;
            completeQuestion.Valid = true;
            completeQuestion.Featured = question.Featured;
            completeQuestion.Mandatory = question.Mandatory;

            var ansersToCopy =
                new OrderStrategyFactory().Get(completeQuestion.AnswerOrder).Reorder(question.Children);
            if (ansersToCopy != null)
            {
                foreach (IAnswer composite in ansersToCopy)
                {
                    IComposite newAnswer;
                    if (question is ICompleteQuestion)
                    {
                        newAnswer = new CompleteAnswer(composite as CompleteAnswer, ((ICompleteQuestion)question).PropogationPublicKey);
                    }
                    else
                    {
                        newAnswer = (CompleteAnswer) (composite as Answer);
                    }
                    completeQuestion.Children.Add(newAnswer);
                }
            }
            if (question.Cards != null)
                foreach (var card in question.Cards)
                {
                    completeQuestion.Cards.Add(card);
                }
            return completeQuestion;

        }

        #endregion
    }
}
