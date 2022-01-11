#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WB.Core.GenericSubdomains.Portable.Services;
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
        private readonly ILogger logger;

        private IQuestionnaire questionnaire = null!;
        private List<CategoricalOption>? options;
        private string Filter { get; set; } = String.Empty;
        public int Count { get; protected set; } = 200;

        public virtual event Func<object, EventArgs, Task>? OptionsChanged;

        public int? ParentValue { set; get; }

        private Identity questionIdentity = null!;
        private int[]? excludedOptionIds;
        private string interviewId = null!;

        private class CategoricalOptionEqualityComparer : IEqualityComparer<CategoricalOption>
        {
            public bool Equals(CategoricalOption x, CategoricalOption y)
            {
                return x.Title == y.Title && x.Value == y.Value;
            }

            public int GetHashCode(CategoricalOption? obj)
            {
                return obj?.Value.GetHashCode() ?? 0;
            }
        }

        protected FilteredOptionsViewModel()
        {
            this.logger = null!;
            this.questionnaire = null!;
            this.interviewRepository = null!;
            this.questionnaireRepository = null!;
            this.answerNotifier = null!;
        }

        public FilteredOptionsViewModel (IQuestionnaireStorage questionnaireRepository,
            IStatefulInterviewRepository interviewRepository,
            AnswerNotifier answerNotifier,
            ILogger logger)
        {
            this.logger = logger;
            this.questionnaireRepository = questionnaireRepository ?? throw new ArgumentNullException(nameof(questionnaireRepository));
            this.interviewRepository = interviewRepository ?? throw new ArgumentNullException(nameof(interviewRepository));

            this.answerNotifier = answerNotifier;
        }

        public virtual void Init(string interviewId, Identity entityIdentity, int? maxCountToLoad = null)
        {
            if (interviewId == null) throw new ArgumentNullException(nameof(interviewId));
            if (entityIdentity == null) throw new ArgumentNullException(nameof(entityIdentity));

            if (maxCountToLoad.HasValue)
                this.Count = maxCountToLoad.Value;

            this.interviewId = interviewId;
            var interview = this.interviewRepository.GetOrThrow(interviewId);
            var questionnaire = this.questionnaireRepository.GetQuestionnaireOrThrow(interview.QuestionnaireIdentity, interview.Language);

            this.questionIdentity = entityIdentity;

            if (!questionnaire.IsQuestionFilteredCombobox(entityIdentity.Id))
            {
                this.options = interview.GetTopFilteredOptionsForQuestion(entityIdentity, ParentValue, Filter, this.Count, null);
            }

            if (questionnaire.IsSupportFilteringForOptions(entityIdentity.Id))
            {
                this.answerNotifier.Init(interviewId);
                this.answerNotifier.QuestionAnswered += AnswerNotifierOnQuestionAnswered;
            }

            if (questionnaire.IsQuestionCascading(entityIdentity.Id))
            {
                var cascadingQuestionParentId = questionnaire.GetCascadingQuestionParentId(entityIdentity.Id);
                if (!cascadingQuestionParentId.HasValue) throw new NullReferenceException($"Parent of cascading question {entityIdentity} is missing");
            
                var parentRosterVector = entityIdentity.RosterVector.Take(questionnaire.GetRosterLevelForEntity(cascadingQuestionParentId.Value)).ToArray();

                var parentQuestionIdentity = new Identity(cascadingQuestionParentId.Value, parentRosterVector);

                var parentSingleOptionQuestion = interview.GetSingleOptionQuestion(parentQuestionIdentity);
                if (parentSingleOptionQuestion.IsAnswered())
                {
                    this.ParentValue = parentSingleOptionQuestion.GetAnswer().SelectedValue;
                }
            }
        }

        public virtual List<CategoricalOption> GetOptions(string filter = "", int[]? excludedOptionIdsArg = null, int? count = null)
        {
            this.Filter = filter;
            this.excludedOptionIds = excludedOptionIdsArg;
            var interview = this.interviewRepository.GetOrThrow(interviewId);
            var optionsFromInterview = interview.GetTopFilteredOptionsForQuestion(this.questionIdentity, ParentValue, filter, count ?? this.Count, excludedOptionIdsArg)
                ?.ToList();
            this.options = optionsFromInterview ?? new List<CategoricalOption>();
            return options;
        }

        public virtual CategoricalOption GetOptionByTextValue(string textValue)
        {
            var interview = this.interviewRepository.GetOrThrow(interviewId);
            return interview.GetOptionForQuestionWithFilter(this.questionIdentity, textValue, ParentValue);
        }

        public virtual CategoricalOption GetAnsweredOption(int answer)
        {
            var interview = this.interviewRepository.GetOrThrow(interviewId);
            return interview.GetOptionForQuestionWithoutFilter(this.questionIdentity, answer, ParentValue);
        }


        private void AnswerNotifierOnQuestionAnswered(object sender, EventArgs eventArgs)
        {
            //temporary fix for KP-13068
            //if view model was created for item in roster
            //and trigger is changed and item is gone
            // getting options could fail
            var interview = this.interviewRepository.GetOrThrow(interviewId);
            if (interview.GetQuestion(questionIdentity) == null)
            {
                logger.Warn($"Trying to reload options on question {questionIdentity} that doesn't exist in interview {interview.Id}");
                return;
            }

            if (questionnaire.IsQuestionFilteredCombobox(questionIdentity.Id))
            {
                // for combo always drop list of options
                if (this.options != null)
                    this.OptionsChanged?.Invoke(this, EventArgs.Empty);
                return;
            }
            
            var listOfNewOptions = interview.GetTopFilteredOptionsForQuestion(questionIdentity, ParentValue, Filter, Count, this.excludedOptionIds).ToList(); 

            var existingOptions = this.options;
            if (existingOptions == null || !listOfNewOptions.SequenceEqual(existingOptions, new CategoricalOptionEqualityComparer()))
            {
                this.options = listOfNewOptions;
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
