using System;
using System.Collections.Generic;
using Android.Views;
using Android.Widget;
using Microsoft.Practices.ServiceLocation;

using WB.Core.GenericSubdomains.Utils.Services;

namespace WB.UI.Capi.Controls.Navigation
{
    public class NavigationListener : Java.Lang.Object, AdapterView.IOnItemSelectedListener
    {
        private readonly IList<NavigationItem> items;

        private static ILogger Logger
        {
            get { return ServiceLocator.Current.GetInstance<ILogger>(); }
        }

        public NavigationListener(IList<NavigationItem> items)
        {
            this.items = items;
        }


        public void OnItemSelected(AdapterView parent, View view, int position, long id)
        {
            try
            {
                this.items[position].Handle(this);
            }
            catch (Exception exception)
            {
                Logger.Warn("Page switch failed", exception);
            }
        }

        public void OnNothingSelected(AdapterView parent)
        {
        }
    }
}