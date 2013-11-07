using System;

namespace WB.Core.BoundedContexts.CAPI.Views.InterviewDetails
{
    public class LinkedAnswerViewModel : Cirrious.MvvmCross.ViewModels.MvxViewModel, ICloneable
    {
        public LinkedAnswerViewModel(int[] propagationVector, string title)
        {
            this.PropagationVector = propagationVector;
            this.Title = title;
        }

        public int[] PropagationVector { get; private set; }
        public string Title { get; private set; }

        public object Clone()
        {
            return new LinkedAnswerViewModel(this.PropagationVector, this.Title);
        }
    }
}