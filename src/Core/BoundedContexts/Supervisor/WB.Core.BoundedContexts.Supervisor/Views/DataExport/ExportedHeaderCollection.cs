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
        protected IDictionary<Guid, IEnumerable<ExportedHeaderItem>> container;
        private readonly ReferenceInfoForLinkedQuestions questionnaireReferences;
        private readonly Dictionary<Guid, AutoPropagateQuestion> autoPropagatedQuestions;

        public ExportedHeaderCollection(ReferenceInfoForLinkedQuestions questionnaireReferences, QuestionnaireDocument document)
        {
            this.container = new Dictionary<Guid, IEnumerable<ExportedHeaderItem>>();
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

        private bool IsQuestionMultiOption(IQuestion question)
        {
            return question is IMultyOptionsQuestion;
        }

        private void AddHeadersForLinkedMultiOptions(IQuestion question)
        {
            var headerItems = new List<ExportedHeaderItem>();

            for (int i = 0; i < GetMaxAvailablePropagationCountForLinkedQuestion(question); i++)
            {
                headerItems.Add(new ExportedHeaderItem(question, i));

            }
            this.container.Add(question.PublicKey, headerItems);
        }

        protected void AddHeaderForNotMultiOptions(IQuestion question)
        {
            this.container.Add(question.PublicKey, new ExportedHeaderItem[] { new ExportedHeaderItem(question) });
        }

        protected void AddHeadersForMultiOptions(IQuestion question)
        {
            var headerItems = new List<ExportedHeaderItem>();

            for (int i = 0; i < question.Answers.Count; i++)
            {
                headerItems.Add(new ExportedHeaderItem(question, i));

            }
            this.container.Add(question.PublicKey, headerItems);
        }

        public IEnumerable<Guid> Keys
        {
            get { return this.container.Select(c => c.Key); }
        }

        public IEnumerator<ExportedHeaderItem> GetEnumerator()
        {
            return this.container.SelectMany(c => c.Value).GetEnumerator();
        }

        public IEnumerable<ExportedHeaderItem> GetAvailableHeaderForQuestion(Guid questionId)
        {
            return this.container.Where(c => c.Key == questionId).SelectMany(c => c.Value);
        }

        private int GetMaxAvailablePropagationCountForLinkedQuestion(IQuestion question)
        {
            var questioIdnWhichTriggersPropagation =
                questionnaireReferences.ReferencesOnLinkedQuestions[question.PublicKey].ScopeId;
            return this.autoPropagatedQuestions[questioIdnWhichTriggersPropagation].MaxValue;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
