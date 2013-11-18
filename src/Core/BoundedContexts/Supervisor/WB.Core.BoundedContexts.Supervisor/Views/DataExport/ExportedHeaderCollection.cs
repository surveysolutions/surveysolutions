using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Supervisor.Views.Questionnaire;

namespace WB.Core.BoundedContexts.Supervisor.Views.DataExport
{
    public class ExportedHeaderCollection : IEnumerable<ExportedHeaderItem>
    {
        protected IDictionary<Guid, ExportedHeaderItem> container;
        private readonly ReferenceInfoForLinkedQuestions questionnaireReferences;
        private readonly Dictionary<Guid, int> maxValuesForRosterSizeQuestions;

        public ExportedHeaderCollection(ReferenceInfoForLinkedQuestions questionnaireReferences, QuestionnaireDocument document)
        {
            this.container = new Dictionary<Guid, ExportedHeaderItem>();
            this.questionnaireReferences = questionnaireReferences;
            this.maxValuesForRosterSizeQuestions =
                document.Find<IAutoPropagateQuestion>(question => true)
                    .ToDictionary(question => question.PublicKey, question => question.MaxValue);

            var rosterSizeQuestionIds =
                document.Find<IGroup>(group => group.IsRoster && group.RosterSizeQuestionId.HasValue)
                    .Select(group => group.RosterSizeQuestionId.Value);

            document.Find<IQuestion>(question => rosterSizeQuestionIds.Contains(question.PublicKey))
                .ToList()
                .ForEach(this.addMaxValueToMaxValuesForRosterSizeQuestionsIfQuestionIsRosterSizeQuestion);
        }

        private void addMaxValueToMaxValuesForRosterSizeQuestionsIfQuestionIsRosterSizeQuestion(IQuestion question)
        {
            var numericQuestion = question as INumericQuestion;
            if (numericQuestion != null)
            {
                this.maxValuesForRosterSizeQuestions.Add(question.PublicKey, numericQuestion.MaxValue.Value);
            }
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
            this.container.Add(question.PublicKey, new ExportedHeaderItem(question, this.GetRosterSizeForLinkedQuestion(question)));
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

        private int GetRosterSizeForLinkedQuestion(IQuestion question)
        {
            var rosterSizeQuestionId =
                questionnaireReferences.ReferencesOnLinkedQuestions[question.PublicKey].ScopeId;
            return this.maxValuesForRosterSizeQuestions[rosterSizeQuestionId];
        }
    }
}
