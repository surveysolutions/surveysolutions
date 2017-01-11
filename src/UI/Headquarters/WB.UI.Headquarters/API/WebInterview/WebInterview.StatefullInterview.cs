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
        public PrefilledPageData GetPrefilledPageData()
        {
            var questions = this.GetCallerQuestionnaire()
                .GetPrefilledQuestions()
                .Select(x => new InterviewEntityWithType
                {
                    Identity = Identity.Create(x, RosterVector.Empty).ToString(),
                    EntityType = this.GetEntityType(x).ToString()
                })
                .ToArray();
            var result = new PrefilledPageData();
            result.Questions = questions;
            result.FirstSectionId = Identity.Create(this.GetCallerQuestionnaire().GetAllSections().First(), RosterVector.Empty).ToString();

            return result;
        }

        public SectionData GetSection(string sectionId)
        {
            if (sectionId == null) throw new ArgumentNullException(nameof(sectionId));
            Identity secitonIdentity = Identity.Parse(sectionId);
            var statefulInterview = this.GetCallerInterview();
            var ids = statefulInterview.GetUnderlyingInterviewerEntities(secitonIdentity);

            var entities = ids.Select(x => new InterviewEntityWithType
               {
                   Identity = x.ToString(),
                   EntityType = this.GetEntityType(x.Id).ToString()
               })
               .ToArray();
            return new SectionData
            {
                Entities =  entities,
                Title = statefulInterview.GetGroup(secitonIdentity).Title.Text
            };
        }

        public InterviewEntity GetEntityDetails(string id)
        {
            var identity = Identity.Parse(id);
            var callerInterview = this.GetCallerInterview();

            InterviewTreeQuestion question = callerInterview.GetQuestion(identity);
            if (question != null)
            {
                GenericQuestion result = new StubEntity {Id = id};

                if (question.IsSingleFixedOption)
                {
                    result = this.autoMapper.Map<InterviewSingleOptionQuestion>(question);
                    var options = callerInterview.GetTopFilteredOptionsForQuestion(identity, null, null, 200);
                    ((InterviewSingleOptionQuestion) result).Options = options;
                }
                else if (question.IsText)
                {
                    InterviewTreeQuestion textQuestion = callerInterview.GetQuestion(identity);
                    result = this.autoMapper.Map<InterviewTextQuestion>(textQuestion);
                }

                this.PutInstructions(result, identity);
                return result;
            }

            return null;
        }

        private void PutInstructions(GenericQuestion result, Identity id)
        {
            var callerQuestionnaire = this.GetCallerQuestionnaire();

            result.Instructions = callerQuestionnaire.GetQuestionInstruction(id.Id);
            result.HideInstructions = callerQuestionnaire.GetHideInstructions(id.Id);
        }

        private InterviewEntityType GetEntityType(Guid entityId)
        {
            var callerQuestionnaire = this.GetCallerQuestionnaire();

            if (callerQuestionnaire.HasGroup(entityId)) return InterviewEntityType.Group;
            if(callerQuestionnaire.IsRosterGroup(entityId)) return InterviewEntityType.RosterInstance;
            if (callerQuestionnaire.IsStaticText(entityId)) return InterviewEntityType.StaticText;

            switch (callerQuestionnaire.GetQuestionType(entityId))
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
                    return callerQuestionnaire.IsQuestionInteger(entityId)
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