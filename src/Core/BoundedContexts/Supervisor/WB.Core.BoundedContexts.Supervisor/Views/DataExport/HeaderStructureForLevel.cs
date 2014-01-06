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
    public class HeaderStructureForLevel
    {
        public Guid LevelId { get; private set; }
        public string LevelName { get; private set; }
        public IDictionary<Guid, ExportedHeaderItem> HeaderItems { get; private set; }

        public HeaderStructureForLevel()
        {
            this.HeaderItems = new Dictionary<Guid, ExportedHeaderItem>();
        }

        public HeaderStructureForLevel(
            IEnumerable<IGroup> groupsInLevel, 
            ReferenceInfoForLinkedQuestions referenceInfoForLinkedQuestions,
            Dictionary<Guid, int> maxValuesForRosterSizeQuestions,
            Guid levelId)
            : this()
        {
            this.LevelId = levelId;

            if (!groupsInLevel.Any())
                return;

            this.LevelName = groupsInLevel.First().Title;

            foreach (var rootGroup in groupsInLevel)
            {
                FillHeaderWithQuestionsInsideGroup(rootGroup, referenceInfoForLinkedQuestions, maxValuesForRosterSizeQuestions);
            }
        }

        private void FillHeaderWithQuestionsInsideGroup(IGroup @group, ReferenceInfoForLinkedQuestions referenceInfoForLinkedQuestions,
            Dictionary<Guid, int> maxValuesForRosterSizeQuestions)
        {
            foreach (var groupChild in @group.Children)
            {
                var question = groupChild as IQuestion;
                if (question != null)
                {
                    if (this.IsQuestionMultiOption(question))
                    {
                        if (question.LinkedToQuestionId.HasValue)
                            AddHeadersForLinkedMultiOptions(question, referenceInfoForLinkedQuestions, maxValuesForRosterSizeQuestions);
                        else AddHeadersForMultiOptions(question);
                    }
                    else
                        AddHeaderForNotMultiOptions(question);
                    continue;
                }

                var innerGroup = groupChild as IGroup;
                if (innerGroup != null)
                {
                    //### old questionnaires supporting        //### roster
                    if (innerGroup.Propagated != Propagate.None || innerGroup.IsRoster)
                        continue;
                    FillHeaderWithQuestionsInsideGroup(innerGroup, referenceInfoForLinkedQuestions, maxValuesForRosterSizeQuestions);
                }
            }
        }

        private bool IsQuestionMultiOption(IQuestion question)
        {
            return question is IMultyOptionsQuestion;
        }

        private void AddHeadersForLinkedMultiOptions(IQuestion question, ReferenceInfoForLinkedQuestions referenceInfoForLinkedQuestions,
            Dictionary<Guid, int> maxValuesForRosterSizeQuestions)
        {
            this.HeaderItems.Add(question.PublicKey, new ExportedHeaderItem(question, this.GetRosterSizeForLinkedQuestion(question, referenceInfoForLinkedQuestions, maxValuesForRosterSizeQuestions)));
        }

        protected void AddHeaderForNotMultiOptions(IQuestion question)
        {
            this.HeaderItems.Add(question.PublicKey, new ExportedHeaderItem(question));
        }

        protected void AddHeadersForMultiOptions(IQuestion question)
        {
            var multiOptionQuestion = question as IMultyOptionsQuestion;
            var maxCount = (multiOptionQuestion == null ? null : multiOptionQuestion.MaxAllowedAnswers) ?? question.Answers.Count;
            this.HeaderItems.Add(question.PublicKey, new ExportedHeaderItem(question, maxCount));
        }

        private int GetRosterSizeForLinkedQuestion(IQuestion question, ReferenceInfoForLinkedQuestions referenceInfoForLinkedQuestions,
            Dictionary<Guid, int> maxValuesForRosterSizeQuestions)
        {
            var rosterSizeQuestionId =
                referenceInfoForLinkedQuestions.ReferencesOnLinkedQuestions[question.PublicKey].ScopeId;

            return maxValuesForRosterSizeQuestions[rosterSizeQuestionId];
        }
    }
}
