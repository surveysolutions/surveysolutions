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

        public InterviewEntity GetEntityDetails(string id)
        {
            var identity = Identity.Parse(id);

            InterviewTreeQuestion question = this.currentInterview.GetQuestion(identity);
            if (question != null)
            {
                GenericQuestion result = new StubEntity {Id = id};

                if (question.IsSingleFixedOption)
                {
                    result = this.autoMapper.Map<InterviewSingleOptionQuestion>(question);
                    var options = this.currentInterview.GetTopFilteredOptionsForQuestion(identity, null, null, 200);
                    ((InterviewSingleOptionQuestion) result).Options = options;
                }
                else if (question.IsText)
                {
                    InterviewTreeQuestion textQuestion = this.currentInterview.GetQuestion(identity);
                    result = this.autoMapper.Map<InterviewTextQuestion>(textQuestion);
                }

                this.PutInstructions(result, identity);
                return result;
            }

            return null;
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
            if (this.currentQuestionnaire.IsStaticText(entityId)) return InterviewEntityType.StaticText;

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
                    throw new Exception(@"Not supported question type");
            }
        }
    }
}