using System;
using System.Collections.Generic;
using System.Linq;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Views.Question;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Json
{
    public class CompleteQuestionnaireJsonView
    {
        private CompleteQuestionnaireJsonView()
        {
            Questions = new List<CompleteQuestionsJsonView>();
        }
        public CompleteQuestionnaireJsonView(CompleteQuestionnaireDocument doc, ICompleteGroup currentGroup)
            : this()
        {
            Id = IdUtil.ParseId(doc.Id);
            Status = doc.Status;
            Responsible = doc.Responsible;

            CollectAll(doc, currentGroup as CompleteGroup);
        }

        public CompleteQuestionnaireJsonView(CompleteQuestionnaireDocument doc)
            : this()
        {

            Id = IdUtil.ParseId(doc.Id);
            Status = doc.Status;
            Responsible = doc.Responsible;

            var group = new CompleteGroup { Children = doc.Children.Where(c => c is ICompleteQuestion).ToList() };

            CollectAll(doc, group);
        }

        private void CollectAll(CompleteQuestionnaireDocument doc, CompleteGroup group)
        {
            var queue = new Queue<ICompleteGroup>();
            queue.Enqueue(group);
            while (queue.Count != 0)
            {
                ICompleteGroup item = queue.Dequeue();
                if (!item.PropogationPublicKey.HasValue && item.Propagated == Propagate.Propagated)
                {
                    continue;
                }
                var parentKey = item.PublicKey;
                var propagatable = false;
                if (item.PropogationPublicKey.HasValue )
                {
                    parentKey = item.PropogationPublicKey.Value;
                    propagatable = true;
                }
                var questions = item.Children.OfType<ICompleteQuestion>().ToList();
                foreach (var question in questions)
                {
                    Questions.Add(new CompleteQuestionsJsonView(question, parentKey, propagatable));
                }

                List<IComposite> innerGroups = item.Children.Where(c => c is ICompleteGroup).ToList();
                foreach (CompleteGroup g in innerGroups)
                {
                    queue.Enqueue(g);
                }
            }

            InitGroups(doc, group.PublicKey);
            Totals = CalcProgress(doc);
        }

        public string Id { get; set; }

        public SurveyStatus Status { get; set; }

        public List<CompleteQuestionsJsonView> Questions { get; set; }
        
        public UserLight Responsible { set; get; }

        public CompleteGroupHeaders[] Menu { get; set; }

        public Counter Totals { get; set; }

        protected void InitGroups(CompleteQuestionnaireDocument doc, Guid currentGroupPublicKey)
        {
            var questions = doc.Children.OfType<ICompleteQuestion>().ToList();
            var groups = doc.Children.OfType<ICompleteGroup>().ToList();
            if (questions.Count > 0)
            {
                Menu = new CompleteGroupHeaders[groups.Count + 1];

                Menu[0] = new CompleteGroupHeaders
                                {
                                    PublicKey = Guid.Empty,
                                    GroupText = "Main",
                                    Totals = CountQuestions(questions),
                                    IsExternal = true
                                };
                for (var i = 1; i <= groups.Count; i++)
                {
                    Menu[i] = new CompleteGroupHeaders
                                    {
                                        PublicKey = groups[i - 1].PublicKey,
                                        GroupText = groups[i - 1].Title,
                                        IsExternal = true
                                    };
                    Menu[i].Totals = CalcProgress(groups[i - 1]);
                }
            }
            else
            {
                Menu = new CompleteGroupHeaders[groups.Count];
                for (var i = 0; i < groups.Count; i++)
                {
                    Menu[i] = new CompleteGroupHeaders
                                    {
                                        PublicKey = groups[i].PublicKey,
                                        GroupText = groups[i].Title,
                                        IsExternal = true
                                    };
                    Menu[i].Totals = CalcProgress(groups[i]);
                }
            }

            var current = Menu.FirstOrDefault(g => g.PublicKey == currentGroupPublicKey);
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
                                Answered = enabled.Count(question => question.Answer != null)
                            };
            return total;
        }
    }
}