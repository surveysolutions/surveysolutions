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
            this.RosterScopes = new Dictionary<Guid, RosterScopeDescription>();
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
                var rosterIdMappedOfRosterTitleQuestionId = this.GetRosterIdMappedOfRosterTitleQuestionIdByAutopropagatedQuestion(questionnaire, autoPropagatebleQuestion);

                var rosterDescription = new RosterScopeDescription(autoPropagatebleQuestion.PublicKey, rosterIdMappedOfRosterTitleQuestionId);

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

                var rosterIdWithTitleQuestionIds = this.GetRosterIdMappedOfRosterTitleQuestionIdByRostersInScope(groupsFromRosterSizeQuestionScope);

                var rosterDescription = new RosterScopeDescription(rosterSizeQuestion.PublicKey, rosterIdWithTitleQuestionIds);

                RosterScopes.Add(rosterSizeQuestion.PublicKey, rosterDescription);
            }

            foreach (var fixedRosterGroup in fixedRosterGroups)
            {
                this.RosterScopes[fixedRosterGroup.PublicKey] = new RosterScopeDescription(fixedRosterGroup.PublicKey,
                    new Dictionary<Guid, Guid?> { { fixedRosterGroup.PublicKey, null } });
            }
        }

        private Dictionary<Guid, Guid?> GetRosterIdMappedOfRosterTitleQuestionIdByRostersInScope(IEnumerable<IGroup> groupsFromRosterSizeQuestionScope)
        {
            return
                groupsFromRosterSizeQuestionScope
                    .ToDictionary(roster => roster.PublicKey, roster => roster.RosterTitleQuestionId);
        }

        private Dictionary<Guid, Guid?> GetRosterIdMappedOfRosterTitleQuestionIdByAutopropagatedQuestion(QuestionnaireDocument questionnaire, IAutoPropagateQuestion autoPropagateQuestion)
        {
            IEnumerable<IGroup> groupsFromAutoPropagatedQuestionScope =
                questionnaire.Find<IGroup>(group => autoPropagateQuestion.Triggers.Contains(group.PublicKey));

            Dictionary<Guid, Guid?> rosterGroupsWithTitleQuestionPairs = new Dictionary<Guid, Guid?>();
            
            foreach (var rosterGroup in groupsFromAutoPropagatedQuestionScope)
            {
                var capitalQuestions = rosterGroup.Find<IQuestion>(question => question.Capital);
                Guid? headQuestionId = null;
                if (capitalQuestions.Any())
                {
                    headQuestionId = capitalQuestions.First().PublicKey;
                }
                rosterGroupsWithTitleQuestionPairs.Add(rosterGroup.PublicKey, headQuestionId);
            }
            return rosterGroupsWithTitleQuestionPairs;
        }

        public Guid QuestionnaireId { get; set; }
        public Dictionary<Guid, RosterScopeDescription> RosterScopes { get; set; }
        public long Version { get; set; }
    }
}
