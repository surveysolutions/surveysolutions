using System.Linq;
using WB.Core.BoundedContexts.Tester.Implementation.Entities;
using WB.Core.BoundedContexts.Tester.Repositories;
using WB.Core.BoundedContexts.Tester.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveySolutions.Services;

namespace WB.Core.BoundedContexts.Tester.Implementation.Services
{
    internal class RosterTitleSubstitutionService : IRosterTitleSubstitutionService
    {
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireStorage;
        private readonly IStatefulInterviewRepository interviewRepository;
        private readonly ISubstitutionService substitutionService;

        public RosterTitleSubstitutionService(IPlainKeyValueStorage<QuestionnaireModel> questionnaireStorage,
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
            var questionnaire = this.questionnaireStorage.GetById(interview.QuestionnaireId);

            var nearestRosterId  = questionnaire.QuestionsNearestRosterIdMap[questionIdentity.Id];

            InterviewRoster roster = interview.FindRosterByOrDeeperRosterLevel(nearestRosterId.Value, questionIdentity.RosterVector);

            var replaceTo = string.IsNullOrEmpty(roster.Title) ? this.substitutionService.DefaultSubstitutionText : roster.Title;
            var result = this.substitutionService.ReplaceSubstitutionVariable(questionTitle, this.substitutionService.RosterTitleSubstitutionReference, replaceTo);
            return result;
        }
    }
}