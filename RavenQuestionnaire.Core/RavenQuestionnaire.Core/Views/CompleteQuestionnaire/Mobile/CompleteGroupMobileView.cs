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
            PropagatedQuestions = new List<PropagatedQuestion>();
            PropagatedGroups = new List<PropagatedGroup>();
            PropogationPublicKeys = new List<Guid>();
            AutoPropagate = new List<bool>();
        }
        public CompleteGroupMobileView(CompleteQuestionnaireDocument doc, CompleteGroup currentGroup)
            : this()
        {
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
                    this.Questions = questions.Select(
                            q => new CompleteQuestionFactory().CreateQuestion(doc, currentGroup, q)).ToList();
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
            var qf = new CompleteQuestionFactory();
            PublicKey = propagatable.PublicKey;
            GroupText = propagatable.Title;
            Propagated = propagatable.Propagated;
            this.Questions = propagatable.Children.OfType<ICompleteQuestion>().Select(
                           q => new CompleteQuestionFactory().CreateQuestion(doc, propagatable, q)).ToList();
            var propagated = propGroups.Where(g => g != propagatable).Select(g => g as PropagatableCompleteGroup).ToList();

            if (propagated.Count > 0)
            {
                PropogationPublicKeys = propagated.Select(g => g.PropogationPublicKey).ToList();
                foreach (var @group in propagated)
                {

                    var pgroup = new PropagatedGroup(@group.PublicKey, @group.Title, @group.AutoPropagate,
                                                     @group.PropogationPublicKey, new List<CompleteQuestionView>());
                                     
                    PropagatedGroups.Add(pgroup);
                    AutoPropagate.Add(@group.AutoPropagate);
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

        public Guid PublicKey { get; set; }

        public string GroupText { get; set; }

        public Propagate Propagated { get; set; }

        public List<Guid> PropogationPublicKeys { get; set; }

        public List<CompleteQuestionView> Questions { get; set; }

        public List<CompleteGroupMobileView> Groups { get; set; }

        public List<PropagatedQuestion> PropagatedQuestions { get; set; }

        public List<bool> AutoPropagate { get; set; }

        public List<PropagatedGroup> PropagatedGroups { get; set; }

        public int PropagatedGroupsCount { get { return PropagatedQuestions.Count > 0 ? PropagatedQuestions[0].Questions.Count : 0; } }

        public Counter Totals { get; set; }

        public virtual string GetClientId(string prefix)
        {
            return string.Format("{0}_{1}", prefix, PublicKey);
        }
    }
}