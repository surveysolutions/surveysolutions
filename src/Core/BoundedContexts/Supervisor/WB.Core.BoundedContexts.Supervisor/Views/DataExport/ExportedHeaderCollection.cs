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
            this.maxValuesForRosterSizeQuestions = GetMaxValuesForRosterSizeQuestions(document);
        }

        private static Dictionary<Guid, int> GetMaxValuesForRosterSizeQuestions(QuestionnaireDocument document)
        { 
            IEnumerable<IAutoPropagateQuestion> autoPropagateQuestions = document.Find<IAutoPropagateQuestion>(question => true);

            var rosterGroups = document.Find<IGroup>(@group => @group.IsRoster && @group.RosterSizeQuestionId.HasValue);

            var fixedRosterGroups =
                document.Find<IGroup>(@group => @group.IsRoster && @group.RosterSizeSource == RosterSizeSourceType.FixedTitles);
            
            IEnumerable<INumericQuestion> rosterSizeNumericQuestions =
                rosterGroups.Select(@group => document.Find<INumericQuestion>(@group.RosterSizeQuestionId.Value))
                    .Where(question => question != null && question.MaxValue.HasValue).Distinct();

            IEnumerable<IMultyOptionsQuestion> rosterSizeMultyOptionQuestions =
                rosterGroups.Select(@group => document.Find<IMultyOptionsQuestion>(@group.RosterSizeQuestionId.Value))
                    .Where(question => question != null);

            var collectedMaxValues = new Dictionary<Guid, int>();

            foreach (IAutoPropagateQuestion autoPropagateQuestion in autoPropagateQuestions)
            {
                collectedMaxValues.Add(autoPropagateQuestion.PublicKey, autoPropagateQuestion.MaxValue);
            }

            foreach (INumericQuestion rosterSizeNumericQuestion in rosterSizeNumericQuestions)
            {
                collectedMaxValues.Add(rosterSizeNumericQuestion.PublicKey, rosterSizeNumericQuestion.MaxValue.Value);
            }

            foreach (IMultyOptionsQuestion rosterSizeMultyOptionQuestion in rosterSizeMultyOptionQuestions)
            {
                collectedMaxValues.Add(rosterSizeMultyOptionQuestion.PublicKey, rosterSizeMultyOptionQuestion.Answers.Count);
            }

            foreach (IGroup fixedRosterGroup in fixedRosterGroups)
            {
                collectedMaxValues.Add(fixedRosterGroup.PublicKey, fixedRosterGroup.RosterFixedTitles.Length);
            }

            return collectedMaxValues;
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
            var parentFixedRoster = GetParentAsRosterByFixedTitles(question);
            //return parentFixedRoster == null
            //    ? this.maxValuesForRosterSizeQuestions[rosterSizeQuestionId]
            //    : parentFixedRoster.RosterFixedTitles.Length;
            return this.maxValuesForRosterSizeQuestions[rosterSizeQuestionId];
        }

        private IGroup GetParentAsRosterByFixedTitles(IQuestion question)
        {
            return null;
        }
    }
}
