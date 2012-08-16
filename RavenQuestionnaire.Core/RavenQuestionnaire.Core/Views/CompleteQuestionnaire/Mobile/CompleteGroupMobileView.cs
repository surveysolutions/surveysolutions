using System;
using System.Linq;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Views.Question;
using RavenQuestionnaire.Core.AbstractFactories;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Mobile
{
    public abstract class AbstractGroupMobileView : ICompositeView
    {
        public AbstractGroupMobileView()
        {
            Children = new List<ICompositeView>();
            this.QuestionsWithCards = new List<CompleteQuestionView>();
            this.QuestionsWithInstructions = new List<CompleteQuestionView>();
        }

        public Guid PublicKey { get; set; }
        public Guid UniqueKey { get; set; }
        public string Title { get; set; }
        public Guid? Parent { get; set; }
        public List<ICompositeView> Children { get; set; }
        public Propagate Propagated { get; set; }
        public bool Enabled { get; set; }
        public ScreenNavigation Navigation { get; set; }
        public Guid QuestionnairePublicKey { get; set; }
        public List<CompleteQuestionView> QuestionsWithCards { get; set; }
        public List<CompleteQuestionView> QuestionsWithInstructions { get; set; }
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
                                       ScreenNavigation navigation)
            : this()
        {
            this.QuestionnairePublicKey = doc.PublicKey;
            this.Navigation = navigation;
            PublicKey = currentGroup.PublicKey;
            Title = currentGroup.Title;
            Propagated = currentGroup.Propagated;
            Enabled = currentGroup.Enabled;
            if (currentGroup.Propagated != Propagate.None)
            {
                PropagateTemplate = new PropagatedGroupMobileView(doc, currentGroup);
            }
            else foreach (var composite in currentGroup.Children)
            {
                if ((composite as ICompleteQuestion) != null)
                {
                    var q = composite as ICompleteQuestion;
                    var question = new CompleteQuestionFactory().CreateQuestion(doc, q);
                    Children.Add(question);
                }
                else
                {
                    var g = composite as CompleteGroup;
                    if (g.Propagated == Propagate.None || !g.PropogationPublicKey.HasValue)
                        Children.Add(new CompleteGroupMobileView(doc, g, new ScreenNavigation()));
                    else
                    {
                        var template =
                            Children.FirstOrDefault(
                                parent => parent.PublicKey == g.PublicKey && !(parent is PropagatedGroupMobileView));
                        template.Children.Add(new PropagatedGroupMobileView(doc, g));

                    }
                }
                CollectGalleries(this);
                CollectInstructions(this);
            }

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

        public virtual string GetClientId(string prefix)
        {
            return string.Format("{0}_{1}", prefix, PublicKey);
        }

        private void CollectGalleries(CompleteGroupMobileView @group)
        {
            var qs = @group.Children.OfType<CompleteQuestionView>().ToList();
            if (qs.Count() > 0)
            {
                QuestionsWithCards.AddRange(qs.Where(question => (question.Cards.Length > 0)).ToList());
            }
        }

        private void CollectInstructions(CompleteGroupMobileView @group)
        {
            var qs = @group.Children.OfType<CompleteQuestionView>().ToList();
            if (qs.Count > 0)
            {
                QuestionsWithInstructions.AddRange(qs.Where(question => !string.IsNullOrWhiteSpace(question.Instructions)).ToList());
            }
        }
    }
}