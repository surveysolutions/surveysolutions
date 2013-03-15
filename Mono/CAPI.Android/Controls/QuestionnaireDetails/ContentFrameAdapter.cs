// -----------------------------------------------------------------------
// <copyright file="ContentFrameAdapter.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

using Android.Support.V4.App;
using Android.Support.V4.View;
using CAPI.Android.Core.Model.ViewModel.QuestionnaireDetails;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CAPI.Android.Controls.QuestionnaireDetails
{
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ContentFrameAdapter : FragmentStatePagerAdapter
    {
     //   private readonly Guid questionnaireId;
        private readonly CompleteQuestionnaireView questionnaire;
        private ItemPublicKey? screenId;
        private bool isRoot;
        private IList<ItemPublicKey> screensHolder;
        private AbstractScreenChangingFragment[] mFragments;
        public ContentFrameAdapter(FragmentManager fm, CompleteQuestionnaireView questionnaire, ItemPublicKey? screenId)
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
            AbstractScreenChangingFragment fragment = this.mFragments[position];
            if (fragment != null)
                return fragment;
            
            if (position == screensHolder.Count && isRoot)
            {
                fragment =  StatisticsContentFragment.NewInstance(questionnaire.PublicKey);
            }
            else
            {

                var param = screensHolder[position];
                var model = questionnaire.Screens[param];
                var screenModel = model as QuestionnaireScreenViewModel;
                if (screenModel != null)
                {
                    fragment = ScreenContentFragment.NewInstance(screenModel.ScreenId, questionnaire.PublicKey);
                }
                else
                {
                    var grid = model as QuestionnaireGridViewModel;
                    if (grid != null)
                    {
                        fragment = GridContentFragment.NewInstance(grid.ScreenId, questionnaire.PublicKey);
                    }
                }
            }
            if (fragment == null)
                throw new InvalidOperationException();
            
            this.mFragments[position] = fragment;
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
        public void UpdateScreenData(ItemPublicKey? newScreenId)
        {
            var screenIdNotNull = newScreenId ?? this.questionnaire.Chapters.First().ScreenId;
            this.screensHolder = this.questionnaire.Screens[screenIdNotNull].Siblings.ToList();
            this.screenId = newScreenId;
            this.isRoot = this.questionnaire.Chapters.Any(s => s.ScreenId == screenIdNotNull);
          
            this.mFragments = new AbstractScreenChangingFragment[this.Count];
            this.NotifyDataSetChanged();
          
        }
    }
}
