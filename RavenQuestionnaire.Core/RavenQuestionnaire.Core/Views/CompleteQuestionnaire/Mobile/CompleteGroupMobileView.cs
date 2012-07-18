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
            this.QuestionsWithCards = new List<CompleteQuestionView>();
            this.QuestionsWithInstructions = new List<CompleteQuestionView>();
            //  this.Templates=new List<PropagatedGroupMobileView>();
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
        //public List<PropagatedGroupMobileView> Templates { get; set; }
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
           
//            List<ICompleteGroup> groups = currentGroup.Children.OfType<ICompleteGroup>().ToList();

            PublicKey = currentGroup.PublicKey;
            Title = currentGroup.Title;
            Propagated = currentGroup.Propagated;
            if (currentGroup.Propagated != Propagate.None)
            {
                PropagateTemplate = new PropagatedGroupMobileView(doc, currentGroup);
            }
            else foreach (var composite in currentGroup.Children)
            {
                if ((composite as ICompleteQuestion) != null)
                {
                    var q = composite as ICompleteQuestion;
                    var question = new CompleteQuestionFactory().CreateQuestion(doc, currentGroup, q);
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
    /*    private void CollectScreens(CompleteGroupMobileView @group)
        {
            if (@group.PropagateTemplate != null)
                Templates.Add(@group.PropagateTemplate);

            foreach (var g in @group.Children.OfType<CompleteGroupMobileView>())
            {
                if (@group.Propagated == Propagate.None)
                {
                //    Screens.Add(g);
                    CollectScreens(g);
                }
            }
            foreach (var g in @group.Children.OfType<PropagatedGroupMobileView>())
            {
                PropagatedScreens.Add(g);
            }
        }*/

        private void CollectGalleries(CompleteGroupMobileView @group)
        {
            var qs = @group.Children.OfType<CompleteQuestionView>().ToList();
            if (qs.Count() > 0)
            {
                QuestionsWithCards.AddRange(qs.Where(question => (question.Cards.Length > 0)).ToList());
            }
    /*        if (@group.Propagated != Propagate.None)
            {
                var questions = @group.PropagateTemplate.Children.OfType<CompleteQuestionView>().ToList();
                var hasCards = questions.Where(question => question.Cards.Length > 0);
                QuestionsWithCards.AddRange(hasCards.ToList());
            }
            var groups = @group.Children.OfType<CompleteGroupMobileView>().ToList();
            foreach (var g in groups)
            {
                CollectGalleries(g);
            }*/
        }

        private void CollectInstructions(CompleteGroupMobileView @group)
        {
            var qs = @group.Children.OfType<CompleteQuestionView>().ToList();
            if (qs.Count > 0)
            {
                QuestionsWithInstructions.AddRange(qs.Where(question => !string.IsNullOrWhiteSpace(question.Instructions)).ToList());
            }
           /* if (@group.Propagated != Propagate.None)
            {
                var questions = @group.PropagateTemplate.Children.OfType<CompleteQuestionView>().ToList();
                var hasInstructions = questions.Where(q => (!string.IsNullOrWhiteSpace(q.Instructions)));
                QuestionsWithInstructions.AddRange(hasInstructions.ToList());
            }
            var groups = @group.Children.OfType<CompleteGroupMobileView>().ToList();
            foreach (var g in groups)
            {
                CollectInstructions(g);
            }*/
        }
    }
}