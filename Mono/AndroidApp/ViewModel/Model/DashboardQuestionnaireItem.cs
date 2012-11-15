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

namespace AndroidApp.ViewModel.Model
{
    public class DashboardQuestionnaireItem
    {
        public DashboardQuestionnaireItem(Guid publicKey, string status, IList<string> properties)
        {
            PublicKey = publicKey;
            Status = status;
            Properties = properties;
        }

        public Guid PublicKey { get;private set; }
       
        public string Status { get; private set; }
        public IList<string> Properties { get; private set; }
    }
}