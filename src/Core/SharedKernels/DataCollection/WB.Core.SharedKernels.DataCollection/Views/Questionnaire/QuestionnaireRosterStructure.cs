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
            this.RosterScopes = new Dictionary<Guid, HashSet<Guid>>();
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
                var triggerHashSet = new HashSet<Guid>();

                foreach (var trigger in autoPropagatebleQuestion.Triggers)
                {
                    triggerHashSet.Add(trigger);
                }

                this.RosterScopes.Add(autoPropagatebleQuestion.PublicKey,
                    triggerHashSet);
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
                this.RosterScopes.Add(rosterSizeQuestion.PublicKey,
                    new HashSet<Guid>(
                        rosterGroups.Where(group => group.RosterSizeQuestionId == rosterSizeQuestion.PublicKey)
                            .Select(group => group.PublicKey)));
            }
        }

        public Guid QuestionnaireId { get; set; }
        public Dictionary<Guid, HashSet<Guid>> RosterScopes { get; set; }
        public long Version { get; set; }
    }
}
