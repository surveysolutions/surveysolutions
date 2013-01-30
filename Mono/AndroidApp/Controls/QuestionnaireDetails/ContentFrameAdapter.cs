// -----------------------------------------------------------------------
// <copyright file="ContentFrameAdapter.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Views;
using AndroidApp.Events;
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
    public class ContentFrameAdapter : FragmentStatePagerAdapter
    {
        private readonly Guid questionnaireId;
        private readonly ViewPager target;
        private ItemPublicKey? screenId;
        private bool isRoot;
        private IList<ItemPublicKey> screensHolder;
        public ContentFrameAdapter(FragmentManager fm, IQuestionnaireViewModel initScreen, ViewPager target)
            : base(fm)
        {
            this.questionnaireId = initScreen.QuestionnaireId;
            this.target = target;
            this.screensHolder = initScreen.Siblings.ToList();
            this.screenId = initScreen.ScreenId;
            this.isRoot = initScreen.Chapters.Any(s => s.ScreenId == initScreen.ScreenId);
            this.target.Adapter = this;
        }


        public override int Count
        {
            get { return screensHolder.Count + (isRoot ? 1 : 0); }
        }

        public bool IsRoot
        {
            get { return isRoot; }
        }
        public ItemPublicKey? ScreenId
        {
            get { return screenId; }
        }
        public override Fragment GetItem(int position)
        {
            Fragment fragment = null;
            if (position == screensHolder.Count && isRoot)
            {
                fragment = new StatisticsContentFragment(questionnaireId);
            }
            else
            {

                var param = screensHolder[position];
                var model = CapiApplication.LoadView<QuestionnaireScreenInput, IQuestionnaireViewModel>(
                    new QuestionnaireScreenInput(questionnaireId, param));
                var screenModel = model as QuestionnaireScreenViewModel;
                if (screenModel != null)
                {
                    fragment = new ScreenContentFragment(screenModel);
                }
                var grid = model as QuestionnaireGridViewModel;
                if (grid != null)
                {
                    fragment = new GridContentFragment(grid);
                }

            }
            if (fragment == null)
                throw new InvalidOperationException();
            return fragment;
        }

        public override int GetItemPosition(Java.Lang.Object p0)
        {
            return PositionNone;
        }
        public int GetScreenIndex(ItemPublicKey? screenId)
        {
            if (!screenId.HasValue)
                return isRoot ? Count - 1 : -1;
            for (int i = 0; i < screensHolder.Count; i++)
            {
                if (screensHolder[i] == screenId.Value)
                    return i;
            }
            return -1;
        }
        public void UpdateScreenData(IQuestionnaireViewModel initScreen, ItemPublicKey? newScreenId)
        {
            this.screensHolder = initScreen.Siblings.ToList();
            this.screenId = initScreen.ScreenId;
            this.isRoot = initScreen.Chapters.Any(s => s.ScreenId == initScreen.ScreenId);
            this.NotifyDataSetChanged();
            target.CurrentItem = this.GetScreenIndex(newScreenId);
        }
    }
}
