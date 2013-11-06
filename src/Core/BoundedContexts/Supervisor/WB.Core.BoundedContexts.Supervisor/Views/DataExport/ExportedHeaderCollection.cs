using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Complete;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.Views.DataExport
{
    public class ExportedHeaderCollection : IEnumerable<ExportedHeaderItem>
    {
        protected IDictionary<Guid, ExportedHeaderItem> container;
        private readonly ReferenceInfoForLinkedQuestions questionnaireReferences;
        private readonly Dictionary<Guid, AutoPropagateQuestion> autoPropagatedQuestions;

        public ExportedHeaderCollection(ReferenceInfoForLinkedQuestions questionnaireReferences, QuestionnaireDocument document)
        {
            this.container = new Dictionary<Guid, ExportedHeaderItem>();
            this.questionnaireReferences = questionnaireReferences;
            this.autoPropagatedQuestions =
                document.Find<AutoPropagateQuestion>(question => true).ToDictionary(question => question.PublicKey, question => question);
        }

        public void Add(IQuestion question)
        {
            if (this.IsQuestionMultiOption(question))
            {
                if (question.LinkedToQuestionId.HasValue)
                    AddHeadersForLinkedMultiOptions(question);
                else AddHeadersForMultiOptions(question);
            }
            else
                AddHeaderForNotMultiOptions(question);
        }

        public ExportedHeaderItem this[Guid id]
        {
            get
            {
                return !this.container.ContainsKey(id) ? null : this.container[id];
            }
        }

        public IEnumerator<ExportedHeaderItem> GetEnumerator()
        {
            return this.container.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private bool IsQuestionMultiOption(IQuestion question)
        {
            return question is IMultyOptionsQuestion;
        }

        private void AddHeadersForLinkedMultiOptions(IQuestion question)
        {
            this.container.Add(question.PublicKey, new ExportedHeaderItem(question, GetMaxAvailablePropagationCountForLinkedQuestion(question)));
        }

        protected void AddHeaderForNotMultiOptions(IQuestion question)
        {
            this.container.Add(question.PublicKey, new ExportedHeaderItem(question));
        }

        protected void AddHeadersForMultiOptions(IQuestion question)
        {
            var multiOptionQuestion = question as IMultyOptionsQuestion;
            var maxCount = (multiOptionQuestion == null ? null : multiOptionQuestion.MaxAllowedAnswers) ?? question.Answers.Count;
            this.container.Add(question.PublicKey, new ExportedHeaderItem(question, maxCount));
        }

        private int GetMaxAvailablePropagationCountForLinkedQuestion(IQuestion question)
        {
            var questioIdnWhichTriggersPropagation =
                questionnaireReferences.ReferencesOnLinkedQuestions[question.PublicKey].ScopeId;
            return this.autoPropagatedQuestions[questioIdnWhichTriggersPropagation].MaxValue;
        }
    }
}
