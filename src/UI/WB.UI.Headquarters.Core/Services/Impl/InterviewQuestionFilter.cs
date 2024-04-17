using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Implementation.Aggregates.InterviewEntities;
using WB.UI.Headquarters.API.WebInterview;

namespace WB.UI.Headquarters.Services.Impl
{
    public class InterviewQuestionFilter
    {
        private readonly Func<InterviewQuestionFilter, bool> rule;
        private readonly IQuestionnaire questionnaire;
        private readonly HashSet<Identity> flaggedQuestionsSet;
        private HashSet<FilterOption> filters;
        private IInterviewTreeNode node;

        public InterviewQuestionFilter(HashSet<Identity> flaggedQuestionsSetQuestions,
            Func<InterviewQuestionFilter, bool> rule, IQuestionnaire questionnaire)
        {
            this.rule = rule;
            this.questionnaire = questionnaire;
            this.flaggedQuestionsSet = flaggedQuestionsSetQuestions;
            
        }

        public void SetNode(IInterviewTreeNode questionToEvaluate)
        {
            this.node = questionToEvaluate;
        }

        public bool Evaluate(HashSet<FilterOption> filters)
        {
            this.filters = filters;

            return rule(this);
        }

        public bool Has(params FilterOption[] options)
        {
            return options.Any(filters.Contains);
        }

        /// <summary>
        /// Apply or rule on options if any of this options supplied for filtering
        /// </summary>
        /// <param name="options">Options to apply OR rule</param>
        public bool Or(params FilterOption[] options)
        {
            if (!Has(options)) return true;

            return options.Any(o => Is(o, false));
        }

        public bool Evaluate(FilterOption option, bool @default)
        {
            if (node.IsDisabled()) return false;

            switch (node)
            {
                case InterviewTreeStaticText staticText:
                    switch (option)
                    {
                        case FilterOption.WithComments: return false;
                        case FilterOption.Invalid: return !staticText.IsValid;
                        case FilterOption.Valid: return staticText.IsValid;
                        default:
                            return @default;
                    }
                case InterviewTreeQuestion question:
                    switch (option)
                    {
                        case FilterOption.Flagged: return flaggedQuestionsSet.Contains(question.Identity);
                        case FilterOption.NotFlagged: return !flaggedQuestionsSet.Contains(question.Identity);
                        case FilterOption.WithComments: return question.AnswerComments.Any();
                        case FilterOption.Invalid: return !question.IsValid;
                        case FilterOption.Valid: return question.IsValid;
                        case FilterOption.Answered: return question.IsAnswered();
                        case FilterOption.NotAnswered: return !question.IsAnswered();
                        case FilterOption.ForSupervisor: return question.IsSupervisors;
                        case FilterOption.ForInterviewer: return question.IsInterviewer && !question.IsReadonly;
                        case FilterOption.CriticalQuestions: return questionnaire.IsCritical(question.Identity.Id);
                        default:
                            return @default;
                    }
            }

            return @default;
        }

        public bool Is(FilterOption option, bool @default = true)
        {
            if (!filters.Contains(option)) return @default;

            return Evaluate(option, @default);
        }
    }
}
