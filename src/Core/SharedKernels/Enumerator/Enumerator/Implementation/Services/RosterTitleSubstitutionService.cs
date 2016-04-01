using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Entities.Interview;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    internal class RosterTitleSubstitutionService : IRosterTitleSubstitutionService
    {
        private readonly IPlainQuestionnaireRepository questionnaireStorage;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ISubstitutionService substitutionService;

        public RosterTitleSubstitutionService(
            IPlainQuestionnaireRepository questionnaireStorage,
            IStatefulInterviewRepository interviewRepository,
            ISubstitutionService substitutionService)
        {
            this.questionnaireStorage = questionnaireStorage;
            this.interviewRepository = interviewRepository;
            this.substitutionService = substitutionService;
        }

        public string Substitute(string questionTitle, Identity questionIdentity, string interviewId)
        {
            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireStorage.GetQuestionnaire(interview.QuestionnaireIdentity);

            Guid nearestRosterId = questionnaire.GetRostersFromTopToSpecifiedQuestion(questionIdentity.Id).Last();

            InterviewRoster roster = interview.FindRosterByOrDeeperRosterLevel(nearestRosterId, questionIdentity.RosterVector);

            var replaceTo = string.IsNullOrEmpty(roster.Title) ? this.substitutionService.DefaultSubstitutionText : roster.Title;
            var result = this.substitutionService.ReplaceSubstitutionVariable(questionTitle, this.substitutionService.RosterTitleSubstitutionReference, replaceTo);
            return result;
        }
    }
}