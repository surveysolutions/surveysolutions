using System;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;

namespace WB.Core.BoundedContexts.Capi.Views.InterviewDetails
{
    public class StaticTextViewModel : Cirrious.MvvmCross.ViewModels.MvxViewModel, IQuestionnaireItemViewModel
    {
        public StaticTextViewModel(InterviewItemId publicKey, string text)
        {
            this.PublicKey = publicKey;
            this.Text = text;
        }

        public InterviewItemId PublicKey { get; private set; }
        public string Text { get; private set; }

        public IQuestionnaireItemViewModel Clone(decimal[] propagationVector)
        {
            return new StaticTextViewModel(publicKey: new InterviewItemId(this.PublicKey.Id, propagationVector),
                text: this.Text);
        }

    }
}