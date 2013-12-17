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
                Guid? titleQuestionId = GetRosterTitleQuestionIdByAutopropagatedQuestion(questionnaire, autoPropagatebleQuestion);

                var rosterDescription = new RosterDescription(autoPropagatebleQuestion.PublicKey, titleQuestionId);

                foreach (var trigger in autoPropagatebleQuestion.Triggers)
                {
                    rosterDescription.RosterGroupsId.Add(trigger);
                }

                this.RosterScopes.Add(autoPropagatebleQuestion.PublicKey,
                    rosterDescription);
            }


            //### roster
            var rosterGroups =
                questionnaire.Find<IGroup>(question => question.IsRoster && question.RosterSizeQuestionId.HasValue);

            var rosterSizeQuestions =
                questionnaire.Find<IQuestion>(
                    question =>
                        rosterGroups.Any(
                            group => group.RosterSizeQuestionId.Value == question.PublicKey));

            foreach (var rosterSizeQuestion in rosterSizeQuestions)
            {
                var groupsFromRosterSizeQuestionScope =
                    rosterGroups.Where(group => group.RosterSizeQuestionId == rosterSizeQuestion.PublicKey).ToList();
                
                Guid? titleQuestionId = GetRosterTitleQuestionId(groupsFromRosterSizeQuestionScope);

                var rosterDescription = new RosterDescription(rosterSizeQuestion.PublicKey, titleQuestionId);
               
                groupsFromRosterSizeQuestionScope.ForEach(group => rosterDescription.RosterGroupsId.Add(group.PublicKey));

                RosterScopes.Add(rosterSizeQuestion.PublicKey, rosterDescription);
            }
        }

        private Guid? GetRosterTitleQuestionId(IEnumerable<IGroup> groupsFromRosterSizeQuestionScope)
        {
            var titleQuestionsId = groupsFromRosterSizeQuestionScope.Where(group => group.RosterTitleQuestionId.HasValue).Select(group => group.RosterTitleQuestionId.Value);

            Guid? titleQuestionId = null;
            if (titleQuestionsId.Any())
            {
                titleQuestionId = titleQuestionsId.First();
            }
            return titleQuestionId;
        }

        private Guid? GetRosterTitleQuestionIdByAutopropagatedQuestion(QuestionnaireDocument questionnaire, IAutoPropagateQuestion autoPropagateQuestion)
        {
            IEnumerable<IGroup> groupsFromAutoPropagatedQuestionScope =
                questionnaire.Find<IGroup>(group => autoPropagateQuestion.Triggers.Contains(group.PublicKey));

            var titleQuestionsId =
                groupsFromAutoPropagatedQuestionScope.SelectMany(group => group.Find<IQuestion>(question => question.Capital)).Select(question=>question.PublicKey);

            Guid? titleQuestionId = null;
            if (titleQuestionsId.Any())
            {
                titleQuestionId = titleQuestionsId.First();
            }
            return titleQuestionId;
        }

        public Guid QuestionnaireId { get; set; }
        public Dictionary<Guid, RosterDescription> RosterScopes { get; set; }
        public long Version { get; set; }
    }
}
