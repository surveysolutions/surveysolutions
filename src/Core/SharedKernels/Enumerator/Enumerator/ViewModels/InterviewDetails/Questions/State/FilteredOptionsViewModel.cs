using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Aggregates;
using WB.Core.SharedKernels.Enumerator.Repositories;


namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State
{
    public class FilteredOptionsViewModel: IDisposable
    {
        private readonly AnswerNotifier answerNotifier;
        private readonly IQuestionnaireStorage questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;

        private IStatefulInterview interview;
        private IEnumerable<CategoricalOption> Options { get; set; }
        private string Filter { get; set; } = String.Empty;
        private int Count { get; set; } = int.MaxValue;

        public virtual event EventHandler OptionsChanged;

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
            if (questionnaireRepository == null) throw new ArgumentNullException("questionnaireRepository");
            if (interviewRepository == null) throw new ArgumentNullException("interviewRepository");

            this.questionnaireRepository = questionnaireRepository;
            this.interviewRepository = interviewRepository;

            this.answerNotifier = answerNotifier;
        }

        public virtual void Init(string interviewId, Identity entityIdentity)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));


            interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(this.interview.QuestionnaireIdentity, this.interview.Language);

            this.questionIdentity = entityIdentity;

            this.Options = interview.GetFilteredOptionsForQuestion(entityIdentity, null, Filter);

            if (questionnaire.IsSupportFilteringForOptions(entityIdentity.Id))
            {
                this.answerNotifier.Init(interviewId);
                this.answerNotifier.QuestionAnswered += AnswerNotifierOnQuestionAnswered;
            }
        }

        public virtual IEnumerable<CategoricalOption> GetOptions(string filter = "",  int count = int.MaxValue)
        {
            this.Filter = filter;
            this.Count = count;
            this.Options = this.interview.GetFilteredOptionsForQuestion(this.questionIdentity, null, filter)
                                .Take(count)
                                .ToList();
            return Options;
        }


        private void AnswerNotifierOnQuestionAnswered(object sender, EventArgs eventArgs)
        {
            var newOptions = interview.GetFilteredOptionsForQuestion(questionIdentity, null, Filter)
                                .Take(Count)
                                .ToList();

            var currentOptions = this.Options;
            var listOfNewOptions = newOptions.ToList();

            if (!Enumerable.SequenceEqual(currentOptions, listOfNewOptions, new CategoricalOptionEqualityComparer()))
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