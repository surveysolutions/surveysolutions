using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidApp.ViewModel.Input;
using AndroidApp.ViewModel.Model;
using Main.Core.View;
using Main.Core.View.CompleteQuestionnaire;

namespace AndroidApp.ViewModel.Factory
{
    public class DashboardFactory : IViewFactory<DashboardInput, DashboardModel>
    {


        #region Implementation of IViewFactory<DashboardInput,DashboardModel>

        public DashboardModel Load(DashboardInput input)
        {
            List<DashboardQuestionnaireItem> items = new List<DashboardQuestionnaireItem>();
         /*    var propertiesTitles = new List<string>();
             for (int j = 0; j < 4; j++)
             {
                 propertiesTitles.Add("pTitle" + j);
             }*/
            for (int i = 0; i < 5; i++)
            {
                var properties = new List<FeaturedItem>();
                for (int j = 0; j < 4; j++)
                {
                    properties.Add(new FeaturedItem(Guid.NewGuid(),"pTitle" + j,"p"+j));
                }
                var item = new DashboardQuestionnaireItem(Guid.NewGuid(),"status" + i, properties);
                items.Add(item);

            }
            var retval =
                new DashboardModel(
                    new[]
                        {
                            new DashboardSurveyItem(Guid.NewGuid(), "Super Servey1", items),
                            new DashboardSurveyItem(Guid.NewGuid(), "Super Servey2",  items),
                             new DashboardSurveyItem(Guid.NewGuid(), "Super Servey3", items)
                        });
            return retval;
        }

        #endregion
    }
}