using System;
using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Utility;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Vertical
{
    public class CompleteQuestionnaireViewV
    {
        public CompleteQuestionnaireViewV(CompleteQuestionnaireDocument doc, ICompleteGroup currentGroup)
        {
            Id = IdUtil.ParseId(doc.Id);
            Title = doc.Title;
            CreationDate = doc.CreationDate;
            LastEntryDate = doc.LastEntryDate;
            Status = doc.Status;
            Responsible = doc.Responsible;

            var cg = currentGroup as ICompleteGroup<ICompleteGroup, ICompleteQuestion>;
            CurrentGroup = new CompleteGroupViewV(doc, currentGroup as CompleteGroup);

            InitGroups(doc, CurrentGroup.PublicKey);
            Totals = CalcProgress(doc);
        }

        public CompleteQuestionnaireViewV(CompleteQuestionnaireDocument doc)
        {
            Title = doc.Title;
            Title = doc.Title;
            CreationDate = doc.CreationDate;
            LastEntryDate = doc.LastEntryDate;
            Status = doc.Status;
            Responsible = doc.Responsible;

            var group = new CompleteGroup { Questions = doc.Questions };
            CurrentGroup = new CompleteGroupViewV(doc, group);
            InitGroups(doc, CurrentGroup.PublicKey);

            Totals = CalcProgress(doc);
        }

        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastEntryDate { get; set; }

        public SurveyStatus Status { get; set; }

        public UserLight Responsible { set; get; }
        public CompleteGroupViewV CurrentGroup { get; set; }
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

    public class Counter
    {
        public Counter()
        {
            Total = 0;
            Enablad = 0;
            Answered = 0;
        }
        public int Total { get; set; }
        public int Enablad { get; set; }
        public int Answered { get; set; }
        public int Progress
        {
            get
            {
                if (Enablad < 1)
                    return 0;
                return (int)Math.Round(100 * ((double)Answered / Enablad));
            }
        }

        public static Counter operator +(Counter a, Counter b)
        {
            return new Counter
            {
                Total = a.Total + b.Total,
                Enablad = a.Enablad + b.Enablad,
                Answered = a.Answered + b.Answered
            };
        }
    }
}