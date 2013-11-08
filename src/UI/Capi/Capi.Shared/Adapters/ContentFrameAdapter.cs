// -----------------------------------------------------------------------
// <copyright file="ContentFrameAdapter.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Android.Support.V4.App;
using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Capi.Views.InterviewDetails;
using WB.Core.SharedKernels.DataCollection.ValueObjects.Interview;
using WB.UI.Capi.Shared.Frames;

namespace WB.UI.Capi.Shared.Adapters
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public abstract class ContentFrameAdapter : FragmentStatePagerAdapter
    {
        private readonly InterviewViewModel questionnaire;
        private InterviewItemId? screenId;
        private bool isRoot;
        private IList<InterviewItemId> screensHolder;
        private AbstractScreenChangingFragment[] mFragments;
        public ContentFrameAdapter(FragmentManager fm, InterviewViewModel questionnaire, InterviewItemId? screenId)
            : base(fm)
        {
            this.questionnaire = questionnaire;
            this.screensHolder = (screenId.HasValue
                                     ? questionnaire.Screens[screenId.Value].Siblings
                                     : questionnaire.Chapters.Select(c=>c.ScreenId)).ToList();
            this.screenId = screenId;
            this.isRoot = questionnaire.Chapters.Any(s => s.ScreenId == screenId) || !screenId.HasValue;
            this.mFragments = new AbstractScreenChangingFragment[this.Count];
        }


        public override int Count
        {
            get { return this.screensHolder.Count + (this.isRoot ? 1 : 0); }
        }

        public bool IsRoot
        {
            get { return this.isRoot; }
        }
        public InterviewItemId? ScreenId
        {
            get { return this.screenId; }
        }
        public override Fragment GetItem(int position)
        {
            AbstractScreenChangingFragment fragment = this.mFragments[position];
            if (fragment != null)
                return fragment;
            
            if (position == this.screensHolder.Count && this.isRoot)
            {
                fragment = CreateStatisticsScreen(this.questionnaire.PublicKey);
            }
            else
            {

                var param = this.screensHolder[position];
                var model = this.questionnaire.Screens[param];
                var screenModel = model as QuestionnaireScreenViewModel;
                if (screenModel != null)
                {
                    fragment = CreateContentScreen(screenModel.ScreenId, this.questionnaire.PublicKey);
                }
                else
                {
                    var grid = model as QuestionnaireGridViewModel;
                    if (grid != null)
                    {
                        fragment = CreateRosterScreen(grid.ScreenId, this.questionnaire.PublicKey);
                    }
                }
            }
            if (fragment == null)
                throw new InvalidOperationException();
            
            this.mFragments[position] = fragment;
            return fragment;
        }

        protected abstract GridContentFragment CreateRosterScreen(InterviewItemId screenId, Guid questionnaireId);
        protected abstract ScreenContentFragment CreateContentScreen(InterviewItemId screenId, Guid questionnaireId);
        protected abstract StatisticsContentFragment CreateStatisticsScreen(Guid questionnaireId);

        public override int GetItemPosition(Java.Lang.Object p0)
        {
            return PositionNone;
        }

        public override void DestroyItem(global::Android.Views.ViewGroup p0, int p1, Java.Lang.Object p2)
        {
            var fragment = p2 as Fragment;

            FragmentTransaction trans = fragment.FragmentManager.BeginTransaction();
            trans.Remove(fragment);
            trans.Commit();
            base.DestroyItem(p0, p1, p2);

            if (this.mFragments.Length > p1)
                this.mFragments[p1] = null;
        }

        public int GetScreenIndex(InterviewItemId? screenId)
        {
            if (!screenId.HasValue)
                return this.isRoot ? this.Count - 1 : -1;
            for (int i = 0; i < this.screensHolder.Count; i++)
            {
                if (this.screensHolder[i] == screenId.Value)
                    return i;
            }
            return -1;
        }
        public void UpdateScreenData(InterviewItemId? newScreenId)
        {
            var screenIdNotNull = newScreenId ?? this.questionnaire.Chapters.First().ScreenId;
            this.screensHolder = this.questionnaire.Screens[screenIdNotNull].Siblings.ToList();
            this.screenId = newScreenId;
            this.isRoot = this.questionnaire.Chapters.Any(s => s.ScreenId == screenIdNotNull);
            this.mFragments = new AbstractScreenChangingFragment[this.Count];
        }
    }
}
