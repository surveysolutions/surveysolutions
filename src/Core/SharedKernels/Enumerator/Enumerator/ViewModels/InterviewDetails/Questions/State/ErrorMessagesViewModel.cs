using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MvvmCross.Core.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.DataCollection;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State
{
    public class ErrorMessagesViewModel : MvxNotifyPropertyChanged,
        IDisposable
    {
        private readonly IDynamicTextViewModelFactory dynamicTextViewModelFactory;

        public ErrorMessagesViewModel(IDynamicTextViewModelFactory dynamicTextViewModelFactory)
        {
            this.dynamicTextViewModelFactory = dynamicTextViewModelFactory;
        }

        public void Init(string interviewId, Identity entityIdentity)
        {
        }

        private string caption;
        public string Caption
        {
            get { return this.caption; }
            set { this.RaiseAndSetIfChanged(ref this.caption, value); }
        }

        public ObservableCollection<DynamicTextViewModel> ValidationErrors { get; } = new ObservableCollection<DynamicTextViewModel>();
        
        public void ChangeValidationErrors(IEnumerable<string> errors)
        {
            this.ValidationErrors.ForEach(x => x.Dispose());
            this.ValidationErrors.Clear();

            foreach (string error in errors)
            {
                var errorViewModel = this.dynamicTextViewModelFactory.CreateDynamicTextViewModel();
                errorViewModel.InitAsStatic(error);

                this.ValidationErrors.Add(errorViewModel);
            }
        }

        public void Dispose()
        {
            foreach (var dynamicTextViewModel in this.ValidationErrors)
            {
                dynamicTextViewModel.Dispose();
            }
        }
    }
}