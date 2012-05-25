using System;
using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Iterators;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Question;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Mobile
{
    public class CompleteQuestionnaireMobileView
    {
        private CompleteQuestionnaireMobileView()
        {
            QuestionsWithCards = new List<CompleteQuestionView>();
            QuestionsWithInstructions = new List<CompleteQuestionView>();
            Screens = new List<CompleteGroupMobileView>();
            PropagatedScreens = new List<PropagatedGroup>();
            Templates = new List<PropagatedGroup>();
        }
        public CompleteQuestionnaireMobileView(CompleteQuestionnaireDocument doc, ICompleteGroup currentGroup)
            : this()
        {
            Id = IdUtil.ParseId(doc.Id);
            Title = doc.Title;
            CreationDate = doc.CreationDate;
            LastEntryDate = doc.LastEntryDate;
            Status = doc.Status;
            Responsible = doc.Responsible;

            var iterator = new CompleteGroupIterator(doc);
            CurrentGroup = new CompleteGroupMobileView(doc, currentGroup as CompleteGroup);

            CollectAll(doc);
        }



        public CompleteQuestionnaireMobileView(CompleteQuestionnaireDocument doc)
            : this()
        {

            Id = IdUtil.ParseId(doc.Id);
            Title = doc.Title;
            CreationDate = doc.CreationDate;
            LastEntryDate = doc.LastEntryDate;
            Status = doc.Status;
            Responsible = doc.Responsible;
            QuestionsWithCards = new List<CompleteQuestionView>();
            QuestionsWithInstructions = new List<CompleteQuestionView>();

            var iterator = new CompleteGroupIterator(doc);

            var group = new CompleteGroup { Children = doc.Children.Where(c=>c is ICompleteQuestion).ToList() };
            CurrentGroup = new CompleteGroupMobileView(doc, group);

            CollectAll(doc);
        }

        private void CollectAll(CompleteQuestionnaireDocument doc)
        {
            InitGroups(doc, CurrentGroup.PublicKey);
            Totals = CalcProgress(doc);
            CollectGalleries(CurrentGroup);
            CollectInstructions(CurrentGroup);
            CollectScreens(CurrentGroup);           
        }

        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastEntryDate { get; set; }

        public SurveyStatus Status { get; set; }

        public List<CompleteQuestionView> QuestionsWithCards { get; set; }
        public List<CompleteQuestionView> QuestionsWithInstructions { get; set; }

        public List<CompleteGroupMobileView> Screens { get; set; }
        public List<PropagatedGroup> Templates { get; set; }
        public List<PropagatedGroup> PropagatedScreens { get; set; }
        
        public UserLight Responsible { set; get; }
        public CompleteGroupMobileView CurrentGroup { get; set; }
        public CompleteGroupHeaders[] Groups { get; set; }
        public Counter Totals { get; set; }

        protected void InitGroups(CompleteQuestionnaireDocument doc, Guid currentGroupPublicKey)
        {
            var questions = doc.Children.OfType<ICompleteQuestion>().ToList();
            var groups = doc.Children.OfType<ICompleteGroup>().ToList();
            if (questions.Count > 0)
            {
                Groups = new CompleteGroupHeaders[groups.Count + 1];

                Groups[0] = new CompleteGroupHeaders
                                {
                                    PublicKey = Guid.Empty,
                                    GroupText = "Main",
                                    Totals = CountQuestions(questions)
                                };
                for (var i = 1; i <= groups.Count; i++)
                {
                    Groups[i] = new CompleteGroupHeaders
                                    {
                                        PublicKey = groups[i - 1].PublicKey,
                                        GroupText = groups[i - 1].Title
                                    };
                    Groups[i].Totals = CalcProgress(groups[i - 1]);
                }
            }
            else
            {
                Groups = new CompleteGroupHeaders[groups.Count];
                for (var i = 0; i < groups.Count; i++)
                {
                    Groups[i] = new CompleteGroupHeaders
                                    {
                                        PublicKey = groups[i].PublicKey,
                                        GroupText = groups[i].Title
                                    };
                    Groups[i].Totals = CalcProgress(groups[i]);
                }
            }
            
            var current = Groups.FirstOrDefault(g => g.PublicKey == currentGroupPublicKey);
            current.IsCurrent = true;
            CurrentGroup.Totals = current.Totals;
        }

        private void CollectScreens(CompleteGroupMobileView @group)
        {
            if (@group.Propagated == Propagate.None)
            {
                foreach (var g in @group.Groups)
                {
                    if (g.PublicKey == Guid.Empty) continue;
                    Screens.Add(g);
                    CollectScreens(g);
                }    
            }
            else
            {
                Templates.Add(@group.PropagateTemplate);
                foreach (var g in @group.PropagatedGroups)
                {
                    PropagatedScreens.Add(g);
                }  
            }
        }

        private void CollectGalleries(CompleteGroupMobileView @group)
        {
            if (@group.Questions.Count > 0)
            {
                var enabled = @group.Questions.Where(q => q.Enabled).ToList();
                QuestionsWithCards.AddRange(enabled.Where(question => (question.Cards.Length > 0)).ToList());
            }
            if (@group.PropagatedQuestions.Count > 0)
            {
                var enabled = (@group.PropagatedQuestions.Where(q => q.Questions.Any(qq => qq.Enabled))).ToList();
                var hasCards = enabled.Where(question => question.Questions.Any(q => (q.Cards.Length > 0)));
                QuestionsWithCards.AddRange(hasCards.Select(qq => qq.Questions.First()).ToList());
            }
            foreach (var g in @group.Groups)
            {
                CollectGalleries(g);
            }
        }

        private void CollectInstructions(CompleteGroupMobileView @group)
        {
            if (@group.Questions.Count > 0)
            {
                var enabled = @group.Questions.Where(q => q.Enabled).ToList();
                QuestionsWithInstructions.AddRange(enabled.Where(question => !string.IsNullOrWhiteSpace(question.Instructions)).ToList());
            }
            if (@group.PropagatedQuestions.Count > 0)
            {
                var enabled = (@group.PropagatedQuestions.Where(q => q.Questions.Any(qq => qq.Enabled))).ToList();
                var hasCards = enabled.Where(question => question.Questions.Any(q => (!string.IsNullOrWhiteSpace(q.Instructions))));
                QuestionsWithInstructions.AddRange(hasCards.Select(qq => qq.Questions.First()).ToList());
            }
            foreach (var g in @group.Groups)
            {
                CollectInstructions(g);
            }
        }

        private Counter CalcProgress(ICompleteGroup @group)
        {
            var total = new Counter();

            var propagated = @group as PropagatableCompleteGroup;
            if (propagated != null)
            {
                total = total + CountQuestions(propagated.Children.Select(q => q as ICompleteQuestion).ToList());
                return total;
            }
            var complete = @group as CompleteGroup;
            if (complete != null && complete.Propagated != Propagate.None)
                return total;
            var gruoSubGroup = @group.Children.OfType<ICompleteGroup>().ToList();
            var gruoSubQuestions = @group.Children.OfType<ICompleteQuestion>().ToList();
            total = total + CountQuestions(gruoSubQuestions);

            foreach (var g in gruoSubGroup)
            {
                total = total + CalcProgress(g);
            }
            return total;
        }

        private Counter CountQuestions(List<ICompleteQuestion> questions)
        {
            if (questions == null || questions.Count == 0)
                return new Counter();

            var enabled = questions.Where(q => q.Enabled).ToList();

            var total = new Counter
                            {
                                Total = questions.Count,
                                Enablad = enabled.Count(),
                                Answered =
                                    enabled.Count(
                                        question =>
                                        question.Children.Any(
                                            a => a is ICompleteAnswer && ((ICompleteAnswer) a).Selected))
                            };
            return total;
        }
    }
}