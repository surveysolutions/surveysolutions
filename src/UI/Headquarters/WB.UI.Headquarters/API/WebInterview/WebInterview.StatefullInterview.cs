using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.UI.Headquarters.Models.WebInterview;

namespace WB.UI.Headquarters.API.WebInterview
{
    public partial class WebInterview
    {
        public InterviewEntityWithType[] GetPrefilledQuestions()
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
        }

        public InterviewTextQuestion GetTextQuestion(string questionIdentity)
        {
            var id = Identity.Parse(questionIdentity);

            InterviewTreeQuestion textQuestion = this.currentInterview.GetQuestion(id);
            InterviewTextQuestion result = this.autoMapper.Map<InterviewTextQuestion>(textQuestion);
            this.PutInstructions(result, id);
            return result;
        }

        public InterviewSingleOptionQuestion GetSingleOptionQuestion(string questionIdentity)
        {
            var id = Identity.Parse(questionIdentity);

            InterviewTreeQuestion question = this.currentInterview.GetQuestion(id);
            InterviewSingleOptionQuestion result = this.autoMapper.Map<InterviewSingleOptionQuestion>(question);

            var options = this.currentInterview.GetTopFilteredOptionsForQuestion(id, null, null, 200);
            result.Options = options;

            this.PutInstructions(result, id);
            return result;
        }

        private void PutInstructions(GenericQuestion result, Identity id)
        {
            result.Instructions = this.currentQuestionnaire.GetQuestionInstruction(id.Id);
            result.HideInstructions = this.currentQuestionnaire.GetHideInstructions(id.Id);
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