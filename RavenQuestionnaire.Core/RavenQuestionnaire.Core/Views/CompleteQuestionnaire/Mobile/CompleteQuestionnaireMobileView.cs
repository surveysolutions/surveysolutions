using System;
using System.Linq;
using System.Collections.Generic;
using RavenQuestionnaire.Core.Utility;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities.Extensions;
using RavenQuestionnaire.Core.ExpressionExecutors;
using RavenQuestionnaire.Core.Entities.SubEntities;
using RavenQuestionnaire.Core.Entities.SubEntities.Complete;

namespace RavenQuestionnaire.Core.Views.CompleteQuestionnaire.Mobile
{
    public class CompleteQuestionnaireMobileView
    {
        #region Properties

        public string Id { get; set; }
        public string Title { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastEntryDate { get; set; }
        public SurveyStatus Status { get; set; }
        public CompleteGroupMobileView CurrentScreen { get; set; }
        public UserLight Responsible { set; get; }
        public CompleteGroupHeaders[] Groups { get; set; }
        public Counter Totals { get; set; }

        #endregion

        #region Constructor
        
        private CompleteQuestionnaireMobileView()
        {
          
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
       //     CollectAll(doc, screenPublicKey, currentGroup as CompleteGroup, navigation);
        }

        public CompleteQuestionnaireMobileView(CompleteQuestionnaireDocument doc, Guid screenPublicKey, ICompleteGroup currentGroup, ScreenNavigation navigation)
            : this(doc)
        {
            CollectAll(doc, screenPublicKey, currentGroup as CompleteGroup, navigation);
        }

        #endregion

        #region PrivateMethod
        
        private void CollectAll(CompleteQuestionnaireDocument doc, Guid screenPublicKey, CompleteGroup group, ScreenNavigation navigation)
        {
            var executor = new CompleteQuestionnaireConditionExecutor(doc.QuestionHash);
            executor.Execute(group);
            var currentGroup = new CompleteGroupMobileView(doc, group, navigation);
            InitGroups(doc, screenPublicKey);
            Totals = CalcProgress(doc);
            CurrentScreen = currentGroup.Propagated != Propagate.None ? currentGroup.PropagateTemplate : currentGroup;
        }

        protected void InitGroups(CompleteQuestionnaireDocument doc, Guid currentGroupPublicKey)
        {
            var questions = doc.Children.OfType<ICompleteQuestion>().ToList();
            var groups = doc.Children.OfType<ICompleteGroup>().ToList();
            var executor = new CompleteQuestionnaireConditionExecutor(new GroupHash(doc));
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
                                        GroupText = groups[i - 1].Title,
                                        Enabled = executor.Execute(groups[i-1])
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
                                        Enabled = executor.Execute(groups[i])
                                    };
                    Groups[i].Totals = CalcProgress(groups[i]);
                }
            }

            var current = Groups.FirstOrDefault(g => g.PublicKey == currentGroupPublicKey);
            current.IsCurrent = true;
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
                                Answered = enabled.Count(question => question.Answer != null)
                            };
            return total;
        }

        #endregion
    }
}