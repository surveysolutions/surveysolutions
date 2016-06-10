using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.EventBus.Lite;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Events.Interview;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Repositories;
using WB.Core.SharedKernels.Enumerator.Services.Infrastructure;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State
{
    public class FilteredOptionsViewModel: IDisposable
    {
        private readonly AnswerNotifier answerNotifier;
        private readonly IPlainQuestionnaireRepository questionnaireRepository;
        private readonly IStatefulInterviewRepository interviewRepository;

        public IList<CategoricalOption> Options { get; private set; }

        public event EventHandler OptionsChanged;

        public string Filter { get; set; } = String.Empty;

        private Identity questionIdentity;
        private Guid interviewId;

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

        public void Init(string interviewId, Identity entityIdentity)
        {
            if (interviewId == null) throw new ArgumentNullException("interviewId");
            if (entityIdentity == null) throw new ArgumentNullException("entityIdentity");


            var interview = this.interviewRepository.Get(interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaire(interview.QuestionnaireIdentity);

            this.questionIdentity = entityIdentity;
            this.interviewId = interview.Id;

            this.Options = interview.GetFilteredOptionsForQuestion(entityIdentity, null, Filter).ToList();

            if (questionnaire.IsSupportFilteringForOptions(entityIdentity.Id))
            {
                this.answerNotifier.Init(interviewId);
                this.answerNotifier.QuestionAnswered += AnswerNotifierOnQuestionAnswered;
            }
        }

        private void AnswerNotifierOnQuestionAnswered(object sender, EventArgs eventArgs)
        {
            var interview = this.interviewRepository.Get(interviewId.FormatGuid());
            var newOptions = interview.GetFilteredOptionsForQuestion(questionIdentity, null, Filter).ToList();
            var currentOptions = this.Options;

            if (!Enumerable.SequenceEqual(currentOptions, newOptions, new CategoricalOptionEqualityComparer()))
            {
                this.Options = newOptions;
                this.OptionsChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            this.answerNotifier.Dispose();
        }
    }
}