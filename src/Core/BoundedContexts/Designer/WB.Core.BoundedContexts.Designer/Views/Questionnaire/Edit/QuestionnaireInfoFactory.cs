using System;
using System.Collections.Generic;
using System.Linq;
using EmitMapper;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.GenericSubdomains.Utils;
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

        private static readonly SelectOption[] QuestionScopeOptions =
        {
            new SelectOption
            {
                Value = "Interviewer",
                Text = "Interviewer"
            },
            new SelectOption
            {
                Value = "Supervisor",
                Text = "Supervisor"
            },
            new SelectOption
            {
                Value = "Prefilled",
                Text = "Prefilled"
            },
        };

        private static readonly SelectOption[] QuestionTypeOptions =
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

        private static readonly SelectOption[] RosterTypeOptions =
        {
            new SelectOption() {Value = RosterType.Fixed.ToString(), Text = Properties.Roster.RosterType_Fixed},
            new SelectOption() {Value = RosterType.List.ToString(), Text = Properties.Roster.RosterType_List},
            new SelectOption() {Value = RosterType.Multi.ToString(), Text = Properties.Roster.RosterType_Multi},
            new SelectOption() {Value = RosterType.Numeric.ToString(), Text = Properties.Roster.RosterType_Numeric}
        };

        public QuestionnaireInfoFactory(IReadSideRepositoryReader<QuestionsAndGroupsCollectionView> questionDetailsReader)
        {
            this.questionDetailsReader = questionDetailsReader;
        }

        public NewEditGroupView GetGroupEditView(string questionnaireId, Guid groupId)
        {
            QuestionsAndGroupsCollectionView questionnaire = this.questionDetailsReader.GetById(questionnaireId);
            if (questionnaire == null)
                return null;
            var group = questionnaire.Groups.FirstOrDefault(x => x.Id == groupId);
            if (group == null)
                return null;
            var result = new NewEditGroupView
            {
                Group = ReplaceGuidsInValidationAndConditionRules(new GroupDetailsView
                {
                    Id = group.Id,
                    Title = group.Title,
                    EnablementCondition = group.EnablementCondition,
                    Description = group.Description,
                    VariableName = group.VariableName
                }, questionnaire, questionnaireId),
                Breadcrumbs = this.GetBreadcrumbs(questionnaire, group)
            };
            return result;
        }


        private GroupDetailsView ReplaceGuidsInValidationAndConditionRules(GroupDetailsView model, QuestionsAndGroupsCollectionView questionnaire, string questionnaireKey)
        {
            var expressionReplacer = new ExpressionReplacer(questionnaire);
            Guid questionnaireGuid = Guid.Parse(questionnaireKey);
            model.EnablementCondition = expressionReplacer.ReplaceGuidsWithStataCaptions(model.EnablementCondition, questionnaireGuid);
            return model;
        }

        public NewEditRosterView GetRosterEditView(string questionnaireId, Guid rosterId)
        {
            var questionnaire = this.questionDetailsReader.GetById(questionnaireId);
            if (questionnaire == null)
                return null;
            var roster = questionnaire.Groups.FirstOrDefault(x => x.Id == rosterId);
            if (roster == null)
                return null;

            RosterType rosterType = this.getRosterType(questionnaire: questionnaire,
                rosterSizeSourceType: roster.RosterSizeSourceType, rosterSizeQuestionId: roster.RosterSizeQuestionId);

            var result = new NewEditRosterView
            {
                ItemId = roster.Id.FormatGuid(),
                Title = roster.Title,
                EnablementCondition = roster.EnablementCondition,
                Description = roster.Description,
                VariableName = roster.VariableName,

                Type = rosterType,
                RosterSizeListQuestionId = rosterType == RosterType.List ? roster.RosterSizeQuestionId : null,
                RosterSizeNumericQuestionId=rosterType == RosterType.Numeric ? roster.RosterSizeQuestionId : null,
                RosterSizeMultiQuestionId = rosterType == RosterType.Multi ? roster.RosterSizeQuestionId : null,
                RosterTitleQuestionId = roster.RosterTitleQuestionId,
                RosterFixedTitles = roster.RosterFixedTitles,
                RosterTypeOptions = RosterTypeOptions,

                NotLinkedMultiOptionQuestions = this.GetNotLinkedMultiOptionQuestionBriefs(questionnaire),
                NumericIntegerQuestions = this.GetNumericIntegerQuestionBriefs(questionnaire),
                NumericIntegerTitles = this.GetNumericIntegerTitles(questionnaire, roster),
                TextListsQuestions = this.GetTextListsQuestionBriefs(questionnaire),
                Breadcrumbs = this.GetBreadcrumbs(questionnaire, roster)
            };

            return result;
        }

        private RosterType getRosterType(QuestionsAndGroupsCollectionView questionnaire,
            RosterSizeSourceType rosterSizeSourceType, Guid? rosterSizeQuestionId)
        {
            if (rosterSizeSourceType == RosterSizeSourceType.FixedTitles)
                return RosterType.Fixed;
            if (rosterSizeQuestionId.HasValue)
            {
                var rosterSizeQuestion =
                    questionnaire.Questions.Find(question => question.Id == rosterSizeQuestionId.Value);

                if (rosterSizeQuestion != null)
                {
                    switch (rosterSizeQuestion.Type)
                    {
                        case QuestionType.MultyOption:
                            return RosterType.Multi;
                        case QuestionType.Numeric:
                            return RosterType.Numeric;
                        case QuestionType.TextList:
                            return RosterType.List;
                    }
                }
            }

            return RosterType.Fixed;
        }

        public NewEditQuestionView GetQuestionEditView(string questionnaireId, Guid questionId)
        {
            QuestionsAndGroupsCollectionView questionnaire = this.questionDetailsReader.GetById(questionnaireId);
            if (questionnaire == null)
                return null;
            QuestionDetailsView question = questionnaire.Questions.FirstOrDefault(x => x.Id == questionId);
            if (question == null)
                return null;
            
            NewEditQuestionView result = MapQuestionFields(question);
            result.Options = result.Options ?? new CategoricalOption[0];
            result.Breadcrumbs = this.GetBreadcrumbs(questionnaire, question);
            result.SourceOfLinkedQuestions = this.GetSourcesOfLinkedQuestionBriefs(questionnaire);
            result.QuestionTypeOptions = QuestionTypeOptions;
            result.QuestionScopeOptions = QuestionScopeOptions;

            this.ReplaceGuidsInValidationAndConditionRules(result, questionnaire, questionnaireId);

            return result;
        }

        public NewEditStaticTextView GetStaticTextEditView(string questionnaireId, Guid staticTextId)
        {
            QuestionsAndGroupsCollectionView questionnaire = this.questionDetailsReader.GetById(questionnaireId);
            if (questionnaire == null)
                return null;
            StaticTextDetailsView staticTextDetailsView = questionnaire.StaticTexts.FirstOrDefault(x => x.Id == staticTextId);
            if (staticTextDetailsView == null)
                return null;

            var result = ObjectMapperManager.DefaultInstance.GetMapper<StaticTextDetailsView, NewEditStaticTextView>()
                            .Map(staticTextDetailsView);
            result.Breadcrumbs = this.GetBreadcrumbs(questionnaire, staticTextDetailsView);
            
            return result;
        }

        private void ReplaceGuidsInValidationAndConditionRules(NewEditQuestionView model, QuestionsAndGroupsCollectionView questionnaire, string questionnaireKey)
        {
            var expressionReplacer = new ExpressionReplacer(questionnaire);
            Guid questionnaireGuid = Guid.Parse(questionnaireKey);
            model.EnablementCondition = expressionReplacer.ReplaceGuidsWithStataCaptions(model.EnablementCondition, questionnaireGuid);
            model.ValidationExpression = expressionReplacer.ReplaceGuidsWithStataCaptions(model.ValidationExpression, questionnaireGuid);
        }

        private static NewEditQuestionView MapQuestionFields(QuestionDetailsView question)
        {
            switch (question.Type)
            {
                case QuestionType.MultyOption:
                    var multiOptionDetailsView = question as MultiOptionDetailsView;
                    var newEditQuestionView = ObjectMapperManager.DefaultInstance
                                                                 .GetMapper<MultiOptionDetailsView, NewEditQuestionView>()
                                                                 .Map(multiOptionDetailsView);
                    newEditQuestionView.LinkedToQuestionId = Monads.Maybe(() => multiOptionDetailsView.LinkedToQuestionId.FormatGuid());
                    return
                        newEditQuestionView;
                case QuestionType.SingleOption:
                    var singleOptionDetailsView = question as SingleOptionDetailsView;
                    var editQuestionView = ObjectMapperManager.DefaultInstance
                                                              .GetMapper<SingleOptionDetailsView, NewEditQuestionView>()
                                                              .Map(singleOptionDetailsView);
                    editQuestionView.LinkedToQuestionId = Monads.Maybe(() => singleOptionDetailsView.LinkedToQuestionId.FormatGuid());
                    return
                        editQuestionView;
                case QuestionType.Text:
                    return
                        ObjectMapperManager.DefaultInstance.GetMapper<TextDetailsView, NewEditQuestionView>()
                            .Map(question as TextDetailsView);
                case QuestionType.DateTime:
                    return
                        ObjectMapperManager.DefaultInstance.GetMapper<DateTimeDetailsView, NewEditQuestionView>()
                            .Map(question as DateTimeDetailsView);
                case QuestionType.Numeric:
                    return
                        ObjectMapperManager.DefaultInstance.GetMapper<NumericDetailsView, NewEditQuestionView>()
                            .Map(question as NumericDetailsView);
                case QuestionType.GpsCoordinates:
                    return ObjectMapperManager.DefaultInstance.GetMapper<GeoLocationDetailsView, NewEditQuestionView>()
                        .Map(question as GeoLocationDetailsView);
                case QuestionType.TextList:
                    return
                        ObjectMapperManager.DefaultInstance.GetMapper<TextListDetailsView, NewEditQuestionView>()
                            .Map(question as TextListDetailsView);
                case QuestionType.QRBarcode:
                    return ObjectMapperManager.DefaultInstance.GetMapper<QrBarcodeDetailsView, NewEditQuestionView>()
                        .Map(question as QrBarcodeDetailsView);
            }
            return null;
        }

        private List<LinkedQuestionSource> GetSourcesOfLinkedQuestionBriefs(QuestionsAndGroupsCollectionView questionsCollection)
        {
            var questions = questionsCollection
                .Questions
                .Where(q => q is TextDetailsView || q is NumericDetailsView || q is DateTimeDetailsView || q is GeoLocationDetailsView)
                .Where(q => q.RosterScopeIds.Length > 0)
                .Select(q => new
                {
                    Id = q.Id,
                    Title = q.Title,
                    Breadcrumbs = this.GetBreadcrumbsAsString(questionsCollection, q),
                    Type = q.Type
                }).ToArray();


            var sourcesOfLinkedQuestionBriefs = questions.GroupBy(x => x.Breadcrumbs);
            var result = new List<LinkedQuestionSource>();

            foreach (var brief in sourcesOfLinkedQuestionBriefs)
            {
                var linkedQuestionSource = new LinkedQuestionSource {
                    Title = brief.Key, 
                    IsSectionPlaceHolder = true
                };

                result.Add(linkedQuestionSource);
                result.AddRange(brief.Select(question => new LinkedQuestionSource
                {
                    Title = question.Title, 
                    Id = question.Id.FormatGuid(), 
                    IsSectionPlaceHolder = false,
                    Breadcrumbs = brief.Key,
                    Type = question.Type.ToString().ToLower()
                }));
            }

            return result;
        }

        private Dictionary<string, QuestionBrief[]> GetNumericIntegerTitles(QuestionsAndGroupsCollectionView questionsCollection, GroupAndRosterDetailsView roster)
        {
            var questions = questionsCollection
                .Questions
                .Where(q => q.ParentGroupId == roster.Id)
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

        private Breadcrumb[] GetBreadcrumbs(QuestionsAndGroupsCollectionView entitiesCollection, DescendantItemView entity)
        {
            return entity.ParentGroupsIds.Reverse().Select(x => entitiesCollection.Groups.Single(g => g.Id == x)).Select(x => new Breadcrumb
            {
                Id = x.Id.FormatGuid(),
                Title = x.Title,
                IsRoster = x.IsRoster
            }).ToArray();
        }

        private string GetBreadcrumbsAsString(QuestionsAndGroupsCollectionView questionsCollection, QuestionDetailsView question)
        {
            return string.Join(" / ", GetBreadcrumbs(questionsCollection, question).Select(x => x.Title));
        }
    }
}
