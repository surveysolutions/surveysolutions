﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Aggregates;
using WB.Core.SharedKernels.DataCollection.Repositories;


namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State
{
    public class FilteredOptionsViewModel: IDisposable
    {
        private readonly AnswerNotifier answerNotifier;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;

        private IStatefulInterview interview;
        private List<CategoricalOption> Options { get; set; }
        private string Filter { get; set; } = String.Empty;
        public int Count { get; protected set; } = 200;

        public virtual event Func<object, EventArgs, Task> OptionsChanged;

        public int? ParentValue { set; get; }

        private Identity questionIdentity;

        private class CategoricalOptionEqualityComparer : IEqualityComparer<CategoricalOption>
        {
            public bool Equals(CategoricalOption x, CategoricalOption y)
            {
                return x.Title == y.Title && x.Value == y.Value;
            }

            public int GetHashCode(CategoricalOption obj)
            {
                return obj?.Value.GetHashCode() ?? 0;
            }
        }

        protected FilteredOptionsViewModel() { }

        public FilteredOptionsViewModel (IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            AnswerNotifier answerNotifier)
        {
            if (questionnaireRepository == null) throw new ArgumentNullException(nameof(questionnaireRepository));
            if (interviewRepository == null) throw new ArgumentNullException(nameof(interviewRepository));

            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;

            this.answerNotifier = answerNotifier;
        }

        public virtual void Init(string interviewId, Identity entityIdentity, int? maxCountToLoad = null)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));

            if (maxCountToLoad.HasValue)
                this.Count = maxCountToLoad.Value;

            interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(this.interview.QuestionnaireIdentity, this.interview.Language);

            this.questionIdentity = entityIdentity;

            if (!questionnaire.IsQuestionFilteredCombobox(entityIdentity.Id))
            {
                this.Options = interview.GetTopFilteredOptionsForQuestion(entityIdentity, ParentValue, Filter, this.Count);
            }

            if (questionnaire.IsSupportFilteringForOptions(entityIdentity.Id))
            {
                this.answerNotifier.Init(interviewId);
                this.answerNotifier.QuestionAnswered += AnswerNotifierOnQuestionAnswered;
            }
        }

        public virtual List<CategoricalOption> GetOptions(string filter = "")
        {
            this.Filter = filter;
            this.Options = this.interview.GetTopFilteredOptionsForQuestion(this.questionIdentity, ParentValue, filter, this.Count).ToList();
            return Options;
        }

        public virtual CategoricalOption GetAnsweredOption(int answer)
            => this.interview.GetOptionForQuestionWithoutFilter(this.questionIdentity, answer, ParentValue);


        private void AnswerNotifierOnQuestionAnswered(object sender, EventArgs eventArgs)
        {
            var newOptions = interview.GetTopFilteredOptionsForQuestion(questionIdentity, ParentValue, Filter, Count)
                                .ToList();

            var listOfNewOptions = newOptions.ToList();

            var existingOptions = this.Options;
            if (existingOptions == null || !listOfNewOptions.SequenceEqual(existingOptions, new CategoricalOptionEqualityComparer()))
            {
                this.Options = listOfNewOptions;
                this.OptionsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            this.answerNotifier.QuestionAnswered -= AnswerNotifierOnQuestionAnswered;
            this.answerNotifier.Dispose();
        }
    }
}
