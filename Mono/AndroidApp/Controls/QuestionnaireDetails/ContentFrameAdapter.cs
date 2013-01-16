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
        private ItemPublicKey? screenId;
        private bool isRoot;
        private IList<QuestionnaireNavigationPanelItem> screensHolder;
        IDictionary<int,Fragment> hash=new Dictionary<int, Fragment>();
        public ContentFrameAdapter(FragmentManager fm, IQuestionnaireViewModel initScreen)
            : base(fm)
        {
            this.questionnaireId = initScreen.QuestionnaireId;
            UpdateScreenData(initScreen);
          
        }
        public override int Count
        {
            get { return screensHolder.Count + (isRoot ? 1 : 0); }
        }

        public bool IsRoot
        {
            get { return isRoot; }
        }

        public override Fragment GetItem(int position)
        {
            if (hash.ContainsKey(position))
                return hash[position];
            Fragment fragment = null;
            if (position == screensHolder.Count && isRoot)
            {
                fragment = new StatisticsContentFragment(questionnaireId);
            }
            else
            {

                var param = screensHolder[position];
                var model = CapiApplication.LoadView<QuestionnaireScreenInput, IQuestionnaireViewModel>(
                    new QuestionnaireScreenInput(questionnaireId, param.ScreenPublicKey));
                var screenModel = model as QuestionnaireScreenViewModel;
                if (screenModel != null)
                {
                    fragment =new  ScreenContentFragment(screenModel);
                    ((ScreenContentFragment) fragment).ScreenChanged +=
                        new EventHandler<ScreenChangedEventArgs>(fragment_ScreenChanged);
                }
                var grid = model as QuestionnaireGridViewModel;
                if (grid != null)
                {
                    fragment = new GridContentFragment(grid);
                    ((GridContentFragment)fragment).ScreenChanged +=
                      new EventHandler<ScreenChangedEventArgs>(fragment_ScreenChanged);
                }

            }
            if (fragment == null)
                throw new InvalidOperationException();
            hash.Add(position, fragment);
            return fragment;
        }
        public override int GetItemPosition(Java.Lang.Object p0)
        {
            if (hash.Any(h => h.Value == p0))
                return PositionUnchanged;
            return PositionNone;
        }
        void fragment_ScreenChanged(object sender, ScreenChangedEventArgs e)
        {
            OnScreenChanged(e);
        }
        public int GetScreenIndex(ItemPublicKey? screenId)
        {
            if (!screenId.HasValue)
                return isRoot ? Count - 1 : -1;
         //   int result = 0;
            for (int i = 0; i < screensHolder.Count; i++)
            {
                if (screensHolder[i].ScreenPublicKey == screenId.Value)
                    return i;
                //  result++;
            }
            return -1;
        }
        public void UpdateScreenData(IQuestionnaireViewModel initScreen)
        {
          
            this.screensHolder = initScreen.Siblings;
            this.screenId = initScreen.ScreenId;
            this.isRoot = initScreen.Chapters.Any(s => s.ScreenPublicKey == initScreen.ScreenId);
            this.NotifyDataSetChanged();
            
        }
        public override void NotifyDataSetChanged()
        {
            hash = new Dictionary<int, Fragment>();
            base.NotifyDataSetChanged();
            
        }

        protected void OnScreenChanged(ScreenChangedEventArgs evt)
        {
            var handler = ScreenChanged;
            if (handler != null)
                handler(this, evt);
        }

        public event EventHandler<ScreenChangedEventArgs> ScreenChanged;
    }
}
