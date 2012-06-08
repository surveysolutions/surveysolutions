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
    public class CompleteGroupMobileView
    {
        public CompleteGroupMobileView()
        {
            Questions = new List<CompleteQuestionView>();
            Groups = new List<CompleteGroupMobileView>();
            Propagated = Propagate.None;
            //PropagatedQuestions = new List<PropagatedQuestion>();
            PropagatedGroups = new List<PropagatedGroup>();
            PropogationPublicKeys = new List<Guid>();
            AutoPropagate = new List<bool>();
            Navigation = new ScreenNavigation();
        }

        public CompleteGroupMobileView(CompleteQuestionnaireDocument doc, CompleteGroup currentGroup,
                                       IList<ScreenNavigation> navigations)
            : this()
        {
            InitNavigation(currentGroup, navigations);

            List<ICompleteQuestion> questions = currentGroup.Children.OfType<ICompleteQuestion>().ToList();
            List<ICompleteGroup> groups = currentGroup.Children.OfType<ICompleteGroup>().ToList();

            PublicKey = currentGroup.PublicKey;
            Title = currentGroup.Title;
            Propagated = currentGroup.Propagated;
            if (questions.Count > 0)
            {
                Questions =
                    questions.Select(q => new CompleteQuestionFactory().CreateQuestion(doc, currentGroup, q)).ToList();
            }

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
            foreach (var k in propGroups)
            {
                Propagate prop = Propagate.Propagated;
                if (k.Value.Count == 1 && k.Value[0].PropogationPublicKey.HasValue)
                {
                    prop = Propagate.None;
                }
                Groups.Add(prop != Propagate.Propagated
                               ? new CompleteGroupMobileView(doc, k.Value[0], navigations)
                               : new CompleteGroupMobileView(doc, k.Value, navigations));
            }
        }


        public CompleteGroupMobileView(CompleteQuestionnaireDocument doc, List<CompleteGroup> propGroups,
                                       IList<ScreenNavigation> navigations)
            : this()
        {
            CompleteGroup propagatable = propGroups.Single(g => !g.PropogationPublicKey.HasValue);

            InitNavigation(propagatable, navigations);

            var qf = new CompleteQuestionFactory();
            PublicKey = propagatable.PublicKey;
            Title = propagatable.Title;
            Propagated = propagatable.Propagated;
            Questions = propagatable.Children.OfType<ICompleteQuestion>().Select(
                q => new CompleteQuestionFactory().CreateQuestion(doc, propagatable, q)).ToList();

            PropagateTemplate = new PropagatedGroup(propagatable.PublicKey, propagatable.Title, false, Guid.Empty,
                                                    Questions);
            PropagateTemplate.Navigation.CurrentScreenTitle = propagatable.Title;
            PropagateTemplate.Navigation.BreadCumbs.AddRange(Navigation.BreadCumbs);
            //PropagateTemplate.Navigation.BreadCumbs.Add(new CompleteGroupHeaders() { Title = this.Title, PublicKey = this.PublicKey });


            List<CompleteGroup> propagated =
                propGroups.Where(g => g != propagatable && g.PropogationPublicKey.HasValue).ToList();

            if (propagated.Count > 0)
            {
                PropogationPublicKeys = propagated.Select(g => g.PropogationPublicKey.Value).ToList();
                PropagatedGroup lastGroup = null;
                for (int i = 0; i < propagated.Count; i++)
                {
                    CompleteGroup @group = propagated[i];
                    string groupTitle = @group.Title;
                    var pgroup = new PropagatedGroup(@group.PublicKey, groupTitle,
                                                     @group.Propagated == Propagate.AutoPropagated,
                                                     @group.PropogationPublicKey.Value, new List<CompleteQuestionView>());

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

                    PropagatedGroups.Add(pgroup);
                    AutoPropagate.Add(@group.Propagated == Propagate.AutoPropagated);
                    lastGroup = pgroup;
                }
            }
            List<ICompleteQuestion> questions = propagatable.Children.OfType<ICompleteQuestion>().ToList();
            for (int i = 0; i < questions.Count; i++)
            {
                ICompleteQuestion question = questions[i];
                var pq = new PropagatedQuestion
                             {
                                 PublicKey = question.PublicKey,
                                 QuestionText = question.QuestionText,
                                 Instructions = question.Instructions,
                                 Questions = new List<CompleteQuestionView>()
                             };

                for (int index = 0; index < propagated.Count; index++)
                {
                    CompleteGroup p = propagated[index];
                    CompleteQuestionView cq = qf.CreateQuestion(doc, p, p.Children[i] as ICompleteQuestion);
                    pq.Questions.Add(cq);
                    PropagatedGroups[index].Questions.Add(cq);
                }
                //PropagatedQuestions.Add(pq);
            }
            foreach (PropagatedGroup group in PropagatedGroups)
            {
                List<string> featuredList = group.Questions.Where(q => q.Featured)
                    .Select(questionView => string.Join(",", questionView.Answers.Where(a => a.Selected)
                                                                 .Select(
                                                                     answer =>
                                                                     !string.IsNullOrEmpty(answer.Title)
                                                                         ? answer.Title
                                                                         : answer.AnswerValue)
                                                                 .Where(a => !string.IsNullOrEmpty(a)).ToArray())).
                    ToList();
                group.FeaturedTitle = string.Join(",", featuredList.Where(f => !string.IsNullOrEmpty(f)));
            }
        }


        public Guid PublicKey { get; set; }

        public string Title { get; set; }

        public Propagate Propagated { get; set; }

        public ScreenNavigation Navigation { get; set; }

        public List<Guid> PropogationPublicKeys { get; set; }

        public List<CompleteQuestionView> Questions { get; set; }

        public List<CompleteGroupMobileView> Groups { get; set; }

        //public List<PropagatedQuestion> PropagatedQuestions { get; set; }

        public List<bool> AutoPropagate { get; set; }

        public PropagatedGroup PropagateTemplate { get; set; }

        public List<PropagatedGroup> PropagatedGroups { get; set; }

       /* public int PropagatedGroupsCount
        {
            get { return PropagatedQuestions.Count > 0 ? PropagatedQuestions[0].Questions.Count : 0; }
        }*/

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

            ScreenNavigation current =
                navigations.Single(n => (n.PublicKey == currentGroup.PublicKey) && (n.PropagateKey == pKey));
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