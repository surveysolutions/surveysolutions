using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Services.Export.Interview.Entities;
using WB.Services.Export.Questionnaire;
using WB.Services.Export.Services;

namespace WB.Services.Export.Interview.Exporters
{
    public interface IInterviewFactory
    {
        Task<IEnumerable<InterviewEntity>> GetInterviewEntities(Guid interviewId);
        Dictionary<string, InterviewLevel> GetInterviewDataLevels(QuestionnaireDocument questionnaire, List<InterviewEntity> interviewEntities);
    }

    internal class InterviewFactory : IInterviewFactory
    {
        private readonly IHeadquartersApi headquartersApi;

        public InterviewFactory(IHeadquartersApi headquartersApi)
        {
            this.headquartersApi = headquartersApi;
        }

        public async Task<IEnumerable<InterviewEntity>> GetInterviewEntities(Guid interviewId)
        {
            var entities = await headquartersApi.GetInterviewAsync(interviewId);
            return entities;
        }

        public Dictionary<string, InterviewLevel> GetInterviewDataLevels(QuestionnaireDocument questionnaire, List<InterviewEntity> interviewEntities)
        {
            var interviewLevels = interviewEntities
                .GroupBy(x => x.Identity?.RosterVector ?? RosterVector.Empty)
                .Select(x => ToInterviewLevel(x.Key, x.ToArray(), questionnaire));

            var interviewDataLevels = interviewLevels.ToDictionary(k => CreateLevelIdFromPropagationVector(k.RosterVector), v => v);
            return interviewDataLevels;
        }

        public static string CreateLevelIdFromPropagationVector(RosterVector vector)
        {
            if (vector.Length == 0)
                return "#";
            return vector.ToString();
        }

        private InterviewLevel ToInterviewLevel(RosterVector rosterVector, InterviewEntity[] interviewDbEntities, QuestionnaireDocument questionnaire)
        {
            Dictionary<ValueVector<Guid>, int?> scopeVectors = new Dictionary<ValueVector<Guid>, int?>();
            if (rosterVector.Length > 0)
            {
                // too slow
                scopeVectors = interviewDbEntities
                    .Select(x => questionnaire.GetRosterSizeSourcesForEntity(x.Identity.Id))
                    .Select(x => new ValueVector<Guid>(x))
                    .Distinct()
                    .ToDictionary(x => x, x => (int?)0);
            }
            else
            {
                scopeVectors.Add(new ValueVector<Guid>(), 0);
            }

            var interviewLevel = new InterviewLevel
            {
                ScopeVectors = scopeVectors,
                RosterVector = rosterVector
            };

            foreach (var entity in interviewDbEntities)
            {
                switch (entity.EntityType)
                {
                    case EntityType.Question:
                        interviewLevel.QuestionsSearchCache.Add(entity.Identity.Id, entity);
                        break;
                    case EntityType.Variable:
                        interviewLevel.Variables.Add(entity.Identity.Id, ToObjectAnswer(entity));
                        if (entity.IsEnabled == false)
                            interviewLevel.DisabledVariables.Add(entity.Identity.Id);
                        break;
                }
            }

            return interviewLevel;
        }

        private object ToObjectAnswer(InterviewEntity entity) => entity.AsString ?? entity.AsInt ?? entity.AsDouble ??
                                                                 entity.AsDateTime ?? entity.AsLong ??
                                                                 entity.AsBool ?? entity.AsGps ?? entity.AsIntArray ??
                                                                 entity.AsList ?? entity.AsYesNo ??
                                                                 entity.AsIntMatrix ?? entity.AsArea ??
                                                                 (object)entity.AsAudio;
    }
}
