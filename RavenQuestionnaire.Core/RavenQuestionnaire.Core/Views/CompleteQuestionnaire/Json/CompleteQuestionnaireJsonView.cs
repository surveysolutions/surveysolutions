using System;
using System.Linq;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Composite;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Entities.Extensions;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;
using RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Mobile;


namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Json
{
    public class CompleteQuestionnaireJsonView
    {
        private CompleteQuestionnaireJsonView()
        {
            Questions = new List<CompleteQuestionsJsonView>();
            InnerGroups=new List<CompleteGroupMobileView>();
        }
        public CompleteQuestionnaireJsonView(CompleteQuestionnaireStoreDocument doc, ICompleteGroup currentGroup)
            : this()
        {
            PublicKey = doc.PublicKey;
            Status = doc.Status;
            Responsible = doc.Responsible;
            CollectAll(doc, currentGroup as CompleteGroup);
        }

        public CompleteQuestionnaireJsonView(CompleteQuestionnaireStoreDocument doc)
            : this()
        {
            PublicKey = doc.PublicKey;
            Status = doc.Status;
            Responsible = doc.Responsible;
            var group = new CompleteGroup { Children = doc.Children.Where(c => c is ICompleteQuestion).ToList() };
            CollectAll(doc, group);
        }

        private void CollectAll(CompleteQuestionnaireStoreDocument doc, CompleteGroup group)
        {
            var executor = new CompleteQuestionnaireConditionExecutor(doc.QuestionHash);
            executor.Execute(group);

            var validator = new CompleteQuestionnaireValidationExecutor(doc.QuestionHash);
            validator.Execute(group);
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
                if (item.PropogationPublicKey.HasValue)
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
                    InnerGroups.Add(new CompleteGroupMobileView(doc, g, null));
                }
            }

            InitGroups(doc);
            Totals = CalcProgress(doc);
        }

        public Guid PublicKey { get; set; }

        public SurveyStatus Status { get; set; }

        public List<CompleteQuestionsJsonView> Questions { get; set; }

        public List<CompleteGroupMobileView> InnerGroups { get; set; }

        public UserLight Responsible { set; get; }

        public CompleteGroupHeaders[] Menu { get; set; }

        public Counter Totals { get; set; }

        protected void InitGroups(CompleteQuestionnaireStoreDocument doc)
        {
            var groups = doc.Children.OfType<ICompleteGroup>().ToList();

            Menu = new CompleteGroupHeaders[groups.Count];
            var executor = new CompleteQuestionnaireConditionExecutor(new GroupHash(doc));
            for (var i = 0; i < groups.Count; i++)
            {
                Menu[i] = new CompleteGroupHeaders
                                {
                                    PublicKey = groups[i].PublicKey,
                                    GroupText = groups[i].Title,
                                    Enabled = executor.Execute(groups[i])
                                };
                Menu[i].Totals = CalcProgress(groups[i]);
            }
        }

        private Counter CalcProgress(ICompleteGroup @group)
        {
            var total = new Counter();
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
                                Answered = enabled.Count(question => question.GetAnswerObject() != null)
                            };
            return total;
        }
    }
}