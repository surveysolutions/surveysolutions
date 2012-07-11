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
            CurrentGroup = new CompleteGroupViewV(doc, currentGroup as CompleteGroup);
            InitGroups(doc, CurrentGroup.PublicKey);
            Totals = CalcProgress(doc);
        }

        public CompleteQuestionnaireViewV(CompleteQuestionnaireDocument doc)
        {
            Id = IdUtil.ParseId(doc.Id);
            Title = doc.Title;
            CreationDate = doc.CreationDate;
            LastEntryDate = doc.LastEntryDate;
            Status = doc.Status;
            Responsible = doc.Responsible;

            var group = new CompleteGroup { Children = doc.Children.Where(c=> c is ICompleteQuestion).ToList() };
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
            var questions = doc.Children.OfType<ICompleteQuestion>().ToList();
            var groups = doc.Children.OfType<ICompleteGroup>().ToList();
            if (questions.Count > 0)
            {
                Groups = new CompleteGroupHeaders[groups.Count + 1];

                Groups[0] = new CompleteGroupHeaders
                                {
                                    PublicKey = Guid.Empty,
                                    GroupText = "Main",

                                };
                for (var i = 1; i <= groups.Count; i++)
                {
                    Groups[i] = new CompleteGroupHeaders
                                    {
                                        PublicKey = groups[i - 1].PublicKey,
                                        GroupText = groups[i - 1].Title
                                    };
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
                }
            }
            var current = Groups.FirstOrDefault(g => g.PublicKey == currentGroupPublicKey);
            current.IsCurrent = true;
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

            total = total + CountQuestions(@group.Children.Select(q => q as ICompleteQuestion).ToList());

            return @group.Children.OfType<ICompleteGroup>().Aggregate(total, (current, g) => current + CalcProgress(g as ICompleteGroup));
        }

        private Counter CountQuestions(List<ICompleteQuestion> questions)
        {
            if (questions == null || questions.Count == 0)
                return new Counter();

            var enabled = questions.Where(q =>q!=null && q.Enabled).ToList();

            var total = new Counter
                            {
                                Total = questions.Count,
                                Enablad = enabled.Count(),
                                Answered = enabled.Count(question => question.Children.Any(a => a is ICompleteAnswer &&  ((ICompleteAnswer)a).Selected))
                            };
            return total;
        }
    }
}