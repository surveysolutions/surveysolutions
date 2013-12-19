using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection.ReadSide;
using System.Linq;

namespace WB.Core.SharedKernels.DataCollection.Views.Questionnaire
{
    public class QuestionnaireRosterStructure : IVersionedView
    {
        public QuestionnaireRosterStructure()
        {
            this.RosterScopes = new Dictionary<Guid, RosterDescription>();
        }

        public QuestionnaireRosterStructure(QuestionnaireDocument questionnaire, long version)
            : this()
        {
            QuestionnaireId = questionnaire.PublicKey;
            Version = version;

            //### old questionnaires supporting
            var autoPropagatebleQuestions =
                questionnaire.Find<IAutoPropagateQuestion>(
                    question =>
                        question.QuestionType == QuestionType.Numeric || question.QuestionType == QuestionType.AutoPropagate);

            foreach (var autoPropagatebleQuestion in autoPropagatebleQuestions)
            {
                var rosterGroupsWithTitleQuestionPairs = this.GetRosterGroupsWithTitleQuestionPairsByAutopropagatedQuestion(questionnaire, autoPropagatebleQuestion);

                var rosterDescription = new RosterDescription(autoPropagatebleQuestion.PublicKey, rosterGroupsWithTitleQuestionPairs);

                foreach (var trigger in autoPropagatebleQuestion.Triggers)
                {
                    rosterDescription.RosterGroupsId.Add(trigger);
                }

                this.RosterScopes.Add(autoPropagatebleQuestion.PublicKey,
                    rosterDescription);
            }


            //### roster
            var rosterGroups = questionnaire.Find<IGroup>(@group => @group.IsRoster && @group.RosterSizeSource == RosterSizeSourceType.Question);
            var fixedRosterGroups = questionnaire.Find<IGroup>(@group => @group.IsRoster && @group.RosterSizeSource == RosterSizeSourceType.FixedTitles).ToList();

            var rosterSizeQuestions = questionnaire.Find<IQuestion>(question => rosterGroups.Any(group => group.RosterSizeQuestionId.Value == question.PublicKey));

            foreach (var rosterSizeQuestion in rosterSizeQuestions)
            {
                var groupsFromRosterSizeQuestionScope =
                    rosterGroups.Where(group => group.RosterSizeQuestionId == rosterSizeQuestion.PublicKey).ToList();

                var rosterGroupsWithTitleQuestionPairs = this.GetRosterGroupsWithTitleQuestionPairsByRostersInScope(groupsFromRosterSizeQuestionScope);

                var rosterDescription = new RosterDescription(rosterSizeQuestion.PublicKey, rosterGroupsWithTitleQuestionPairs);
               
                groupsFromRosterSizeQuestionScope.ForEach(group => rosterDescription.RosterGroupsId.Add(group.PublicKey));

                RosterScopes.Add(rosterSizeQuestion.PublicKey, rosterDescription);
            }

            foreach (var fixedRosterGroup in fixedRosterGroups)
            {
                var rosterDescription = new RosterDescription(fixedRosterGroup.PublicKey, null);
                rosterDescription.RosterGroupsId.Add(fixedRosterGroup.PublicKey);
                this.RosterScopes[fixedRosterGroup.PublicKey] = rosterDescription;
            }
        }

        private Dictionary<Guid, Guid> GetRosterGroupsWithTitleQuestionPairsByRostersInScope(IEnumerable<IGroup> groupsFromRosterSizeQuestionScope)
        {
            return
                groupsFromRosterSizeQuestionScope.Where(group => group.RosterTitleQuestionId.HasValue)
                    .ToDictionary(roster => roster.PublicKey, roster => roster.RosterTitleQuestionId.Value);
        }

        private Dictionary<Guid, Guid> GetRosterGroupsWithTitleQuestionPairsByAutopropagatedQuestion(QuestionnaireDocument questionnaire, IAutoPropagateQuestion autoPropagateQuestion)
        {
            IEnumerable<IGroup> groupsFromAutoPropagatedQuestionScope =
                questionnaire.Find<IGroup>(group => autoPropagateQuestion.Triggers.Contains(group.PublicKey));

            Dictionary<Guid, Guid> rosterGroupsWithTitleQuestionPairs = new Dictionary<Guid, Guid>();
            
            foreach (var rosterGroup in groupsFromAutoPropagatedQuestionScope)
            {
                var capitalQuestions = rosterGroup.Find<IQuestion>(question => question.Capital);
                if (capitalQuestions.Any())
                {
                    rosterGroupsWithTitleQuestionPairs[rosterGroup.PublicKey] = capitalQuestions.First().PublicKey;
                }
            }
            return rosterGroupsWithTitleQuestionPairs;
        }

        public Guid QuestionnaireId { get; set; }
        public Dictionary<Guid, RosterDescription> RosterScopes { get; set; }
        public long Version { get; set; }
    }
}
