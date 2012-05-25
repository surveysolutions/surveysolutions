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
    public class ScreenNavigation
    {
        public ScreenNavigation()
        {
            BreadCumbs = new List<CompleteGroupHeaders>();
        }
        public List<CompleteGroupHeaders> BreadCumbs { get; set; }

        public CompleteGroupHeaders NextScreen { get; set; }

        public CompleteGroupHeaders PrevScreen { get; set; }

        public string CurrentScreenTitle { get; set; }

    }
    public class CompleteGroupMobileView
    {
        public CompleteGroupMobileView()
        {
            Questions = new List<CompleteQuestionView>();
            Groups = new List<CompleteGroupMobileView>();
            Propagated = Propagate.None;
            PropagatedQuestions = new List<PropagatedQuestion>();
            PropagatedGroups = new List<PropagatedGroup>();
            PropogationPublicKeys = new List<Guid>();
            AutoPropagate = new List<bool>();
            Navigation = new ScreenNavigation();
        }
        public static CompleteGroupHeaders GetHeader(ICompleteGroup group)
        {
            return group == null ? null : new CompleteGroupHeaders()
            {
                GroupText = group.Title,
                PublicKey = group.PublicKey
            };
        }
        public CompleteGroupMobileView(CompleteQuestionnaireDocument doc, CompleteGroup currentGroup) : this()
        {
            InitNavigation(currentGroup);

            var questions = currentGroup.Children.OfType<ICompleteQuestion>().ToList();
            var groups = currentGroup.Children.OfType<ICompleteGroup>().ToList();
          
            PublicKey = currentGroup.PublicKey;
            GroupText = currentGroup.Title;
            Propagated = currentGroup.Propagated;
            if (questions.Count > 0)
            {
                if (currentGroup.Propagated == Propagate.None)
                {
                    var questionQgroup = new CompleteGroupMobileView();
                    questionQgroup.Questions =
                        questions.Select(
                            q => new CompleteQuestionFactory().CreateQuestion(doc, currentGroup, q)).ToList();
                    questionQgroup.PublicKey = Guid.Empty;
                    questionQgroup.GroupText = "Main";
                    Groups.Add(questionQgroup);
                }
                else if (currentGroup.Propagated == Propagate.Propagated)
                {
                    this.Questions = questions.Select(q => new CompleteQuestionFactory().CreateQuestion(doc, currentGroup, q)).ToList();
                }
            }

            // grouping by group's PublicKey
            var propGroups = new Dictionary<Guid, List<CompleteGroup>>();
            foreach (var @group in groups)
            {
                if (!propGroups.ContainsKey(@group.PublicKey))
                {
                    propGroups.Add(@group.PublicKey, new List<CompleteGroup>());
                }
                propGroups[@group.PublicKey].Add(@group as CompleteGroup);
            }
            foreach (var k in propGroups)
            {
                var prop = Propagate.Propagated;
                if (k.Value.Count == 1 && (k.Value[0] as PropagatableCompleteGroup) == null)
                {
                    prop = Propagate.None;
                }
                Groups.Add(prop != Propagate.Propagated
                               ? new CompleteGroupMobileView(doc, k.Value[0] as CompleteGroup)
                               : new CompleteGroupMobileView(doc, k.Value));
            }
        }

        public CompleteGroupMobileView(CompleteQuestionnaireDocument doc, List<CompleteGroup> propGroups)
            : this()
        {
            var propagatable = propGroups.Single(g => (g as PropagatableCompleteGroup) == null);

            InitNavigation(propagatable);

            var qf = new CompleteQuestionFactory();
            PublicKey = propagatable.PublicKey;
            GroupText = propagatable.Title;
            Propagated = propagatable.Propagated;
            this.Questions = propagatable.Children.OfType<ICompleteQuestion>().Select(
                           q => new CompleteQuestionFactory().CreateQuestion(doc, propagatable, q)).ToList();

            PropagateTemplate = new PropagatedGroup(propagatable.PublicKey, propagatable.Title, false, Guid.Empty, this.Questions);
            PropagateTemplate.Navigation.CurrentScreenTitle = propagatable.Title;
            PropagateTemplate.Navigation.BreadCumbs.AddRange(this.Navigation.BreadCumbs);
            PropagateTemplate.Navigation.BreadCumbs.Add(new CompleteGroupHeaders() { GroupText = this.GroupText, PublicKey = this.PublicKey });


            var propagated = propGroups.Where(g => g != propagatable).Select(g => g as PropagatableCompleteGroup).ToList();

            if (propagated.Count > 0)
            {
                PropogationPublicKeys = propagated.Select(g => g.PropogationPublicKey).ToList();
                PropagatedGroup lastGroup = null;
                foreach (var @group in propagated)
                {

                    var pgroup = new PropagatedGroup(@group.PublicKey, @group.Title, @group.AutoPropagate,
                                                     @group.PropogationPublicKey, new List<CompleteQuestionView>());


                    if (lastGroup != null)
                    {
                        pgroup.Navigation.PrevScreen = new CompleteGroupHeaders { GroupText = lastGroup.GroupText, PublicKey = lastGroup.PropogationKey };
                        lastGroup.Navigation.NextScreen = new CompleteGroupHeaders { GroupText = pgroup.GroupText, PublicKey = @group.PropogationPublicKey };
                    }
                    pgroup.Navigation.CurrentScreenTitle = @group.Title;
                    pgroup.Navigation.BreadCumbs.AddRange(this.Navigation.BreadCumbs);
                    pgroup.Navigation.BreadCumbs.Add(new CompleteGroupHeaders() { GroupText = this.GroupText, PublicKey = this.PublicKey });

                    PropagatedGroups.Add(pgroup);
                    AutoPropagate.Add(@group.AutoPropagate);
                    lastGroup = pgroup;
                }
            }
            var questions = propagatable.Children.OfType<ICompleteQuestion>().ToList();
            for (int i = 0; i < questions.Count; i++)
            {
                var question = questions[i];
                var pq = new PropagatedQuestion
                             {
                                 PublicKey = question.PublicKey,
                                 QuestionText = question.QuestionText,
                                 Instructions = question.Instructions,
                                 Questions = new List<CompleteQuestionView>()
                             };

                for (int index = 0; index < propagated.Count; index++)
                {
                    var p = propagated[index];
                    var cq = qf.CreateQuestion(doc, p, p.Children[i] as ICompleteQuestion);
                    pq.Questions.Add(cq);
                    PropagatedGroups[index].Questions.Add(cq);
                }
                PropagatedQuestions.Add(pq);
            }
        }

        private void InitNavigation(CompleteGroup currentGroup)
        {
            var parent = currentGroup.ParentGroup;
            while (parent != null)
            {
                var bc = GetHeader(parent);
                Navigation.BreadCumbs.Add(bc);
                parent = parent.ParentGroup;
            }
            Navigation.BreadCumbs.Reverse();
            Navigation.NextScreen = GetHeader(currentGroup.NextGroup);
            Navigation.PrevScreen = GetHeader(currentGroup.PrevGroup);
            Navigation.CurrentScreenTitle = currentGroup.Title;
        }


        public Guid PublicKey { get; set; }

        public string GroupText { get; set; }

        public Propagate Propagated { get; set; }

        public ScreenNavigation Navigation { get; set; }

        public List<Guid> PropogationPublicKeys { get; set; }

        public List<CompleteQuestionView> Questions { get; set; }

        public List<CompleteGroupMobileView> Groups { get; set; }

        public List<PropagatedQuestion> PropagatedQuestions { get; set; }

        public List<bool> AutoPropagate { get; set; }

        public PropagatedGroup PropagateTemplate { get; set; }

        public List<PropagatedGroup> PropagatedGroups { get; set; }

        public int PropagatedGroupsCount { get { return PropagatedQuestions.Count > 0 ? PropagatedQuestions[0].Questions.Count : 0; } }

        public Counter Totals { get; set; }

        public virtual string GetClientId(string prefix)
        {
            return string.Format("{0}_{1}", prefix, PublicKey);
        }
    }
}