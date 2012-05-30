using System;
using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Composite;
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

            CollectAll(doc, currentGroup as CompleteGroup);
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

            var group = new CompleteGroup { Children = doc.Children.Where(c => c is ICompleteQuestion).ToList() };

            CollectAll(doc, group);
        }

        private void CollectAll(CompleteQuestionnaireDocument doc, CompleteGroup group)
        {
            IList<ScreenNavigation> navigations = new List<ScreenNavigation>();

            var queue = new Queue<ICompleteGroup>();
            queue.Enqueue(doc);
            while (queue.Count != 0)
            {
                ICompleteGroup item = queue.Dequeue();
                List<IComposite> innerGroups = item.Children.Where(c => c is ICompleteGroup).ToList();
                ScreenNavigation prevScreen = null;
                ICompleteGroup prevGroup = null;
                foreach (CompleteGroup g in innerGroups)
                {
                    var ng = new ScreenNavigation
                                 {
                                     CurrentScreenTitle = g.Title,
                                     PublicKey = g.PublicKey,
                                     PrevScreen = prevGroup == null ? null : new CompleteGroupHeaders(prevGroup),
                                     Parent = new CompleteGroupHeaders(item)
                                 };
                    if (g.PropogationPublicKey.HasValue)
                        ng.PropagateKey = g.PropogationPublicKey.Value;

                    if (prevGroup != null)
                        prevScreen.NextScreen = new CompleteGroupHeaders(g);

                    if (item.PublicKey == doc.PublicKey)
                    {
                        if (ng.NextScreen != null)
                            ng.NextScreen.IsExternal = true;
                        if (ng.PrevScreen != null)
                            ng.PrevScreen.IsExternal = true;
                        if (prevScreen != null && prevScreen.NextScreen != null)
                            prevScreen.NextScreen.IsExternal = true;
                    }
                    queue.Enqueue(g);
                    prevGroup = g;
                    prevScreen = ng;
                    navigations.Add(ng);
                }
            }


            var currentGroup = new CompleteGroupMobileView(doc, group, navigations);

            InitGroups(doc, currentGroup.PublicKey);
            Totals = CalcProgress(doc);
            CollectGalleries(currentGroup);
            CollectInstructions(currentGroup);
            CollectScreens(currentGroup);
            CurrentScreen = new CompleteGroupMobileView()
                                {
                                    Navigation = currentGroup.Navigation,
                                    Questions = currentGroup.Questions,
                                    Groups = currentGroup.Groups,
                                    GroupText = currentGroup.GroupText,
                                    PublicKey = currentGroup.PublicKey,
                                    Propagated = currentGroup.Propagated
                                };
        }

        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastEntryDate { get; set; }

        public SurveyStatus Status { get; set; }

        public List<CompleteQuestionView> QuestionsWithCards { get; set; }
        public List<CompleteQuestionView> QuestionsWithInstructions { get; set; }

        public CompleteGroupMobileView CurrentScreen { get; set; }
        public List<CompleteGroupMobileView> Screens { get; set; }
        public List<PropagatedGroup> Templates { get; set; }
        public List<PropagatedGroup> PropagatedScreens { get; set; }

        public UserLight Responsible { set; get; }

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
                                    Totals = CountQuestions(questions),
                                    IsExternal = true
                                };
                for (var i = 1; i <= groups.Count; i++)
                {
                    Groups[i] = new CompleteGroupHeaders
                                    {
                                        PublicKey = groups[i - 1].PublicKey,
                                        GroupText = groups[i - 1].Title,
                                        IsExternal = true
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
                                        GroupText = groups[i].Title,
                                        IsExternal = true
                                    };
                    Groups[i].Totals = CalcProgress(groups[i]);
                }
            }

            var current = Groups.FirstOrDefault(g => g.PublicKey == currentGroupPublicKey);
            current.IsCurrent = true;
            //CurrentGroup.Totals = current.Totals;
        }

        private void CollectScreens(CompleteGroupMobileView @group)
        {
            if (@group.Propagated == Propagate.None)
            {
                foreach (var g in @group.Groups)
                {
                    //if (g.PublicKey == Guid.Empty) continue;
                    Screens.Add(g);
                    CollectScreens(g);
                }
            }

            if (@group.PropagateTemplate != null)
                Templates.Add(@group.PropagateTemplate);
            foreach (var g in @group.PropagatedGroups)
            {
                PropagatedScreens.Add(g);
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

      //      var propagated = @group as PropagatableCompleteGroup;
            if (@group.PropogationPublicKey.HasValue)
            {
                total = total + CountQuestions(@group.Children.Select(q => q as ICompleteQuestion).ToList());
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
                                Answered = enabled.Count(question => question.Children.Any(a => a is ICompleteAnswer && ((ICompleteAnswer)a).Selected))
                            };
            return total;
        }
    }
}