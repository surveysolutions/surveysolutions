using System.Globalization;
using Cirrious.CrossCore.Droid.Platform;
using Cirrious.CrossCore.UI;
using Cirrious.MvvmCross.Plugins.Color;
using Microsoft.Practices.ServiceLocation;
using WB.Core.BoundedContexts.QuestionnaireTester.ViewModels.QuestionsViewModels;

namespace WB.UI.QuestionnaireTester.Converters
{
    public class GetGroupInfoTextColorByStatusConverter : MvxColorValueConverter<GroupStatus>
    {
        private IMvxAndroidCurrentTopActivity CurrentAcrivity
        {
            get { return ServiceLocator.Current.GetInstance<IMvxAndroidCurrentTopActivity>(); }
        }

        protected override MvxColor Convert(GroupStatus value, object parameter, CultureInfo culture)
        {
            int colorResourceId;
            switch (value)
            {
                case GroupStatus.Started:
                    colorResourceId = Resource.Color.group_started;
                    break;
                case GroupStatus.Completed:
                    colorResourceId = Resource.Color.group_completed;
                    break;
                case GroupStatus.HasInvlidAnswers:
                    colorResourceId = Resource.Color.group_with_invalid_answers;
                    break;
                default:
                    colorResourceId = Resource.Color.group_not_started;
                    break;
            }

            return new MvxColor(this.CurrentAcrivity.Activity.Resources.GetColor(colorResourceId));
        }
    }
}