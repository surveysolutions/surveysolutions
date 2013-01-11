using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidApp.ViewModel.QuestionnaireDetails;
using Cirrious.MvvmCross.Commands;
using Cirrious.MvvmCross.ViewModels;
namespace AndroidApp.ViewModel.Model
{
    public class DashboardQuestionnaireItem : MvxViewModel
    {
        public DashboardQuestionnaireItem(Guid publicKey, string status, IList<FeaturedItem> properties)
        {
            PublicKey = publicKey;
            Status = status;
            Properties = properties;
        }

        public Guid PublicKey { get;private set; }
       
        public string Status { get; private set; }
        public IList<FeaturedItem> Properties { get; private set; }

        public ICommand ViewDetailCommand
        {
            get
            {
                return new MvxRelayCommand(() =>
                    {
                        RequestNavigate<QuestionnaireScreenViewModel>(new { completeQuestionnaireId = PublicKey.ToString() });
                    });
            }
        }
    }
}