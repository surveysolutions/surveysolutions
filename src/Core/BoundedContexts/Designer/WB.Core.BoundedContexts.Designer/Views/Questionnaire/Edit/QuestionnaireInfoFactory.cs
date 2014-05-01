﻿using System;
using System.Collections.Generic;
using System.Linq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class QuestionnaireInfoFactory : IQuestionnaireInfoFactory
    {
        public class SelectOption
        {
            public string Value { get; set; }
            public string Text { get; set; }
        }

        private readonly IReadSideRepositoryReader<QuestionsAndGroupsCollectionView> questionDetailsReader;

        private readonly SelectOption[] questionScopeOptions = new SelectOption[]
        {
            new SelectOption
            {
                Value = "Interviewer",
                Text = "Interviewer"
            },
            new SelectOption
            {
                Value ="Supervisor",
                Text ="Supervisor"
            },
            new SelectOption
            {
                Value = "Headquarter",
                Text = "Headquarters"
            }
        };

        private readonly SelectOption[] questionTypeOptopns = new SelectOption[]
        {
            new SelectOption
            {
                Value = "SingleOption",
                Text = "Categorical: one answer"
            },
            new SelectOption
            {
                Value = "MultyOption",
                Text = "Categorical: multiple answers"
            },
            new SelectOption
            {
                Value = "Numeric",
                Text = "Numeric"
            },
            new SelectOption
            {
                Value = "DateTime",
                Text = "Date"
            },
            new SelectOption
            {
                Value = "Text",
                Text = "Text"
            },
            new SelectOption
            {
                Value = "GpsCoordinates",
                Text = "Geo Location"
            }
            ,
            new SelectOption
            {
                Value = "TextList",
                Text = "List"
            },
            new SelectOption
            {
                Value = "QRBarcode",
                Text = "QR Barcode"
            }
        };

        public QuestionnaireInfoFactory(IReadSideRepositoryReader<QuestionsAndGroupsCollectionView> questionDetailsReader)
        {
            this.questionDetailsReader = questionDetailsReader;
        }

        public NewEditGroupView GetGroupEditView(string questionnaireId, Guid groupId)
        {
            var questionnaire = this.questionDetailsReader.GetById(questionnaireId);
            if (questionnaire == null)
                return null;
            var group = questionnaire.Groups.FirstOrDefault(x => x.Id == groupId);
            if (group == null)
                return null;
            var result = new NewEditGroupView
            {
                Group = new GroupDetailsView
                {
                    Id = group.Id,
                    Title = group.Title,
                    EnablementCondition = group.EnablementCondition,
                    Description = group.Description
                },
                Breadcrumbs = this.GetBreadcrumbs(questionnaire, group)
            };
            return result;
        }

        public NewEditRosterView GetRosterEditView(string questionnaireId, Guid rosterId)
        {
            var questionnaire = this.questionDetailsReader.GetById(questionnaireId);
            if (questionnaire == null)
                return null;
            var roster = questionnaire.Groups.FirstOrDefault(x => x.Id == rosterId);
            if (roster == null)
                return null;
            var result = new NewEditRosterView
            {
                Roster = new RosterDetailsView
                {
                    Id = roster.Id,
                    Title = roster.Title,
                    EnablementCondition = roster.EnablementCondition,
                    Description = roster.Description,
                    RosterFixedTitles = roster.RosterFixedTitles,
                    RosterSizeQuestionId = roster.RosterSizeQuestionId,
                    RosterSizeSourceType = roster.RosterSizeSourceType,
                    RosterTitleQuestionId = roster.RosterTitleQuestionId
                },
                NotLinkedMultiOptionQuestions = this.GetNotLinkedMultiOptionQuestionBriefs(questionnaire),
                NumericIntegerQuestions = this.GetNumericIntegerQuestionBriefs(questionnaire),
                TextListsQuestions = this.GetTextListsQuestionBriefs(questionnaire),
                Breadcrumbs = this.GetBreadcrumbs(questionnaire, roster)
            };

            return result;
        }

        public NewEditQuestionView GetQuestionEditView(string questionnaireId, Guid questionId)
        {
            var questionnaire = this.questionDetailsReader.GetById(questionnaireId);
            if (questionnaire == null)
                return null;
            var question = questionnaire.Questions.FirstOrDefault(x => x.Id == questionId);
            if (question == null)
                return null;
            var result = new NewEditQuestionView
            {
                Question = question,
                Breadcrumbs = this.GetBreadcrumbs(questionnaire,question),
                SourceOfLinkedQuestions = this.GetSourcesOfLinkedQuestionBriefs(questionnaire),
                QuestionTypeOptopns = this.questionTypeOptopns,
                QuestionScopeOptions = this.questionScopeOptions
            };

            return result;
        }

        private Dictionary<string, QuestionBrief[]> GetSourcesOfLinkedQuestionBriefs(QuestionsAndGroupsCollectionView questionsCollection)
        {
            var questions = questionsCollection
                .Questions
                .Where(q => q is TextDetailsView || q is NumericDetailsView || q is DateTimeDetailsView || q is GeoLocationDetailsView)
                .Where(q => q.RosterScopeIds.Length > 0)
                .Select(q => new
                {
                    Id = q.Id,
                    Title = q.Title,
                    Breadcrumbs = this.GetBreadcrumbsAsString(questionsCollection, q)
                }).ToArray();

            return questions.GroupBy(x => x.Breadcrumbs).ToDictionary(g => g.Key, g => g.Select(x => new QuestionBrief
            {
                Id = x.Id,
                Title = x.Title
            }).ToArray());
        }

        private Dictionary<string, QuestionBrief[]> GetNumericIntegerQuestionBriefs(QuestionsAndGroupsCollectionView questionsCollection)
        {
            var questions = questionsCollection
                .Questions.OfType<NumericDetailsView>()
                .Where(q => q.IsInteger)
                .Select(q => new
                {
                    Id = q.Id,
                    Title = q.Title,
                    Breadcrumbs = this.GetBreadcrumbsAsString(questionsCollection, q)
                }).ToArray();

            return questions.GroupBy(x => x.Breadcrumbs).ToDictionary(g => g.Key, g => g.Select(x => new QuestionBrief
            {
                Id = x.Id,
                Title = x.Title
            }).ToArray());
        }

        private Dictionary<string, QuestionBrief[]> GetNotLinkedMultiOptionQuestionBriefs(
            QuestionsAndGroupsCollectionView questionsCollection)
        {
            var questions = questionsCollection
                .Questions.OfType<MultiOptionDetailsView>()
                .Where(q => q.LinkedToQuestionId == null)
                .Select(q => new
                {
                    Id = q.Id,
                    Title = q.Title,
                    Breadcrumbs = this.GetBreadcrumbsAsString(questionsCollection, q)
                }).ToArray();

            return questions.GroupBy(x => x.Breadcrumbs).ToDictionary(g => g.Key, g => g.Select(x => new QuestionBrief
            {
                Id = x.Id,
                Title = x.Title
            }).ToArray());
        }

        private Dictionary<string, QuestionBrief[]> GetTextListsQuestionBriefs(QuestionsAndGroupsCollectionView questionsCollection)
        {
            var questions = questionsCollection
                .Questions.OfType<TextListDetailsView>()
                .Select(q => new
                {
                    Id = q.Id,
                    Title = q.Title,
                    Breadcrumbs = this.GetBreadcrumbsAsString(questionsCollection, q)
                }).ToArray();

            return questions.GroupBy(x => x.Breadcrumbs).ToDictionary(g => g.Key, g => g.Select(x => new QuestionBrief
            {
                Id = x.Id,
                Title = x.Title
            }).ToArray());
        }

        private GroupBrief[] GetBreadcrumbs(QuestionsAndGroupsCollectionView questionsCollection, DescendantItemView question)
        {
            return question.ParentGroupsIds.Reverse().Select(x => questionsCollection.Groups.Single(g => g.Id == x)).Select(x => new GroupBrief
            {
                Id = x.Id,
                Title = x.Title
            }).ToArray();
        }

        private string GetBreadcrumbsAsString(QuestionsAndGroupsCollectionView questionsCollection, QuestionDetailsView question)
        {
            return string.Join(" / ", GetBreadcrumbs(questionsCollection, question).Select(x => x.Title));
        }
    }
}
