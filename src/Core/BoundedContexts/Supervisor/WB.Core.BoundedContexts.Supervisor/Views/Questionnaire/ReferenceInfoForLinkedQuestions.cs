using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.SharedKernels.DataCollection.ReadSide;

namespace WB.Core.BoundedContexts.Supervisor.Views.Questionnaire
{
    public class ReferenceInfoForLinkedQuestions : IVersionedView
    {
        public ReferenceInfoForLinkedQuestions()
        {
            this.ReferencesOnLinkedQuestions = new Dictionary<Guid, ReferenceInfoByQuestion>();
        }

        public ReferenceInfoForLinkedQuestions(QuestionnaireDocument questionnaire, long version)
        {
            this.QuestionnaireId = questionnaire.PublicKey;
            this.Version = version;
            
            var referenceInfo = new Dictionary<Guid, ReferenceInfoByQuestion>();

            var linkedQuestions = GetAllLinkedQuestions(questionnaire);

            var groupsMappedOnPropagatableQuestion = this.GetAllRosterScopesGroupedByRosterId(questionnaire);

            foreach (var linkedQuestion in linkedQuestions)
            {
                var referencedQuestion =
                    questionnaire.FirstOrDefault<IQuestion>(question => question.PublicKey == linkedQuestion.LinkedToQuestionId.Value);

                referenceInfo[linkedQuestion.PublicKey] = new ReferenceInfoByQuestion(
                    GetScopeOfReferencedQuestions(referencedQuestion, groupsMappedOnPropagatableQuestion),
                    referencedQuestion.PublicKey);
            }

            this.ReferencesOnLinkedQuestions = referenceInfo;
        }

        public Guid QuestionnaireId { get; set; }
        public long Version { get; set; }
        public Dictionary<Guid, ReferenceInfoByQuestion> ReferencesOnLinkedQuestions { get; private set; }

        private Guid GetScopeOfReferencedQuestions(IQuestion referencedQuestion, IDictionary<Guid, Guid> groupsMappedOnPropagatableQuestion)
        {
            var questionParent = referencedQuestion.GetParent();

            while (!(questionParent is IQuestionnaireDocument))
            {
                var group = questionParent as IGroup;
                if (group != null && (group.Propagated != Propagate.None || group.IsRoster))
                {
                    return groupsMappedOnPropagatableQuestion[group.PublicKey];
                }
                questionParent = questionParent.GetParent();
            }
            return questionParent.PublicKey;
        }

        private IDictionary<Guid, Guid> GetAllRosterScopesGroupedByRosterId(QuestionnaireDocument template)
        {
            var result = new Dictionary<Guid, Guid>();

            foreach (var scope in template.Find<IAutoPropagateQuestion>(
                question =>
                    question.QuestionType == QuestionType.Numeric || question.QuestionType == QuestionType.AutoPropagate))
            {
                foreach (var triggarableGroup in scope.Triggers)
                {
                    result.Add(triggarableGroup, scope.PublicKey);
                }
            }

            foreach (var roster in template.Find<IGroup>(group => group.IsRoster && group.RosterSizeSource == RosterSizeSourceType.Question))
            {
                result.Add(roster.PublicKey, roster.RosterSizeQuestionId.Value);
            }

            foreach (var roster in template.Find<IGroup>(group => group.IsRoster && group.RosterSizeSource == RosterSizeSourceType.FixedTitles))
            {
                result.Add(roster.PublicKey, roster.PublicKey);
            }

            return result;
        }

        private IEnumerable<IQuestion> GetAllLinkedQuestions(QuestionnaireDocument template)
        {
            return template.Find<IQuestion>(
                question =>
                    (question.QuestionType == QuestionType.SingleOption || question.QuestionType == QuestionType.MultyOption) &&
                        question.LinkedToQuestionId.HasValue);
        }
    }
}
