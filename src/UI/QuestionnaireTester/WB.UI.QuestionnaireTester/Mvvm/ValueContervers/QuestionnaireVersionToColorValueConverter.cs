using System.Globalization;
using Cirrious.CrossCore;
using Cirrious.CrossCore.Droid.Platform;
using Cirrious.CrossCore.UI;
using Cirrious.MvvmCross.Plugins.Color;
using WB.Core.SharedKernels.DataCollection;
using QuestionnaireVersion = WB.Core.SharedKernel.Structures.Synchronization.Designer.QuestionnaireVersion;

namespace WB.UI.QuestionnaireTester.Mvvm.ValueContervers
{
    public class QuestionnaireVersionToColorValueConverter : MvxColorValueConverter
    {
        protected override MvxColor Convert(object value, object parameter, CultureInfo culture)
        {
            var currentView = Mvx.Resolve<IMvxAndroidCurrentTopActivity>();
            var engineVersion = QuestionnaireVersionProvider.GetCurrentEngineVersion();
            var supportedEngineVersion = new QuestionnaireVersion()
            {
                Major = engineVersion.Major,
                Minor = engineVersion.Minor,
                Patch = engineVersion.Patch
            };
            var templateVersion = value as QuestionnaireVersion;

            var resourceColor = templateVersion != null && templateVersion <= supportedEngineVersion
                ? currentView.Activity.Resources.GetColor(Resource.Color.enabledQuestionnaire)
                : currentView.Activity.Resources.GetColor(Resource.Color.disabledQuestionnaire);

            return new MvxColor(resourceColor.ToArgb());
        }
    }
}
