using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails
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