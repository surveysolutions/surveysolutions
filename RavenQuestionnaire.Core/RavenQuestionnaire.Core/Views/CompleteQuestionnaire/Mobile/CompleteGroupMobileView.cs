#region

using System;
using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.AbstractFactories;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Views.Question;

#endregion

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Mobile
{
    public abstract class AbstractGroupMobileView : ICompositeView
    {
        public AbstractGroupMobileView()
        {
            Children = new List<ICompositeView>();
        }
        public Guid PublicKey { get; set; }

        public string Title { get; set; }

        public Guid? Parent { get; set; }

        public List<ICompositeView> Children { get; set; }

        public Propagate Propagated { get; set; }

        public ScreenNavigation Navigation { get; set; }
    }

    public class CompleteGroupMobileView : AbstractGroupMobileView
    {
        public CompleteGroupMobileView()
            : base()
        {
            Propagated = Propagate.None;
            Navigation = new ScreenNavigation();
        }

        public CompleteGroupMobileView(CompleteQuestionnaireDocument doc, CompleteGroup currentGroup,
                                       IList<ScreenNavigation> navigations)
            : this()
        {
            InitNavigation(currentGroup, navigations);

            List<ICompleteGroup> groups = currentGroup.Children.OfType<ICompleteGroup>().ToList();

            PublicKey = currentGroup.PublicKey;
            Title = currentGroup.Title;
            Propagated = currentGroup.Propagated;

            // grouping by group's PublicKey
            var propGroups = new Dictionary<Guid, List<CompleteGroup>>();
            foreach (ICompleteGroup @group in groups)
            {
                if (!propGroups.ContainsKey(@group.PublicKey))
                {
                    propGroups.Add(@group.PublicKey, new List<CompleteGroup>());
                }
                propGroups[@group.PublicKey].Add(@group as CompleteGroup);
            }

            var addedGroupsPK = new List<Guid>();

            foreach (var composite in currentGroup.Children)
            {
                if ((composite as ICompleteQuestion) != null)
                {
                    var q = composite as ICompleteQuestion;
                    var question = new CompleteQuestionFactory().CreateQuestion(doc, currentGroup, q);
                    Children.Add(question);
                }
                else
                {
                    var g = composite as ICompleteGroup;
                    if (!addedGroupsPK.Contains(g.PublicKey))
                    {
                        var pgroups = propGroups[g.PublicKey];
                        var prop = Propagate.Propagated;
                        if (pgroups.Count == 1 && pgroups[0].Propagated == Propagate.None)
                        {
                            prop = Propagate.None;
                        }
                        Children.Add(prop != Propagate.None
                              ? new CompleteGroupMobileView(doc, pgroups, navigations)
                              : new CompleteGroupMobileView(doc, pgroups[0], navigations));
                        addedGroupsPK.Add(g.PublicKey);
                    }
                }
            }
        }


        public CompleteGroupMobileView(CompleteQuestionnaireDocument doc, List<CompleteGroup> propGroups,
                                       IList<ScreenNavigation> navigations)
            : this()
        {
            CompleteGroup propagatable = propGroups.Single(g => !g.PropogationPublicKey.HasValue);

            InitNavigation(propagatable, navigations);

            PublicKey = propagatable.PublicKey;
            Title = propagatable.Title;
            Propagated = propagatable.Propagated;
            var questions = propagatable.Children.OfType<ICompleteQuestion>().Select(q => new CompleteQuestionFactory().CreateQuestion(doc, propagatable, q)).ToList();

            PropagateTemplate = new PropagatedGroupMobileView(propagatable.PublicKey, propagatable.Title, false, Guid.Empty, questions);
            PropagateTemplate.Navigation.CurrentScreenTitle = propagatable.Title;
            PropagateTemplate.Navigation.BreadCumbs.AddRange(Navigation.BreadCumbs);


            List<CompleteGroup> propagated = propGroups.Where(g => g != propagatable && g.PropogationPublicKey.HasValue).ToList();

            var propagetedGroups = new List<ICompositeView>();
            if (propagated.Count > 0)
            {
                PropagatedGroupMobileView lastGroup = null;
                foreach (CompleteGroup @group in propagated)
                {
                    string groupTitle = @group.Title;
                    var pgroup = new PropagatedGroupMobileView(@group.PublicKey, groupTitle,
                                                               @group.Propagated == Propagate.AutoPropagated,
                                                               @group.PropogationPublicKey.Value,
                                                               @group.Children.OfType<ICompleteQuestion>().Select(q => new CompleteQuestionFactory().CreateQuestion(doc, propagatable, q)).ToList());

                    if (lastGroup != null)
                    {
                        pgroup.Navigation.PrevScreen = new CompleteGroupHeaders
                                                           {
                                                               GroupText = lastGroup.Title,
                                                               PublicKey = lastGroup.PropogationKey
                                                           };
                        lastGroup.Navigation.NextScreen = new CompleteGroupHeaders
                                                              {
                                                                  GroupText = pgroup.Title,
                                                                  PublicKey = @group.PropogationPublicKey.Value
                                                              };
                    }
                    pgroup.Navigation.CurrentScreenTitle = groupTitle;
                    pgroup.Navigation.BreadCumbs.AddRange(Navigation.BreadCumbs);

                    propagetedGroups.Add(pgroup);
                    lastGroup = pgroup;
                }
            }

            foreach (PropagatedGroupMobileView group in propagetedGroups)
            {
                List<string> featuredList = group.Children.OfType<CompleteQuestionView>().Where(q => q.Featured)
                    .Select(questionView => string.Join(",", questionView.Answers.Where(a => a.Selected)
                                                                 .Select(answer => !string.IsNullOrEmpty(answer.Title)
                                                                         ? answer.Title
                                                                         : answer.AnswerValue)
                                                                 .Where(a => !string.IsNullOrEmpty(a)).ToArray())).ToList();
                group.FeaturedTitle = string.Join(",", featuredList.Where(f => !string.IsNullOrEmpty(f)));
            }

            this.Children.AddRange(propagetedGroups);
        }
        public PropagatedGroupMobileView PropagateTemplate { get; set; }

        public Counter Totals { get; set; }

        public static CompleteGroupHeaders GetHeader(ICompleteGroup group)
        {
            return group == null
                       ? null
                       : new CompleteGroupHeaders
                             {
                                 GroupText = group.Title,
                                 PublicKey = group.PublicKey
                             };
        }

        private void InitNavigation(CompleteGroup currentGroup, IList<ScreenNavigation> navigations)
        {
            Guid pKey = Guid.Empty;
            if (currentGroup.PropogationPublicKey.HasValue)
                pKey = currentGroup.PropogationPublicKey.Value;

            ScreenNavigation current = navigations.SingleOrDefault(n => (n.PublicKey == currentGroup.PublicKey) && (n.PropagateKey == pKey));
            if (current != null)
            {
                CompleteGroupHeaders parent = current.Parent;
                while (parent != null)
                {
                    Navigation.BreadCumbs.Add(parent);
                    ScreenNavigation nav = navigations.SingleOrDefault(n => n.PublicKey == parent.PublicKey);
                    parent = nav == null ? null : nav.Parent;
                }
                Navigation.BreadCumbs.Reverse();

                Navigation.NextScreen = current.NextScreen;
                Navigation.PrevScreen = current.PrevScreen;
            }
            if (Navigation.BreadCumbs.Count == 1)
            {
                if (Navigation.NextScreen != null)
                    Navigation.NextScreen.IsExternal = true;
                if (Navigation.PrevScreen != null)
                    Navigation.PrevScreen.IsExternal = true;
            }
            Navigation.CurrentScreenTitle = currentGroup.Title;
        }

        public virtual string GetClientId(string prefix)
        {
            return string.Format("{0}_{1}", prefix, PublicKey);
        }
    }
}