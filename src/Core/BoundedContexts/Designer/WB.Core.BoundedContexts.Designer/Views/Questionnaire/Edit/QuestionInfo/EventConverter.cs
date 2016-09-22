using System;
using Main.Core.Entities;
using Main.Core.Entities.SubEntities;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using WB.Core.BoundedContexts.Designer.Events.Questionnaire;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo
{
    internal static class EventConverter
    {
        #region for very-very old and invalid events
        internal static Answer[] GetValidAnswersCollection(Answer[] answers)
        {
            if (answers == null)
                return null;

            foreach (var answer in answers)
            {
                if (string.IsNullOrWhiteSpace(answer.AnswerValue))
                {
                    answer.AnswerValue = (new Random().NextDouble() * 100).ToString("0.00");
                }
                if (string.IsNullOrWhiteSpace(answer.AnswerText))
                {
                    answer.AnswerText = "Option " + answer.AnswerValue;
                }
            }
            return answers;
        } 
        #endregion
    }
}