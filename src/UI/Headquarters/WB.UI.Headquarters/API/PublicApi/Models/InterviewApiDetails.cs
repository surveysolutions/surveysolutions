using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.Core.SharedKernels.SurveyManagement.Web.Models.Api.Interview;

namespace WB.UI.Headquarters.API.PublicApi.Models
{
    public class InterviewApiDetails
    {
        public InterviewApiDetails(IStatefulInterview interview)
        {
            if (interview == null) return;

            var allNodes = interview.GetAllInterviewNodes().ToArray();
            var entitiesOutOfRosters = allNodes.Where(x => x.Identity.RosterVector == RosterVector.Empty).ToArray();

            this.Questions = entitiesOutOfRosters.OfType<InterviewTreeQuestion>().Select(ToQuestionApiView).ToList();

            this.Rosters = allNodes.OfType<InterviewTreeRoster>()
                .Where(x => x.Identity.RosterVector.Length == 1)
                .Select(ToRosterApiView).ToList();
        }

        private RosterApiItem ToRosterApiView(InterviewTreeRoster rosterInstance)
        {
            var allRosterNodes = rosterInstance.Children.TreeToEnumerable(x => x.Children);
            var questionsOfRoster = allRosterNodes
                .OfType<InterviewTreeQuestion>()
                .Where(x => x.Identity.RosterVector == rosterInstance.RosterVector)
                .Select(ToQuestionApiView)
                .ToList();

            var rosterInstancesOfRoster = allRosterNodes
                .OfType<InterviewTreeRoster>()
                .Where(x => x.Identity.RosterVector.Length == rosterInstance.Identity.RosterVector.Length + 1)
                .Select(ToRosterApiView)
                .ToList();

            return new RosterApiItem
            {
                Id = rosterInstance.Identity.Id,
                RosterVector = rosterInstance.Identity.RosterVector,
                Questions = questionsOfRoster,
                Rosters = rosterInstancesOfRoster
            };
        }

        private QuestionApiItem ToQuestionApiView(InterviewTreeQuestion question) =>
            new QuestionApiItem(question.VariableName, InterviewTreeQuestion.GetAnswerAsObject(question) ?? string.Empty);
        
        [DataMember]
        public List<QuestionApiItem> Questions { set; get; }

        [DataMember]
        public List<RosterApiItem> Rosters { set; get; }
    }
}