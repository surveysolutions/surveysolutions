using System;
using System.Collections.Generic;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection.ReadSide;

namespace WB.Core.SharedKernels.DataCollection.Views.Questionnaire
{
    public class QuestionnairePropagationStructure : IVersionedView
    {
        public QuestionnairePropagationStructure()
        {
            this.PropagationScopes = new Dictionary<Guid, HashSet<Guid>>();
        }

        public QuestionnairePropagationStructure(QuestionnaireDocument questionnaire, long version)
            : this()
        {
            QuestionnaireId = questionnaire.PublicKey;
            Version = version;

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

                PropagationScopes.Add(autoPropagatebleQuestion.PublicKey,
                    triggerHashSet);
            }
        }

        public Guid QuestionnaireId { get; set; }
        public Dictionary<Guid, HashSet<Guid>> PropagationScopes { get; set; }
        public long Version { get; set; }
    }
}
