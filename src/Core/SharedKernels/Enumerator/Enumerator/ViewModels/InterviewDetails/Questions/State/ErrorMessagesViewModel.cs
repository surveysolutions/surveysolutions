using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using MvvmCross.ViewModels;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Enumerator.Properties;

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

        private string caption;
        public string Caption
        {
            get => this.caption;
            set => this.RaiseAndSetIfChanged(ref this.caption, value);
        }

        public ObservableCollection<ErrorMessageViewModel> ValidationErrors { get; } = new ObservableCollection<ErrorMessageViewModel>();
        
        public void ChangeValidationErrors(IEnumerable<string> errors)
        {
            this.ValidationErrors.ForEach(x => x.Dispose());
            this.ValidationErrors.Clear();

            foreach (string error in errors)
            {
                var errorViewModel = this.dynamicTextViewModelFactory.CreateErrorMessage();
                // default test if error message is empty
                var errorText = string.IsNullOrWhiteSpace(error)
                    ? UIResources.Error
                    : error;

                errorViewModel.InitAsStatic(errorText);

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