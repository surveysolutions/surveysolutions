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

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Vertical
{
    public class CompleteGroupViewV
    {
        public CompleteGroupViewV()
        {
            Questions = new List<CompleteQuestionView>();
            Groups = new List<CompleteGroupViewV>();
            Propagated = Propagate.None;
            PropagatedQuestions = new List<PropagatedQuestion>();
            PropagatedGroups = new List<PropagatedGroup>();
            PropogationPublicKeys = new List<Guid>();
            AutoPropagate = new List<bool>();
        }
        public CompleteGroupViewV(CompleteQuestionnaireDocument doc, CompleteGroup currentGroup)
            : this()
        {
            PublicKey = currentGroup.PublicKey;
            GroupText = currentGroup.Title;
            Propagated = currentGroup.Propagated;
            if (currentGroup.Questions.Count > 0 && currentGroup.Propagated == Propagate.None)
            {
                var questionQgroup = new CompleteGroupViewV();
                questionQgroup.Questions = currentGroup.Questions.Select(q => new CompleteQuestionFactory().CreateQuestion(doc, currentGroup, q)).ToList();
                questionQgroup.PublicKey = Guid.Empty;
                questionQgroup.GroupText = "Main";
                Groups.Add(questionQgroup);
            }
            // grouping by group's PublicKey
            var propGroups = new Dictionary<Guid, List<CompleteGroup>>();
            foreach (var @group in currentGroup.Groups)
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
                               ? new CompleteGroupViewV(doc, k.Value[0] as CompleteGroup)
                               : new CompleteGroupViewV(doc, k.Value));
            }

           
        }
        public CompleteGroupViewV(CompleteQuestionnaireDocument doc, List<CompleteGroup> propGroups)
            : this()
        {
            var propagatable = propGroups.Single(g => (g as PropagatableCompleteGroup) == null);
            var qf = new CompleteQuestionFactory();
            PublicKey = propagatable.PublicKey;
            GroupText = propagatable.Title;
            Propagated = propagatable.Propagated;

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
            for (int i = 0; i < propagatable.Questions.Count; i++)
            {
                var question = propagatable.Questions[i];
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
                    var cq = qf.CreateQuestion(doc, p, p.Questions[i]);
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

        public List<CompleteGroupViewV> Groups { get; set; }

        public List<PropagatedQuestion> PropagatedQuestions { get; set; }

        public List<bool> AutoPropagate { get; set; }

        public List<PropagatedGroup> PropagatedGroups { get; set; }

        public int PropagatedGroupsCount { get { return PropagatedQuestions.Count > 0 ? PropagatedQuestions[0].Questions.Count : 0; } }

        

        public virtual string GetClientId(string prefix)
        {
            return string.Format("{0}_{1}", prefix, PublicKey);
        }
    }
}