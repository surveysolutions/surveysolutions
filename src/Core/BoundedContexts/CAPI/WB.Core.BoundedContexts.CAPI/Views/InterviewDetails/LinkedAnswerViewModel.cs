using System;

namespace WB.Core.BoundedContexts.Capi.Views.InterviewDetails
{
    public class LinkedAnswerViewModel : Cirrious.MvvmCross.ViewModels.MvxViewModel, ICloneable
    {
        public LinkedAnswerViewModel(decimal[] propagationVector, string title)
        {
            this.PropagationVector = propagationVector;
            this.Title = title;
        }

        public decimal[] PropagationVector { get; private set; }
        public string Title { get; private set; }

        public object Clone()
        {
            return new LinkedAnswerViewModel(this.PropagationVector, this.Title);
        }
    }
}