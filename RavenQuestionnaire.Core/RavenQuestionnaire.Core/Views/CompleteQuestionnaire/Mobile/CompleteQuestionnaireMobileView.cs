using System;
using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.Documents;
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

            var cg = currentGroup as ICompleteGroup<ICompleteGroup, ICompleteQuestion>;
            CurrentGroup = new CompleteGroupMobileView(doc, currentGroup as CompleteGroup);

            InitGroups(doc, CurrentGroup.PublicKey);
            Totals = CalcProgress(doc);
            CollectGalleries(CurrentGroup);
            CollectInstructions(CurrentGroup);
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

            var group = new CompleteGroup { Questions = doc.Questions };
            CurrentGroup = new CompleteGroupMobileView(doc, group);
            InitGroups(doc, CurrentGroup.PublicKey);

            Totals = CalcProgress(doc);
            CollectGalleries(CurrentGroup);
            CollectInstructions(CurrentGroup);

        }

        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastEntryDate { get; set; }

        public SurveyStatus Status { get; set; }

        public List<CompleteQuestionView> QuestionsWithCards { get; set; }
        public List<CompleteQuestionView> QuestionsWithInstructions { get; set; }

        public UserLight Responsible { set; get; }
        public CompleteGroupMobileView CurrentGroup { get; set; }
        public CompleteGroupHeaders[] Groups { get; set; }
        public Counter Totals { get; set; }

        protected void InitGroups(CompleteQuestionnaireDocument doc, Guid currentGroupPublicKey)
        {
            if (doc.Questions.Count > 0)
            {
                Groups = new CompleteGroupHeaders[doc.Groups.Count + 1];

                Groups[0] = new CompleteGroupHeaders
                                {
                                    PublicKey = Guid.Empty,
                                    GroupText = "Main",

                                };
                for (var i = 1; i <= doc.Groups.Count; i++)
                {
                    Groups[i] = new CompleteGroupHeaders
                                    {
                                        PublicKey = doc.Groups[i - 1].PublicKey,
                                        GroupText = doc.Groups[i - 1].Title
                                    };
                }
            }
            else
            {
                Groups = new CompleteGroupHeaders[doc.Groups.Count];
                for (var i = 0; i < doc.Groups.Count; i++)
                {
                    Groups[i] = new CompleteGroupHeaders
                                    {
                                        PublicKey = doc.Groups[i].PublicKey,
                                        GroupText = doc.Groups[i].Title
                                    };
                }
            }
            var current = Groups.FirstOrDefault(g => g.PublicKey == currentGroupPublicKey);
            current.IsCurrent = true;
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

        private Counter CalcProgress(ICompleteGroup<ICompleteGroup, ICompleteQuestion> @group)
        {
            var total = new Counter();

            var propagated = @group as PropagatableCompleteGroup;
            if (propagated != null)
            {
                total = total + CountQuestions(propagated.Questions.Select(q => q as ICompleteQuestion<ICompleteAnswer>).ToList());
                return total;
            }
            var complete = @group as CompleteGroup;
            if (complete != null && complete.Propagated != Propagate.None)
                return total;

            total = total + CountQuestions(@group.Questions.Select(q => q as ICompleteQuestion<ICompleteAnswer>).ToList());

            foreach (var g in @group.Groups)
            {
                total = total + CalcProgress(g as ICompleteGroup<ICompleteGroup, ICompleteQuestion>);
            }
            return total;
        }

        private Counter CountQuestions(List<ICompleteQuestion<ICompleteAnswer>> questions)
        {
            if (questions == null || questions.Count == 0)
                return new Counter();

            var enabled = questions.Where(q => q.Enabled).ToList();

            var total = new Counter
                            {
                                Total = questions.Count,
                                Enablad = enabled.Count(),
                                Answered = enabled.Count(question => question.Answers.Any(a => a.Selected))
                            };
            return total;
        }
    }
}