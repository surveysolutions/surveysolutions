using System;
using System.Linq;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    internal class RosterTitleSubstitutionService : IRosterTitleSubstitutionService
    {
        private readonly IQuestionnaireStorage questionnaireStorage;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ISubstitutionService substitutionService;

        public RosterTitleSubstitutionService(
            IQuestionnaireStorage questionnaireStorage,
            IStatefulInterviewRepository interviewRepository,
            ISubstitutionService substitutionService)
        {
            this.questionnaireStorage = questionnaireStorage;
            this.interviewRepository = interviewRepository;
            this.substitutionService = substitutionService;
        }

        public string Substitute(string title, Identity entityIdentity, string interviewId)
        {
            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(interview.QuestionnaireIdentity, interview.Language);

            Guid nearestRosterId = questionnaire.GetRostersFromTopToSpecifiedEntity(entityIdentity.Id).Last();

            int rosterLevel = questionnaire.GetRosterLevelForGroup(nearestRosterId);
            var rosterVector = entityIdentity.RosterVector.Shrink(rosterLevel);

            InterviewTreeRoster roster = interview.GetRoster(Identity.Create(nearestRosterId, rosterVector));

            if (roster != null)
            {
                var replaceTo = string.IsNullOrEmpty(roster.RosterTitle) ? this.substitutionService.DefaultSubstitutionText : roster.RosterTitle;
                var result = this.substitutionService.ReplaceSubstitutionVariable(title, this.substitutionService.RosterTitleSubstitutionReference, replaceTo);
                return result;
            }

            return title;
        }
    }
}