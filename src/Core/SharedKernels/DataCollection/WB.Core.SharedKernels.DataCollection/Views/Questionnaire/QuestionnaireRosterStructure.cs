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
                var rosterIdMappedOfRosterTitleQuestionId = this.GetRosterIdToRosterTitleQuestionIdMapByAutopropagatedQuestion(questionnaire, autoPropagatebleQuestion);

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

                var rosterIdWithTitleQuestionIds = this.GetRosterIdToRosterTitleQuestionIdMapByRostersInScope(questionnaire, groupsFromRosterSizeQuestionScope);

                var rosterDescription = new RosterScopeDescription(rosterSizeQuestion.PublicKey, rosterIdWithTitleQuestionIds);

                RosterScopes.Add(rosterSizeQuestion.PublicKey, rosterDescription);
            }

            foreach (var fixedRosterGroup in fixedRosterGroups)
            {
                this.RosterScopes[fixedRosterGroup.PublicKey] = new RosterScopeDescription(fixedRosterGroup.PublicKey,
                    new Dictionary<Guid, RosterTitleQuestionDescription> { { fixedRosterGroup.PublicKey, null } });
            }
        }

        private Dictionary<Guid, RosterTitleQuestionDescription> GetRosterIdToRosterTitleQuestionIdMapByRostersInScope(QuestionnaireDocument questionnaire, IEnumerable<IGroup> groupsFromRosterSizeQuestionScope)
        {
            return
                groupsFromRosterSizeQuestionScope
                    .ToDictionary(roster => roster.PublicKey,
                        roster => roster.RosterTitleQuestionId.HasValue
                            ? CreateRosterTitleQuestionDescription(
                                questionnaire.FirstOrDefault<IQuestion>(question => question.PublicKey == roster.RosterTitleQuestionId.Value))
                            : null);
        }

        private Dictionary<Guid, RosterTitleQuestionDescription> GetRosterIdToRosterTitleQuestionIdMapByAutopropagatedQuestion(QuestionnaireDocument questionnaire, IAutoPropagateQuestion autoPropagateQuestion)
        {
            IEnumerable<IGroup> groupsFromAutoPropagatedQuestionScope =
                questionnaire.Find<IGroup>(group => autoPropagateQuestion.Triggers.Contains(group.PublicKey));

            var capitalQuestions =
                groupsFromAutoPropagatedQuestionScope.SelectMany(rosterGroup => rosterGroup.Find<IQuestion>(question => question.Capital));

            RosterTitleQuestionDescription headQuestion = null;

            if (capitalQuestions.Any())
            {
                headQuestion = CreateRosterTitleQuestionDescription(capitalQuestions.First());
            }

            var rosterGroupsWithTitleQuestionPairs = new Dictionary<Guid, RosterTitleQuestionDescription>();
            
            foreach (var rosterGroup in groupsFromAutoPropagatedQuestionScope)
            {
                rosterGroupsWithTitleQuestionPairs.Add(rosterGroup.PublicKey, headQuestion);
            }
            return rosterGroupsWithTitleQuestionPairs;
        }

        private RosterTitleQuestionDescription CreateRosterTitleQuestionDescription(IQuestion question)
        {
            return new RosterTitleQuestionDescription(question.PublicKey,
                question.Answers.ToDictionary(a => decimal.Parse(a.AnswerValue), a => a.AnswerText));
        }

        public Guid QuestionnaireId { get; set; }
        public Dictionary<Guid, RosterScopeDescription> RosterScopes { get; set; }
        public long Version { get; set; }
    }
}
