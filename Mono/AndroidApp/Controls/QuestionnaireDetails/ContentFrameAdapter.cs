// -----------------------------------------------------------------------
// <copyright file="ContentFrameAdapter.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Android.Support.V4.App;
using AndroidApp.ViewModel.QuestionnaireDetails;

namespace AndroidApp.Controls.QuestionnaireDetails
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ContentFrameAdapter : FragmentPagerAdapter
    {
        private readonly Guid questionnaireId;
        private IList<QuestionnaireNavigationPanelItem> screensHolder;

        public ContentFrameAdapter(FragmentManager fm, Guid questionnaireId, Guid screenId, Guid? propagationKey)
            : base(fm)
        {
            this.questionnaireId = questionnaireId;
            this.screensHolder = CapiApplication.LoadView<QuestionnaireScreenInput, QuestionnaireScreenViewModel>(
                new QuestionnaireScreenInput(questionnaireId, screenId, propagationKey)).Siblings;
        }
        public ContentFrameAdapter(FragmentManager fm, QuestionnaireScreenViewModel initScreen)
            : base(fm)
        {
            this.questionnaireId = initScreen.QuestionnaireId;
            this.screensHolder = initScreen.Siblings;
        }
        public override int Count
        {
            get { return screensHolder.Count + 1; }
        }

        public override Fragment GetItem(int position)
        {
            if(position==screensHolder.Count)
            {
                return StatisticsContentFragment.NewInstance(questionnaireId);
            }

            var param = screensHolder[position];
            var model = CapiApplication.LoadView<QuestionnaireScreenInput, QuestionnaireScreenViewModel>(
                new QuestionnaireScreenInput(questionnaireId, param.ScreenPublicKey, null));
            return ScreenContentFragment.NewInstance(model);
        }

    }
}
