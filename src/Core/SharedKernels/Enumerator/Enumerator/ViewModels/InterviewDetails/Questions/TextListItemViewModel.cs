﻿using System;
using MvvmCross.Core.ViewModels;
using WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions.State;

namespace WB.Core.SharedKernels.Enumerator.ViewModels.InterviewDetails.Questions
{
    public class TextListItemViewModel : MvxNotifyPropertyChanged
    {
        public event EventHandler ItemEdited;

        public event EventHandler ItemDeleted;

        public EnablementViewModel Enablement { get; set; }

        public decimal Value { get; set; }

        private string title;
        public string Title
        {
            get
            {
                return this.title;
            }
            set
            {
                this.title = value;
                this.RaisePropertyChanged();
            }
        }

        private IMvxCommand valueChangeCommand;
        public IMvxCommand ValueChangeCommand
        {
            get { return this.valueChangeCommand ?? (this.valueChangeCommand = new MvxCommand(this.OnItemEdited)); }
        }

        public IMvxCommand DeleteListItemCommand
        {
            get { return new MvxCommand(this.DeleteListItem); }
        }

        private void DeleteListItem()
        {
            if (this.ItemDeleted != null) this.ItemDeleted.Invoke(this, EventArgs.Empty);
        }

        private void OnItemEdited()
        {
            if (this.ItemEdited != null) this.ItemEdited.Invoke(this, EventArgs.Empty);
        }
    }
}