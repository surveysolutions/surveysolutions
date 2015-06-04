using System.Linq;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Repositories;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.SurveySolutions.Services;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Services
{
    internal class RosterTitleSubstitutionService : IRosterTitleSubstitutionService
    {
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireStorage;
        private readonly IStatefullInterviewRepository interviewRepository;
        private readonly ISubstitutionService substitutionService;

        public RosterTitleSubstitutionService(IPlainKeyValueStorage<QuestionnaireModel> questionnaireStorage,
            IStatefullInterviewRepository interviewRepository,
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

            var parents = questionnaire.Parents[questionIdentity.Id];
            var nearestRoster = parents.Last(p => p.IsRoster);

            InterviewRoster roster = interview.FindRosterByOrDeeperRosterLevel(nearestRoster.Id, questionIdentity.RosterVector);

            var replaceTo = string.IsNullOrEmpty(roster.Title) ? this.substitutionService.DefaultSubstitutionText : roster.Title;
            var result = this.substitutionService.ReplaceSubstitutionVariable(questionTitle, this.substitutionService.RosterTitleSubstitutionReference, replaceTo);
            return result;
        }
    }
}