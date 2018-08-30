using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Base;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.Tasks;
using WB.Core.SharedKernels.DataCollection;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.Enumerator.Utils;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State
{
    public class SpecialValuesViewModel : MvxNotifyPropertyChanged, IDisposable
    {
        private readonly FilteredOptionsViewModel optionsViewModel;
        private readonly IMvxMainThreadAsyncDispatcher mvxMainThreadDispatcher;
        private readonly IStatefulInterviewRepository interviewRepository;

        protected SpecialValuesViewModel(){}

        public SpecialValuesViewModel(
            FilteredOptionsViewModel optionsViewModel,
            IMvxMainThreadAsyncDispatcher mvxMainThreadDispatcher,
            IStatefulInterviewRepository interviewRepository) 
        {
            this.optionsViewModel = optionsViewModel;
            this.mvxMainThreadDispatcher = mvxMainThreadDispatcher;
            this.interviewRepository = interviewRepository;
        }

        private bool? isSpecialValue;
        private Identity questionIdentity;
        private string interviewId;
        private IQuestionStateViewModel questionState;
        private HashSet<int> allSpecialValues = new HashSet<int>();

        public CovariantObservableCollection<SingleOptionQuestionOptionViewModel> SpecialValues { get; } = new CovariantObservableCollection<SingleOptionQuestionOptionViewModel>();

        public bool? IsSpecialValue
        {
            get => this.isSpecialValue;
            set
            {
                if (this.isSpecialValue == value) return;
                this.isSpecialValue = value;
                this.RaisePropertyChanged();
            }
        }

        public event EventHandler SpecialValueChanged;
        public event EventHandler SpecialValueRemoved;

        private void SpecialValueSelected(object sender, EventArgs eventArgs)
        {
            var selectedSpecialValue = (SingleOptionQuestionOptionViewModel) sender;
            var previousOption =
                this.SpecialValues.SingleOrDefault(option => option.Selected && option != selectedSpecialValue);

            if (previousOption != null) previousOption.Selected = false;

            this.SpecialValueChanged?.Invoke(selectedSpecialValue, EventArgs.Empty);
        }
        
        public virtual void Init(string interviewId, Identity entityIdentity, IQuestionStateViewModel questionState)
        {
            this.questionIdentity = entityIdentity ?? throw new ArgumentNullException(nameof(entityIdentity));
            this.interviewId = interviewId ?? throw new ArgumentNullException(nameof(interviewId));
            this.questionState = questionState;

            this.optionsViewModel.Init(interviewId, entityIdentity, 200);
            this.UpdateSpecialValuesAsync().WaitAndUnwrapException();

            allSpecialValues = this.optionsViewModel.GetOptions().Select(x => x.Value).ToHashSet();
            if (this.SpecialValues.Any(x => x.Selected))
                IsSpecialValue = true;
        }

        public bool IsSpecialValueSelected(decimal? value)
        {
            if (!value.HasValue)
                return false;

            var intPart = Math.Truncate(value.Value);
            if (intPart != value.Value)
                return false;

            // Double to int conversion can overflow.
            try
            {
               return this.allSpecialValues.Contains(Convert.ToInt32(value.Value));
            }
            catch (OverflowException)
            {
                return false;
            }
        }

        private void RemoveAnswerHandler(object sender, EventArgs e)
        {
            this.SpecialValueRemoved?.Invoke(sender, EventArgs.Empty);
        }

        private async Task UpdateSpecialValuesAsync()
        {
            var interview = this.interviewRepository.Get(interviewId);
            
            List<SingleOptionQuestionOptionViewModel> specialValuesViewModels;

            var integerQuestion = interview.GetIntegerQuestion(this.questionIdentity);
            if (integerQuestion != null)
            {
                specialValuesViewModels = this.optionsViewModel.GetOptions()
                    .Select(model => this.ToViewModel(model, isSelected: integerQuestion.IsAnswered() && model.Value == integerQuestion.GetAnswer().Value))
                    .ToList();
            }
            else
            {
                var doubleQuestion = interview.GetDoubleQuestion(this.questionIdentity);
                specialValuesViewModels = this.optionsViewModel.GetOptions()
                    .Select(model => this.ToViewModel(model, isSelected: doubleQuestion.IsAnswered() && model.Value == doubleQuestion.GetAnswer().Value))
                    .ToList();
            }

            await RemoveSpecialValues();

            if (specialValuesViewModels.Any(x => x.Selected) || !interview.GetQuestion(this.questionIdentity).IsAnswered())
            {
                await this.mvxMainThreadDispatcher.ExecuteOnMainThreadAsync(() =>
                {
                    specialValuesViewModels.ForEach(x => this.SpecialValues.Add(x));
                    this.RaisePropertyChanged(() => this.SpecialValues);
                });
            }
        }

        private SingleOptionQuestionOptionViewModel ToViewModel(CategoricalOption model, bool isSelected)
        {
            var optionViewModel = new SingleOptionQuestionOptionViewModel
            {
                Enablement = this.questionState.Enablement,
                Value = model.Value,
                Title = model.Title,
                Selected = isSelected,
                QuestionState = this.questionState
            };
            optionViewModel.BeforeSelected += this.SpecialValueSelected;
            optionViewModel.AnswerRemoved += this.RemoveAnswerHandler;

            return optionViewModel;
        }

        public async Task ClearSelectionAndShowValues()
        {
            if (SpecialValues.Count == 0 && this.allSpecialValues.Any())
            {
                await UpdateSpecialValuesAsync();
            }
            else
            {
                foreach (var option in this.SpecialValues)
                {
                    option.Selected = false;
                }
            }

            IsSpecialValue = null;
        }

        private async Task RemoveSpecialValues()
        {
            await this.mvxMainThreadDispatcher.ExecuteOnMainThreadAsync(() =>
            {
                this.SpecialValues.ForEach(option =>
                {
                    option.BeforeSelected -= this.SpecialValueSelected;
                    option.AnswerRemoved -= this.RemoveAnswerHandler;
                    option.DisposeIfDisposable();
                });
                this.SpecialValues.Clear();
                this.RaisePropertyChanged(() => this.SpecialValues);
            });
        }

        public async Task SetAnswer(decimal? answeredOrSelectedValue)
        {
            IsSpecialValue = IsSpecialValueSelected(answeredOrSelectedValue);

            if (this.IsSpecialValue == true)
            {
                if (SpecialValues.Count == 0 && this.allSpecialValues.Any())
                {
                    await UpdateSpecialValuesAsync();
                }

                if (answeredOrSelectedValue.HasValue)
                {
                    var selectedOption =
                        this.SpecialValues.FirstOrDefault(x => x.Value == answeredOrSelectedValue.Value);
                    if (selectedOption != null && selectedOption.Selected == false)
                    {
                        selectedOption.Selected = true;
                    }
                }
            }
            else
            {
                await RemoveSpecialValues();
            }
        }

        public IObservableCollection<ICompositeEntity> AsChildren {
            get
            {
                var result = new CompositeCollection<ICompositeEntity>();
                
                result.AddCollection(SpecialValues);
                result.Add(new OptionBorderViewModel(this.questionState, false));
                
                return result;
            }
        }

        public void Dispose()
        {
            this.optionsViewModel.Dispose();

            foreach (var option in this.SpecialValues)
            {
                option.BeforeSelected -= this.SpecialValueSelected;
                option.AnswerRemoved -= this.RemoveAnswerHandler;
            }
        }
    }
}
