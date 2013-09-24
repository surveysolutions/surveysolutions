using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using Main.Core.Events.Questionnaire;
using Ncqrs.Eventing.ServiceModel.Bus;
using Ncqrs.Eventing.ServiceModel.Bus.ViewConstructorEventBus;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;
using WB.Core.SharedKernels.DataCollection.ReadSide;

namespace WB.Core.BoundedContexts.Supervisor.EventHandler
{
    public class ReferenceInfoForLinkedQuestionsDenormalizer : IEventHandler, IEventHandler<TemplateImported>
    {
        private readonly IVersionedReadSideRepositoryWriter<ReferenceInfoForLinkedQuestions> questionnaires;

        public ReferenceInfoForLinkedQuestionsDenormalizer(
            IVersionedReadSideRepositoryWriter<ReferenceInfoForLinkedQuestions> questionnaires)
        {
            this.questionnaires = questionnaires;
        }

        public string Name
        {
            get { return GetType().Name; }
        }

        public Type[] UsesViews
        {
            get { return new Type[0]; }
        }

        public Type[] BuildsViews
        {
            get { return new[] {typeof (ReferenceInfoForLinkedQuestions)}; }
        }

        public void Handle(IPublishedEvent<TemplateImported> evnt)
        {
            var template = evnt.Payload.Source;
            template.ConnectChildsWithParent();
            var referenceInfo = new Dictionary<Guid, ReferenceInfoByQuestion>();
            
            var linkedQuestions =
                GetAllLinkedQuestions(template);

            var groupsMappedOnPropagatableQuestion =
              this.GetAllPropagatebleGroupsMappedOnPropagatableQuestion(template);

            foreach (var linkedQuestion in linkedQuestions)
            {
                var referencedQuestion =
                    template.FirstOrDefault<IQuestion>(question => question.PublicKey == linkedQuestion.LinkedToQuestionId.Value);

                referenceInfo[linkedQuestion.PublicKey] =
                    new ReferenceInfoByQuestion(GetScopeOfReferencedQuestions(referencedQuestion, groupsMappedOnPropagatableQuestion),
                        referencedQuestion.PublicKey);
            }

            var questionnaire = new ReferenceInfoForLinkedQuestions(evnt.EventSourceId, evnt.EventSequence, referenceInfo);

            questionnaires.Store(questionnaire, evnt.EventSourceId);
        }

        private Guid GetScopeOfReferencedQuestions(IQuestion referencedQuestion, IDictionary<Guid, Guid> groupsMappedOnPropagatableQuestion)
        {
            var questionParent = referencedQuestion.GetParent();

            while (!(questionParent is IQuestionnaireDocument))
            {
                var group = questionParent as IGroup;
                if (group != null && group.Propagated!=Propagate.None)
                {
                    return groupsMappedOnPropagatableQuestion[group.PublicKey];
                }
                questionParent = questionParent.GetParent();
            }
            return questionParent.PublicKey;
        }

        private IDictionary<Guid, Guid> GetAllPropagatebleGroupsMappedOnPropagatableQuestion(QuestionnaireDocument template)
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
