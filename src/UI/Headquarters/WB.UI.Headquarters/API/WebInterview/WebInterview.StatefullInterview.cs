using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using Microsoft.Practices.ServiceLocation;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.Infrastructure.Transactions;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.UI.Headquarters.Models.WebInterview;

namespace WB.UI.Headquarters.API.WebInterview
{
    public partial class WebInterview
    {
        public InterviewEntityWithType[] GetPrefilledQuestions()
        {
            
            return plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                return plainTransactionManager.ExecuteInQueryTransaction(() =>
                {
                    var result = this.currentQuestionnaire
                                    .GetPrefilledQuestions()
                                    .Select(x => new InterviewEntityWithType
                                    {
                                        Identity = Identity.Create(x, RosterVector.Empty).ToString(),
                                        EntityType = this.GetEntityType(x).ToString()
                                    })
                                    .ToArray();
                    return result;
                });
            });
        }

        public InterviewTextQuestion GetTextQuestion(string questionIdentity)
        {
            return plainTransactionManager.ExecuteInPlainTransaction(() =>
            {
                var id = Identity.Create(Guid.Parse(questionIdentity.Replace(">", "").Replace("<", "")), RosterVector.Empty);

                InterviewTreeQuestion textQuestion = this.currentInterview.GetQuestion(id);
                InterviewTextQuestion result = this.autoMapper.Map<InterviewTextQuestion>(textQuestion);
                result.Instructions = currentQuestionnaire.GetQuestionInstruction(id.Id);
                result.HideInstructions = currentQuestionnaire.GetHideInstructions(id.Id);
                return result;
            });
        }

        private InterviewEntityType GetEntityType(Guid entityId)
        {
            if (this.currentQuestionnaire.HasGroup(entityId)) return InterviewEntityType.Group;
            if(this.currentQuestionnaire.IsRosterGroup(entityId)) return InterviewEntityType.RosterInstance;
            switch (this.currentQuestionnaire.GetQuestionType(entityId))
            {
                case QuestionType.DateTime:
                    return InterviewEntityType.DateTime;
                case QuestionType.GpsCoordinates:
                    return InterviewEntityType.Gps;
                case QuestionType.Multimedia:
                    return InterviewEntityType.Multimedia;
                case QuestionType.MultyOption:
                    return InterviewEntityType.CategoricalMulti;
                case QuestionType.SingleOption:
                    return InterviewEntityType.CategoricalSingle;
                case QuestionType.Numeric:
                    return this.currentQuestionnaire.IsQuestionInteger(entityId)
                        ? InterviewEntityType.Integer
                        : InterviewEntityType.Double;
                case QuestionType.Text:
                    return InterviewEntityType.TextQuestion;
                default:
                    return InterviewEntityType.StaticText;
            }
        }
    }
}