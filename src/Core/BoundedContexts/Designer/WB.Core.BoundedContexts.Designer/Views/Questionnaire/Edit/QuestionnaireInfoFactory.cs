using System;
using System.Collections.Generic;
using System.Linq;
using EmitMapper;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Properties;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.QuestionnaireEntities;

namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class QuestionnaireInfoFactory : IQuestionnaireInfoFactory
    {
        public class SelectOption
        {
            public string Value { get; set; }
            public string Text { get; set; }
        }

        private readonly IReadSideKeyValueStorage<QuestionsAndGroupsCollectionView> questionDetailsReader;

        private readonly IExpressionProcessor expressionProcessor;

        private static readonly SelectOption[] AllQuestionScopeOptions =
        {
            new SelectOption { Value = "Interviewer", Text = "Interviewer" },
            new SelectOption { Value = "Supervisor", Text = "Supervisor" },
            new SelectOption { Value = "Hidden", Text = "Hidden" },
            new SelectOption { Value = "Prefilled", Text = "Prefilled" }
        };

        private static readonly Type[] QuestionsWhichCanBeUsedAsSourceOfLinkedQuestion = new[]
        {typeof (TextDetailsView), typeof (NumericDetailsView), typeof (DateTimeDetailsView)};

        private static readonly SelectOption[] VariableTypeOptions =
        {
            new SelectOption
            {
                Value = VariableType.Boolean.ToString(),
                Text = "Boolean"
            },
            new SelectOption
            {
                Value = VariableType.Double.ToString(),
                Text = "Double"
            },
            new SelectOption
            {
                Value = VariableType.DateTime.ToString(),
                Text = "Date/Time"
            },
            new SelectOption
            {
                Value = VariableType.LongInteger.ToString(),
                Text = "Long Integer"
            },
            new SelectOption
            {
                Value = VariableType.String.ToString(),
                Text = "String"
            }
        };

        private static readonly SelectOption[] QuestionTypeOptions =
        {
            new SelectOption
            {
                Value = "SingleOption",
                Text = "Categorical: Single-select"
            },
            new SelectOption
            {
                Value = "MultyOption",
                Text = "Categorical: Multi-select"
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
                Text = "GPS"
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
                Text = "Barcode"
            },
            new SelectOption
            {
                Value = "Multimedia",
                Text = "Picture"
            }
        };

        private readonly string rosterType = "roster";

        private static readonly SelectOption[] RosterTypeOptions =
        {
            new SelectOption() {Value = RosterType.Fixed.ToString(), Text = Properties.Roster.RosterType_Fixed},
            new SelectOption() {Value = RosterType.List.ToString(), Text = Properties.Roster.RosterType_List},
            new SelectOption() {Value = RosterType.Multi.ToString(), Text = Properties.Roster.RosterType_Multi},
            new SelectOption() {Value = RosterType.Numeric.ToString(), Text = Properties.Roster.RosterType_Numeric}
        };

        public QuestionnaireInfoFactory(IReadSideKeyValueStorage<QuestionsAndGroupsCollectionView> questionDetailsReader,
            IExpressionProcessor expressionProcessor)
        {
            this.questionDetailsReader = questionDetailsReader;
            this.expressionProcessor = expressionProcessor;
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
                    HideIfDisabled = group.HideIfDisabled,
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

            var parentRosterScopeIds = roster.RosterScopeIds.Skip(1).ToArray();
            var result = new NewEditRosterView
            {
                ItemId = roster.Id.FormatGuid(),
                Title = roster.Title,
                EnablementCondition = roster.EnablementCondition,
                HideIfDisabled = roster.HideIfDisabled,
                VariableName = roster.VariableName,

                Type = rosterType,
                RosterSizeListQuestionId = rosterType == RosterType.List ? roster.RosterSizeQuestionId.FormatGuid() : null,
                RosterSizeNumericQuestionId = rosterType == RosterType.Numeric ? roster.RosterSizeQuestionId.FormatGuid() : null,
                RosterSizeMultiQuestionId = rosterType == RosterType.Multi ? roster.RosterSizeQuestionId.FormatGuid() : null,
                RosterTitleQuestionId = roster.RosterTitleQuestionId.FormatGuid(),
                FixedRosterTitles = roster.FixedRosterTitles.ToArray(),
                RosterTypeOptions = RosterTypeOptions,

                NotLinkedMultiOptionQuestions = this.GetNotLinkedMultiOptionQuestionBriefs(questionnaire, parentRosterScopeIds),
                NumericIntegerQuestions = this.GetNumericIntegerQuestionBriefs(questionnaire, parentRosterScopeIds),
                NumericIntegerTitles = this.GetNumericIntegerTitles(questionnaire, roster),
                TextListsQuestions = this.GetTextListsQuestionBriefs(questionnaire, parentRosterScopeIds),
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
                
                if (rosterSizeQuestion == null)
                    return RosterType.Numeric;
                else
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
            var questions = questionnaire?.Questions.ToList();
            QuestionDetailsView question = questions?.FirstOrDefault(x => x.Id == questionId);
            if (question == null)
                return null;

            NewEditQuestionView result = MapQuestionFields(question);
            result.Options = result.Options ?? new CategoricalOption[0];
            result.OptionsCount = result.Options.Length;
            result.Breadcrumbs = this.GetBreadcrumbs(questionnaire, question);
            result.SourceOfLinkedEntities = this.GetSourcesOfLinkedQuestionBriefs(questionnaire, questionId);
            result.SourceOfSingleQuestions = this.GetSourcesOfSingleQuestionBriefs(questionnaire, questionId);
            result.QuestionTypeOptions = QuestionTypeOptions;
            result.AllQuestionScopeOptions = AllQuestionScopeOptions;
            result.ValidationConditions.AddRange(question.ValidationConditions);

            this.ReplaceGuidsInValidationAndConditionRules(result, questionnaire, questionnaireId);

            return result;
        }

        public NewEditStaticTextView GetStaticTextEditView(string questionnaireId, Guid staticTextId)
        {
            QuestionsAndGroupsCollectionView questionnaire = this.questionDetailsReader.GetById(questionnaireId);

            var staticTexts = questionnaire?.StaticTexts.ToList();
            StaticTextDetailsView staticTextDetailsView = staticTexts?.FirstOrDefault(x => x.Id == staticTextId);
            if (staticTextDetailsView == null)
                return null;

            var result = ObjectMapperManager.DefaultInstance.GetMapper<StaticTextDetailsView, NewEditStaticTextView>()
                            .Map(staticTextDetailsView);

            result.ValidationConditions.AddRange(staticTextDetailsView.ValidationConditions);
            result.Breadcrumbs = this.GetBreadcrumbs(questionnaire, staticTextDetailsView);

            return result;
        }

        public List<DropdownQuestionView> GetQuestionsEligibleForNumericRosterTitle(string questionnaireId, Guid rosterId, Guid rosterSizeQuestionId)
        {
            var questionnaire = this.questionDetailsReader.GetById(questionnaireId);
            if (questionnaire == null)
                return null;

            var roster = this.GetRoster(questionnaire, rosterId);

            Func<List<QuestionDetailsView>, List<QuestionDetailsView>> questionFilter;

            var areTitlesForUnsavedRosterSizeRequested = roster.RosterSizeQuestionId != rosterSizeQuestionId;
            if (areTitlesForUnsavedRosterSizeRequested)
            {
                var prospectiveRosterScopeIds = roster.RosterScopeIds.Take(roster.RosterScopeIds.Length - 1).Union(rosterSizeQuestionId.ToEnumerable()).ToArray();
                questionFilter = q => q.Where(x => x.ParentGroupId == rosterId || x.RosterScopeIds.SequenceEqual(prospectiveRosterScopeIds) && x.Type != QuestionType.Multimedia).ToList();
            }
            else
            {
                questionFilter = q => q.Where(x => x.RosterScopeIds.SequenceEqual(roster.RosterScopeIds) && x.Type != QuestionType.Multimedia).ToList();
            }
             
            return this.PrepareGroupedQuestionsListForDropdown(questionnaire, questionFilter);
        }

        public VariableView GetVariableEditView(string questionnaireId, Guid variableId)
        {
            QuestionsAndGroupsCollectionView questionnaire = this.questionDetailsReader.GetById(questionnaireId);

            var variables = questionnaire?.Variables.ToList();
            VariableView result = variables?.FirstOrDefault(x => x.Id == variableId);
            if (result == null)
                return null;

            result.TypeOptions = VariableTypeOptions;
            result.Breadcrumbs = this.GetBreadcrumbs(questionnaire, result);

            return result;
        }

        public List<QuestionnaireItemLink> GetAllBrokenGroupDependencies(string questionnaireId, Guid id)
        {
            var questionnaire = this.questionDetailsReader.GetById(questionnaireId);
            if (questionnaire == null)
                return null;

            var variablesToBeDeleted = questionnaire.Questions
                .Where(x => x.ParentGroupsIds.Contains(id))
                .Select(x => x.VariableName)
                .ToList();

            var allReferencedQuestionsByExpressions = questionnaire.Questions
                .Where(x => !x.ParentGroupsIds.Contains(id))
                .Where(x => !string.IsNullOrEmpty(x.EnablementCondition) || x.ValidationConditions.Any(q => !string.IsNullOrEmpty(q.Expression)))
                .Where(x => this.expressionProcessor.GetIdentifiersUsedInExpression(x.EnablementCondition).Any(v => variablesToBeDeleted.Contains(v))
                         || x.ValidationConditions.SelectMany(q => this.expressionProcessor.GetIdentifiersUsedInExpression(q.Expression)).Any(v => variablesToBeDeleted.Contains(v)));

            var singleQuestionIdsToBeDeleted = questionnaire.Questions
                .Where(x => x.ParentGroupsIds.Contains(id))
                .Where(x => x.Type == QuestionType.SingleOption)
                .Select(x => x.Id)
                .ToList();

            var allCascadingDependentOutsideQestions = questionnaire.Questions
                .OfType<SingleOptionDetailsView>()
                .Where(x => !x.ParentGroupsIds.Contains(id))
                .Where(x => x.Type == QuestionType.SingleOption)
                .Where(x => x.CascadeFromQuestionId != null && singleQuestionIdsToBeDeleted.Contains(x.CascadeFromQuestionId.Value));


            var allQuestions = allReferencedQuestionsByExpressions.Concat(allCascadingDependentOutsideQestions);
            return allQuestions.Select(x => new QuestionnaireItemLink
                                                      {
                                                          Id = x.Id.FormatGuid(),
                                                          ChapterId = x.ParentGroupsIds.Last().FormatGuid(),
                                                          Title = x.Title
                                                      }).ToList();
        }

        private void ReplaceGuidsInValidationAndConditionRules(NewEditQuestionView model, QuestionsAndGroupsCollectionView questionnaire, string questionnaireKey)
        {
            var expressionReplacer = new ExpressionReplacer(questionnaire);
            Guid questionnaireGuid = Guid.Parse(questionnaireKey);
            model.EnablementCondition = expressionReplacer.ReplaceGuidsWithStataCaptions(model.EnablementCondition, questionnaireGuid);

            foreach (var validationExpression in model.ValidationConditions)
            {
                validationExpression.Expression = expressionReplacer.ReplaceGuidsWithStataCaptions(validationExpression.Expression, questionnaireGuid);
            }
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
                    newEditQuestionView.LinkedToEntityId = Monads.Maybe(() => multiOptionDetailsView.LinkedToEntityId.FormatGuid());
                    newEditQuestionView.LinkedFilterExpression = Monads.Maybe(() => multiOptionDetailsView.LinkedFilterExpression);
                    return
                        newEditQuestionView;
                case QuestionType.SingleOption:
                    var singleOptionDetailsView = question as SingleOptionDetailsView;
                    var editQuestionView = ObjectMapperManager.DefaultInstance
                                                              .GetMapper<SingleOptionDetailsView, NewEditQuestionView>()
                                                              .Map(singleOptionDetailsView);
                    editQuestionView.LinkedToEntityId = Monads.Maybe(() => singleOptionDetailsView.LinkedToEntityId.FormatGuid());
                    editQuestionView.LinkedFilterExpression = Monads.Maybe(() => singleOptionDetailsView.LinkedFilterExpression);
                    editQuestionView.CascadeFromQuestionId = Monads.Maybe(() => singleOptionDetailsView.CascadeFromQuestionId.FormatGuid());
                    return editQuestionView;
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
                case QuestionType.Multimedia:
                    return ObjectMapperManager.DefaultInstance.GetMapper<MultimediaDetailsView, NewEditQuestionView>()
                        .Map(question as MultimediaDetailsView);
            }
            return null;
        }

        private List<DropdownQuestionView> GetSourcesOfSingleQuestionBriefs(QuestionsAndGroupsCollectionView questionsCollection, Guid questionId)
        {
            Func<List<QuestionDetailsView>, List<QuestionDetailsView>> questionFilter =
                x => x.Where(q => q.Id != questionId)
                    .Where(q => q is SingleOptionDetailsView)
                    .Where(q => !(q as SingleOptionDetailsView).LinkedToEntityId.HasValue)
                    .ToList();

            var result = this.PrepareGroupedQuestionsListForDropdown(questionsCollection, questionFilter);

            return result;
        }

        private List<DropdownQuestionView> GetSourcesOfLinkedQuestionBriefs(
            QuestionsAndGroupsCollectionView questionsCollection, Guid questionId)
        {
            var result = new List<DropdownQuestionView>();

            var rosters = questionsCollection.Groups.Where(g => g.IsRoster).ToList();

            foreach (var roster in rosters)
            {
                var rosterPlaceholder = this.CreateRosterBreadcrumbPlaceholder(questionsCollection, roster);
                result.Add(rosterPlaceholder);

                var rosterTitlePlaceholder = this.CreateRosterTitlePlaceholder(roster, rosterPlaceholder, questionsCollection);
                result.Add(rosterTitlePlaceholder);

                var questions = GetQuestionInsideRosterWhichCanBeUsedAsSourceOfLink(questionsCollection, questionId, roster);
                result.AddRange(questions);
            }

            return result;
        }

        private List<DropdownQuestionView> GetQuestionInsideRosterWhichCanBeUsedAsSourceOfLink(
            QuestionsAndGroupsCollectionView questionsCollection, Guid questionId, GroupAndRosterDetailsView roster)
        {
            
            var pathInsideRoster = new[] {roster.Id}.Union(roster.ParentGroupsIds).ToArray();
            var questions =
                questionsCollection.Questions.Where(
                    q => QuestionsWhichCanBeUsedAsSourceOfLinkedQuestion.Contains(q.GetType()))
                    .Where(
                        q =>
                            pathInsideRoster.SequenceEqual(q.ParentGroupsIds.Skip(q.ParentGroupsIds.Length - pathInsideRoster.Length)) 
                            && q.RosterScopeIds.SequenceEqual(roster.RosterScopeIds)
                            && q.Id != questionId)
                    .OrderBy(q => q.ParentGroupsIds.Length)
                    .Select(q => new DropdownQuestionView
                    {
                        Id = q.Id.FormatGuid(),
                        Title = q.Title,
                        Breadcrumbs = GetBreadcrumbsAsString(questionsCollection,q),
                        Type = q.Type.ToString().ToLower(),
                        VarName = q.VariableName
                    }).ToList();
            return questions;
        }

        private DropdownQuestionView CreateRosterTitlePlaceholder(GroupAndRosterDetailsView roster,
            DropdownQuestionView rosterPlaceholder, QuestionsAndGroupsCollectionView questionsCollection)
        {
            var rosterTitlePlaceholder = new DropdownQuestionView
            {
                Title = string.Format(Roster.RosterTitle, roster.Title),
                Id = roster.Id.FormatGuid(),
                IsSectionPlaceHolder = false,
                Breadcrumbs = rosterPlaceholder.Title,
                Type = this.rosterType,
                VarName = roster.VariableName,
                QuestionType = GetRosterSourceType(roster, questionsCollection)
            };
            return rosterTitlePlaceholder;
        }

        private static string GetRosterSourceType(GroupAndRosterDetailsView roster,
            QuestionsAndGroupsCollectionView questionsCollection) => roster.RosterSizeQuestionId.HasValue
                ? questionsCollection?.Questions?.Find(x => x.Id == roster.RosterSizeQuestionId.Value)?
                    .Type.ToString()
                    .ToLower()
                : null;

        private DropdownQuestionView CreateRosterBreadcrumbPlaceholder(QuestionsAndGroupsCollectionView questionsCollection,
            GroupAndRosterDetailsView roster)
        {
            var rosterPlaceholder = new DropdownQuestionView
            {
                Title =
                    string.Join(" / ",
                        this.GetBreadcrumbs(questionsCollection, roster)
                            .Select(x => x.Title)),
                IsSectionPlaceHolder = true
            };
            return rosterPlaceholder;
        }

        private List<DropdownQuestionView> GetNumericIntegerTitles(QuestionsAndGroupsCollectionView questionsCollection,
            GroupAndRosterDetailsView roster)
        {
            var rosterSizeQuestion = roster.RosterSizeQuestionId;

            Func<List<QuestionDetailsView>, List<QuestionDetailsView>> questionFilter;

            if (rosterSizeQuestion.HasValue && this.IsQuestionIsNumeric(questionsCollection, rosterSizeQuestion.Value))
            {
                Guid? rosterSizeQuestionId = rosterSizeQuestion.Value;
                Func<List<QuestionDetailsView>, List<QuestionDetailsView>> questionFilter1 = q => q.Where(x => x.RosterScopeIds.SequenceEqual(roster.RosterScopeIds) && x.Type != QuestionType.Multimedia).ToList();
                return this.PrepareGroupedQuestionsListForDropdown(questionsCollection, questionFilter1);
            }
            else
            {
                questionFilter = q => q.Where(x => x.ParentGroupId == roster.Id && x.Type != QuestionType.Multimedia).ToList();
            }

            return this.PrepareGroupedQuestionsListForDropdown(questionsCollection, questionFilter);
        }

        private bool IsQuestionIsNumeric(QuestionsAndGroupsCollectionView questionsCollection, Guid questionId)
        {
            return questionsCollection.Questions.OfType<NumericDetailsView>().Any(x => x.Id == questionId);
        }

        private GroupAndRosterDetailsView GetRoster(QuestionsAndGroupsCollectionView questionnaire, Guid rosterId)
        {
            return questionnaire.Groups.FirstOrDefault(x => x.Id == rosterId);
        }

        private List<DropdownQuestionView> GetNumericIntegerQuestionBriefs(QuestionsAndGroupsCollectionView questionsCollection, Guid[] rosterScopeIds)
        {
            Func<List<QuestionDetailsView>, List<QuestionDetailsView>> questionFilter =
                questions => questions
                    .OfType<NumericDetailsView>()
                    .Where(x => x.IsInteger)
                    .Where(x => x.RosterScopeIds.Length <= rosterScopeIds.Length)
                    .Where(x => x.RosterScopeIds.All(rosterScopeIds.Contains))
                    .Cast<QuestionDetailsView>().ToList();

            return this.PrepareGroupedQuestionsListForDropdown(questionsCollection, questionFilter);
        }

        private List<DropdownQuestionView> GetNotLinkedMultiOptionQuestionBriefs(QuestionsAndGroupsCollectionView questionsCollection, Guid[] rosterScopeIds)
        {
            Func<List<QuestionDetailsView>, List<QuestionDetailsView>> questionFilter =
                questions => questions
                    .OfType<MultiOptionDetailsView>()
                    .Where(x => x.LinkedToEntityId == null)
                    .Where(x => x.RosterScopeIds.Length <= rosterScopeIds.Length)
                    .Where(x => x.RosterScopeIds.All(rosterScopeIds.Contains))
                    .Cast<QuestionDetailsView>().ToList();

            return this.PrepareGroupedQuestionsListForDropdown(questionsCollection, questionFilter);
        }

        private List<DropdownQuestionView> GetTextListsQuestionBriefs(QuestionsAndGroupsCollectionView questionsCollection, Guid[] rosterScopeIds)
        {
            Func<List<QuestionDetailsView>, List<QuestionDetailsView>> questionFilter =
                questions => questions
                    .OfType<TextListDetailsView>()
                    .Where(x => x.RosterScopeIds.Length <= rosterScopeIds.Length)
                    .Where(x => x.RosterScopeIds.All(rosterScopeIds.Contains))
                    .Cast<QuestionDetailsView>().ToList();

            return this.PrepareGroupedQuestionsListForDropdown(questionsCollection, questionFilter);
        }

        private List<DropdownQuestionView> PrepareGroupedQuestionsListForDropdown(QuestionsAndGroupsCollectionView questionsCollection, Func<List<QuestionDetailsView>, List<QuestionDetailsView>> questionFilter)
        {
            var questions = questionFilter(questionsCollection.Questions)
                .Select(q => new DropdownQuestionView
                {
                    Id = q.Id.FormatGuid(),
                    Title = q.Title,
                    Breadcrumbs = this.GetBreadcrumbsAsString(questionsCollection, q),
                    Type = q.Type.ToString().ToLower(),
                    VarName = q.VariableName
                }).ToArray();


            var groupedQuestionsList = questions.GroupBy(x => x.Breadcrumbs);
            var result = new List<DropdownQuestionView>();

            foreach (var brief in groupedQuestionsList)
            {
                var sectionPlaceholder = new DropdownQuestionView
                {
                    Title = brief.Key,
                    IsSectionPlaceHolder = true
                };

                result.Add(sectionPlaceholder);
                result.AddRange(brief.Select(question => new DropdownQuestionView
                {
                    Title = question.Title,
                    Id = question.Id,
                    IsSectionPlaceHolder = false,
                    Breadcrumbs = brief.Key,
                    Type = question.Type,
                    VarName = question.VarName
                }));
            }
            return result;
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
