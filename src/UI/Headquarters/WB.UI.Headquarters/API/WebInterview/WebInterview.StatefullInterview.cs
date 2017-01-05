using System;
using System.Linq;
using Main.Core.Entities.SubEntities;
using WB.Core.SharedKernels.DataCollection;
using WB.UI.Headquarters.Models.WebInterview;

namespace WB.UI.Headquarters.API.WebInterview
{
    public partial class WebInterview
    {
        public InterviewEntityWithType[] GetPrefilledQuestions() => this.currentQuestionnaire
            .GetPrefilledQuestions()
            .Select(x => new InterviewEntityWithType
            {
                Identity = Identity.Create(x, RosterVector.Empty),
                EntityType = this.GetEntityType(x)
            })
            .ToArray();

        public InterviewTextQuestion GetTextQuestion(Identity questionIdentity)
            => this.autoMapper.Map<InterviewTextQuestion>(this.currentInterview.GetTextQuestion(questionIdentity));

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
                default:
                    return InterviewEntityType.StaticText;
            }
        }
    }
}