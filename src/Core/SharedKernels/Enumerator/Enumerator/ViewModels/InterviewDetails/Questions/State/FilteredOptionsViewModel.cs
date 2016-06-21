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
        private readonly IPlainQuestionnaireRepository questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;

        private IStatefulInterview interview;
        private IEnumerable<CategoricalOption> Options { get; set; }
        private string Filter { get; set; } = String.Empty;

        public virtual event EventHandler OptionsChanged;

        private Identity questionIdentity;

        public bool IsNeedCompareOptionsOnChanges { get; set; } = true;


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

        public FilteredOptionsViewModel (IPlainQuestionnaireRepository questionnaireRepository,
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
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");


            interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity);

            this.questionIdentity = entityIdentity;

            this.Options = interview.GetFilteredOptionsForQuestion(entityIdentity, null, Filter);

            if (questionnaire.IsSupportFilteringForOptions(entityIdentity.Id))
            {
                this.answerNotifier.Init(interviewId);
                this.answerNotifier.QuestionAnswered += AnswerNotifierOnQuestionAnswered;
            }
        }

        public virtual IEnumerable<CategoricalOption> GetOptions(string filter = "")
        {
            this.Filter = filter;
            var filteredOptionsForQuestion = this.interview.GetFilteredOptionsForQuestion(this.questionIdentity, null, filter);
            this.Options = this.IsNeedCompareOptionsOnChanges ? filteredOptionsForQuestion.ToList() : filteredOptionsForQuestion;
            return Options;
        }


        private void AnswerNotifierOnQuestionAnswered(object sender, EventArgs eventArgs)
        {
            var newOptions = interview.GetFilteredOptionsForQuestion(questionIdentity, null, Filter);

            if (!this.IsNeedCompareOptionsOnChanges)
            {
                this.Options = newOptions;
                this.OptionsChanged?.Invoke(this, EventArgs.Empty);
                return;
            }

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