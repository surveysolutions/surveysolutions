using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Properties;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
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

        private readonly IPlainKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader;
       
        private readonly IExpressionProcessor expressionProcessor;

        private static readonly SelectOption[] AllQuestionScopeOptions =
        {
            new SelectOption { Value = "Interviewer", Text = "Interviewer" },
            new SelectOption { Value = "Supervisor", Text = "Supervisor" },
            new SelectOption { Value = "Hidden", Text = "Hidden" },
            new SelectOption { Value = "Prefilled", Text = "Prefilled" }
        };

        private static readonly Type[] QuestionsWhichCanBeUsedAsSourceOfLinkedQuestion = new[]
        {typeof (TextQuestion), typeof (NumericQuestion), typeof (DateTimeQuestion)};

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
            new SelectOption {Value = RosterType.Fixed.ToString(), Text = Roster.RosterType_Fixed},
            new SelectOption {Value = RosterType.List.ToString(), Text = Roster.RosterType_List},
            new SelectOption {Value = RosterType.Multi.ToString(), Text = Roster.RosterType_Multi},
            new SelectOption {Value = RosterType.Numeric.ToString(), Text = Roster.RosterType_Numeric}
        };

        public QuestionnaireInfoFactory(
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader,
            IExpressionProcessor expressionProcessor)
        {
            this.questionnaireDocumentReader = questionnaireDocumentReader;
            this.expressionProcessor = expressionProcessor;
        }

        public NewEditGroupView GetGroupEditView(string questionnaireId, Guid groupId)
        {
            var questionnaire = this.questionnaireDocumentReader.GetById(questionnaireId);
            var group = questionnaire?.Find<IGroup>(groupId);
            if (@group == null)
                return null;

            var result = new NewEditGroupView
            {
                Group = ReplaceGuidsInValidationAndConditionRules(new GroupDetailsView
                {
                    Id = group.PublicKey,
                    Title = group.Title,
                    EnablementCondition = group.ConditionExpression,
                    HideIfDisabled = group.HideIfDisabled,
                    VariableName = group.VariableName
                }, questionnaire, questionnaireId),
                Breadcrumbs = this.GetBreadcrumbs(questionnaire, group)
            };
            return result;
        }


        private GroupDetailsView ReplaceGuidsInValidationAndConditionRules(GroupDetailsView model, QuestionnaireDocument questionnaire, string questionnaireKey)
        {
            var expressionReplacer = new ExpressionReplacer(questionnaire);
            Guid questionnaireGuid = Guid.Parse(questionnaireKey);
            model.EnablementCondition = expressionReplacer.ReplaceGuidsWithStataCaptions(model.EnablementCondition, questionnaireGuid);
            return model;
        }

        public NewEditRosterView GetRosterEditView(string questionnaireId, Guid rosterId)
        {
            var questionnaire = this.questionnaireDocumentReader.GetById(questionnaireId);
            if (questionnaire == null)
                return null;
            questionnaire.ConnectChildrenWithParent();

            var roster = questionnaire.Find<IGroup>(rosterId);
            if (roster == null)
                return null;

            RosterType rosterType = this.getRosterType(questionnaire: questionnaire,
                rosterSizeSourceType: roster.RosterSizeSource, rosterSizeQuestionId: roster.RosterSizeQuestionId);

            var parentRosterScopeIds = GetRosterScopeIds(roster).Skip(1).ToArray();
            var result = new NewEditRosterView
            {
                ItemId = roster.PublicKey.FormatGuid(),
                Title = roster.Title,
                EnablementCondition = roster.ConditionExpression,
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

        private RosterType getRosterType(QuestionnaireDocument questionnaire,
            RosterSizeSourceType rosterSizeSourceType, Guid? rosterSizeQuestionId)
        {
            if (rosterSizeSourceType == RosterSizeSourceType.FixedTitles)
                return RosterType.Fixed;
            if (rosterSizeQuestionId.HasValue)
            {
                var rosterSizeQuestion =
                    questionnaire.Find<IQuestion>(rosterSizeQuestionId.Value);
                
                if (rosterSizeQuestion == null)
                    return RosterType.Numeric;
                else
                {
                    switch (rosterSizeQuestion.QuestionType)
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
            var questionnaire = this.questionnaireDocumentReader.GetById(questionnaireId);
            if (questionnaire == null)
                return null;
            questionnaire.ConnectChildrenWithParent();

            var question = questionnaire.Find<IQuestion>(questionId);
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

            this.ReplaceGuidsInValidationAndConditionRules(result, questionnaire, questionnaireId);

            return result;
        }

        public NewEditStaticTextView GetStaticTextEditView(string questionnaireId, Guid staticTextId)
        {
            var questionnaire = this.questionnaireDocumentReader.GetById(questionnaireId);

            var staticText = questionnaire?.Find<IStaticText>(staticTextId);
            if (staticText == null)
                return null;

            var result = new NewEditStaticTextView()
            {
                Id = staticText.PublicKey,
                Text = staticText.Text,
                AttachmentName = staticText.AttachmentName,
                EnablementCondition = staticText.ConditionExpression,
                HideIfDisabled = staticText.HideIfDisabled,
            }; 

            result.ValidationConditions.AddRange(staticText.ValidationConditions);
            result.Breadcrumbs = this.GetBreadcrumbs(questionnaire, staticText);

            return result;
        }

        public List<DropdownQuestionView> GetQuestionsEligibleForNumericRosterTitle(string questionnaireId, Guid rosterId, Guid rosterSizeQuestionId)
        {
            var questionnaire = this.questionnaireDocumentReader.GetById(questionnaireId);
            if (questionnaire == null)
                return null;

            questionnaire.ConnectChildrenWithParent();

            var roster = this.GetRoster(questionnaire, rosterId); 

            Func<IEnumerable<IQuestion>, IEnumerable<IQuestion>> questionFilter;

            var areTitlesForUnsavedRosterSizeRequested = roster.RosterSizeQuestionId != rosterSizeQuestionId;
            if (areTitlesForUnsavedRosterSizeRequested)
            {
                var prospectiveRosterScopeIds = GetRosterScopeIds(roster).Take(GetRosterScopeIds(roster).Length - 1).Union(rosterSizeQuestionId.ToEnumerable()).ToArray();
                questionFilter = q => q.Where(x => x.GetParent()?.PublicKey == rosterId || GetRosterScopeIds(x).SequenceEqual(prospectiveRosterScopeIds) && x.QuestionType != QuestionType.Multimedia).ToList();
            }
            else
            {
                questionFilter = q => q.Where(x => GetRosterScopeIds(x).SequenceEqual(GetRosterScopeIds(roster)) && x.QuestionType != QuestionType.Multimedia).ToList();
            }
             
            return this.PrepareGroupedQuestionsListForDropdown(questionnaire, questionFilter);
        }

        public VariableView GetVariableEditView(string questionnaireId, Guid variableId)
        {
            var questionnaire = this.questionnaireDocumentReader.GetById(questionnaireId);

            var variable = questionnaire?.Find<IVariable>(variableId);
            if (variable == null)
                return null;
            VariableView result = new VariableView()
            {
                Id = variable.PublicKey,
                ItemId = variable.PublicKey.FormatGuid(),
                VariableData = new VariableData(variable.Type, variable.Name, variable.Expression),
            };
            result.TypeOptions = VariableTypeOptions;
            result.Breadcrumbs = this.GetBreadcrumbs(questionnaire, variable);

            return result;
        }

        public List<QuestionnaireItemLink> GetAllBrokenGroupDependencies(string questionnaireId, Guid id)
        {
            var questionnaireDocument = this.questionnaireDocumentReader.GetById(questionnaireId);
            if (questionnaireDocument == null)
                return null;

            questionnaireDocument.ConnectChildrenWithParent();

            ReadOnlyQuestionnaireDocument questionnaire = new ReadOnlyQuestionnaireDocument(questionnaireDocument);

            var variablesToBeDeleted = questionnaire.Find<IQuestion>()
                .Where(x => this.GetParentGroupsIds(x).Contains(id))
                .Select(x => x.StataExportCaption)
                .ToList();

            var allReferencedQuestionsByExpressions = questionnaire.Find<IQuestion>()
                .Where(x => !this.GetParentGroupsIds(x).Contains(id))
                .Where(x => !string.IsNullOrEmpty(x.ConditionExpression) || x.ValidationConditions.Any(q => !string.IsNullOrEmpty(q.Expression)))
                .Where(x => this.expressionProcessor.GetIdentifiersUsedInExpression(x.ConditionExpression).Any(v => variablesToBeDeleted.Contains(v))
                         || x.ValidationConditions.SelectMany(q => this.expressionProcessor.GetIdentifiersUsedInExpression(q.Expression)).Any(v => variablesToBeDeleted.Contains(v)));

            var singleQuestionIdsToBeDeleted = questionnaire.Find<IQuestion>()
                .Where(x => this.GetParentGroupsIds(x).Contains(id))
                .Where(x => x.QuestionType == QuestionType.SingleOption)
                .Select(x => x.PublicKey)
                .ToList();

            var allCascadingDependentOutsideQuestions = questionnaire.Find<IQuestion>()
                .Where(x => x.QuestionType == QuestionType.SingleOption)
                .Where(x => !this.GetParentGroupsIds(x).Contains(id))
                .Where(x => x.CascadeFromQuestionId != null && singleQuestionIdsToBeDeleted.Contains(x.CascadeFromQuestionId.Value));


            var allQuestions = allReferencedQuestionsByExpressions.Concat(allCascadingDependentOutsideQuestions);
            return allQuestions.Select(x => new QuestionnaireItemLink
                                                      {
                                                          Id = x.PublicKey.FormatGuid(),
                                                          ChapterId = this.GetParentGroupsIds(x).Last().FormatGuid(),
                                                          Title = x.QuestionText
                                                      }).ToList();
        }

        private void ReplaceGuidsInValidationAndConditionRules(NewEditQuestionView model, QuestionnaireDocument questionnaire, string questionnaireKey)
        {
            var expressionReplacer = new ExpressionReplacer(questionnaire);
            Guid questionnaireGuid = Guid.Parse(questionnaireKey);
            model.EnablementCondition = expressionReplacer.ReplaceGuidsWithStataCaptions(model.EnablementCondition, questionnaireGuid);

            foreach (var validationExpression in model.ValidationConditions)
            {
                validationExpression.Expression = expressionReplacer.ReplaceGuidsWithStataCaptions(validationExpression.Expression, questionnaireGuid);
            }
        }

        private static NewEditQuestionView MapQuestionFields(IQuestion question)
        {
            var questionView = new NewEditQuestionView()
            {
                Title = question.QuestionText,
                Type = question.QuestionType,
                VariableLabel = question.VariableLabel,
                Id = question.PublicKey,
                ParentGroupId = question.GetParent().PublicKey,
                Instructions = question.Instructions,
                EnablementCondition = question.ConditionExpression,
                IsPreFilled = question.Featured,
                IsTimestamp = question.IsTimestamp,
                IsFilteredCombobox = question.IsFilteredCombobox,
                QuestionScope = question.QuestionScope,
                HideIfDisabled = question.HideIfDisabled,
                CascadeFromQuestionId = question.CascadeFromQuestionId?.FormatGuid(),
                LinkedFilterExpression = question.LinkedFilterExpression,
                HideInstructions = question.Properties?.HideInstructions ?? false,
                UseFormatting = question.Properties?.UseFormatting ?? false,
                OptionsFilterExpression = question.Properties?.OptionsFilterExpression,
                VariableName = question.StataExportCaption,
                LinkedToEntityId = (question.LinkedToQuestionId ?? question.LinkedToRosterId)?.FormatGuid(),
                QuestionTypeOptions = question.Answers.Select(a => new SelectOption() { Text = a.AnswerText, Value = a.AnswerValue}).ToArray(),
            };
            questionView.ValidationConditions.AddRange(question.ValidationConditions);

            questionView.RosterScopeIds = new Guid[0];
            questionView.ParentGroupsIds = new Guid[0];


            switch (question.QuestionType)
            {
                case QuestionType.MultyOption:
                    var multyOptionsQuestion = (IMultyOptionsQuestion)question;
                    questionView.YesNoView = multyOptionsQuestion.YesNoView;
                    questionView.AreAnswersOrdered = multyOptionsQuestion.AreAnswersOrdered;
                    questionView.MaxAllowedAnswers = multyOptionsQuestion.MaxAllowedAnswers;
                    questionView.LinkedToEntityId = multyOptionsQuestion.LinkedToQuestionId?.FormatGuid() ?? multyOptionsQuestion.LinkedToRosterId?.FormatGuid();
                    questionView.LinkedFilterExpression = multyOptionsQuestion.LinkedFilterExpression;
                    questionView.Options = CreateCategoricalOptions(multyOptionsQuestion.Answers);
                    questionView.OptionsFilterExpression = multyOptionsQuestion.Properties.OptionsFilterExpression;
                    return questionView;
                case QuestionType.TextList:
                    var textListQuestion = (ITextListQuestion) question;
                    questionView.MaxAnswerCount = textListQuestion.MaxAnswerCount;
                    return questionView;
                case QuestionType.Numeric:
                    var numericQuestion = (INumericQuestion)question;
                    questionView.CountOfDecimalPlaces = numericQuestion.CountOfDecimalPlaces;
                    questionView.IsInteger = numericQuestion.IsInteger;
                    questionView.UseFormatting = numericQuestion.UseFormatting;
                    return questionView;
                case QuestionType.SingleOption:
                    var singleoptionQuestion = (SingleQuestion)question;
                    questionView.LinkedToEntityId = singleoptionQuestion.LinkedToQuestionId?.FormatGuid() ??
                                            singleoptionQuestion.LinkedToRosterId?.FormatGuid();
                    questionView.LinkedFilterExpression = singleoptionQuestion.LinkedFilterExpression;
                    questionView.IsFilteredCombobox = singleoptionQuestion.IsFilteredCombobox;
                    questionView.CascadeFromQuestionId = singleoptionQuestion.CascadeFromQuestionId?.FormatGuid();
                    questionView.Options = CreateCategoricalOptions(singleoptionQuestion.Answers);
                    questionView.OptionsFilterExpression = singleoptionQuestion.Properties.OptionsFilterExpression;
                    return questionView;
                case QuestionType.Text:
                    var textQuestion = (TextQuestion)question;
                    questionView.Mask = textQuestion.Mask;
                    return questionView;
                case QuestionType.DateTime:
                    var dateTimeQuestion = (DateTimeQuestion)question;
                    questionView.IsTimestamp = dateTimeQuestion.IsTimestamp;
                    return questionView;
                case QuestionType.QRBarcode:
                case QuestionType.Multimedia:
                case QuestionType.GpsCoordinates:
                    return questionView;
            }
            return null;
        }

        private static CategoricalOption[] CreateCategoricalOptions(List<Answer> answers)
        {
            if (answers == null)
                return null;

            return GetValidAnswersCollection(answers.ToArray()).Select(x => new CategoricalOption
            {
                Title = x.AnswerText,
                Value = decimal.Parse(x.AnswerValue),
                ParentValue = string.IsNullOrWhiteSpace(x.ParentValue) || !x.ParentValue.IsDecimal() ? (decimal?)null : Convert.ToDecimal(x.ParentValue)
            }).ToArray();
        }

        private static Answer[] GetValidAnswersCollection(Answer[] answers)
        {
            if (answers == null)
                return null;

            foreach (var answer in answers)
            {
                if (string.IsNullOrWhiteSpace(answer.AnswerValue))
                {
                    answer.AnswerValue = (new Random().NextDouble() * 100).ToString("0.00");
                }
                if (string.IsNullOrWhiteSpace(answer.AnswerText))
                {
                    answer.AnswerText = "Option " + answer.AnswerValue;
                }
            }
            return answers;
        }

        private List<DropdownQuestionView> GetSourcesOfSingleQuestionBriefs(QuestionnaireDocument document, Guid questionId)
        {
            Func<IEnumerable<IQuestion>, IEnumerable<IQuestion>> questionFilter =
                x => x.Where(q => q.PublicKey != questionId)
                    .Where(q => q is SingleQuestion)
                    .Cast<SingleQuestion>()
                    .Where(q => !q.LinkedToRosterId.HasValue && !q.LinkedToQuestionId.HasValue)
                    .ToList();

            var result = this.PrepareGroupedQuestionsListForDropdown(document, questionFilter);

            return result;
        }

        private List<DropdownQuestionView> GetSourcesOfLinkedQuestionBriefs(
            QuestionnaireDocument document, Guid questionId)
        {
            var result = new List<DropdownQuestionView>();

            var rosters = document.Find<IGroup>().Where(g => g.IsRoster).ToList();

            foreach (var roster in rosters)
            {
                var rosterPlaceholder = this.CreateRosterBreadcrumbPlaceholder(document, roster);
                result.Add(rosterPlaceholder);

                var rosterTitlePlaceholder = this.CreateRosterTitlePlaceholder(roster, rosterPlaceholder, document);
                result.Add(rosterTitlePlaceholder);

                var questions = GetQuestionInsideRosterWhichCanBeUsedAsSourceOfLink(document, questionId, roster);
                result.AddRange(questions);
            }

            return result;
        }

        private List<DropdownQuestionView> GetQuestionInsideRosterWhichCanBeUsedAsSourceOfLink(
            QuestionnaireDocument document, Guid questionId, IGroup roster)
        {
            
            var pathInsideRoster = new[] {roster.PublicKey}.Union(this.GetParentGroupsIds(roster)).ToArray();
            var questions =
                document.Find<IQuestion>().Where(
                    q => QuestionsWhichCanBeUsedAsSourceOfLinkedQuestion.Contains(q.GetType()))
                    .Where(
                        q =>
                            pathInsideRoster.SequenceEqual(GetParentGroupsIds(q).Skip(GetParentGroupsIds(q).Length - pathInsideRoster.Length)) 
                            && GetRosterScopeIds(q).SequenceEqual(GetRosterScopeIds(roster))
                            && q.PublicKey != questionId)
                    .OrderBy(q => GetParentGroupsIds(q).Length)
                    .Select(q => new DropdownQuestionView
                    {
                        Id = q.PublicKey.FormatGuid(),
                        Title = q.QuestionText,
                        Breadcrumbs = GetBreadcrumbsAsString(document, q),
                        Type = q.QuestionType.ToString().ToLower(),
                        VarName = q.StataExportCaption
                    }).ToList();
            return questions;
        }

        private DropdownQuestionView CreateRosterTitlePlaceholder(IGroup roster,
            DropdownQuestionView rosterPlaceholder, QuestionnaireDocument document)
        {
            var rosterTitlePlaceholder = new DropdownQuestionView
            {
                Title = string.Format(Roster.RosterTitle, roster.Title),
                Id = roster.PublicKey.FormatGuid(),
                IsSectionPlaceHolder = false,
                Breadcrumbs = rosterPlaceholder.Title,
                Type = this.rosterType,
                VarName = roster.VariableName,
                QuestionType = GetRosterSourceType(roster, document)
            };
            return rosterTitlePlaceholder;
        }

        private static string GetRosterSourceType(IGroup roster, QuestionnaireDocument document)
        {
            return roster.RosterSizeQuestionId.HasValue
                ? document?.Find<IQuestion>(roster.RosterSizeQuestionId.Value)?.QuestionType.ToString().ToLower()
                : null;
        }

        private DropdownQuestionView CreateRosterBreadcrumbPlaceholder(QuestionnaireDocument document,
            IGroup roster)
        {
            var rosterPlaceholder = new DropdownQuestionView
            {
                Title = string.Join(" / ", this.GetBreadcrumbs(document, roster).Select(x => x.Title)),
                IsSectionPlaceHolder = true
            };
            return rosterPlaceholder;
        }

        private List<DropdownQuestionView> GetNumericIntegerTitles(QuestionnaireDocument document,
            IGroup roster)
        {
            var rosterSizeQuestion = roster.RosterSizeQuestionId;

            Func<IEnumerable<IQuestion>, IEnumerable<IQuestion>> questionFilter;

            if (rosterSizeQuestion.HasValue && this.IsQuestionIsNumeric(document, rosterSizeQuestion.Value))
            {
                questionFilter = q => q.Where(x => GetRosterScopeIds(x).SequenceEqual(GetRosterScopeIds(roster)) && x.QuestionType != QuestionType.Multimedia).ToList();
            }
            else
            {
                questionFilter = q => q.Where(x => x.GetParent()?.PublicKey == roster.PublicKey && x.QuestionType != QuestionType.Multimedia).ToList();
            }

            return this.PrepareGroupedQuestionsListForDropdown(document, questionFilter);
        }

        private bool IsQuestionIsNumeric(QuestionnaireDocument document, Guid questionId)
        {
            return document.Find<IQuestion>(questionId)?.QuestionType == QuestionType.Numeric;
        }

        private IGroup GetRoster(QuestionnaireDocument document, Guid rosterId)
        {
            var roster = document.Find<IGroup>(rosterId);
            if (roster?.IsRoster ?? false)
                return roster;
            return null;
        }

        private List<DropdownQuestionView> GetNumericIntegerQuestionBriefs(QuestionnaireDocument document, Guid[] rosterScopeIds)
        {
            Func<IEnumerable<IQuestion>, IEnumerable<IQuestion>> questionFilter =
                questions => questions
                    .OfType<INumericQuestion>()
                    .Where(x => x.IsInteger)
                    .Where(x => GetRosterScopeIds(x).Length <= rosterScopeIds.Length)
                    .Where(x => GetRosterScopeIds(x).All(rosterScopeIds.Contains))
                    .ToList();

            return this.PrepareGroupedQuestionsListForDropdown(document, questionFilter);
        }

        private List<DropdownQuestionView> GetNotLinkedMultiOptionQuestionBriefs(QuestionnaireDocument document, Guid[] rosterScopeIds)
        {
            Func<IEnumerable<IQuestion>, IEnumerable<IQuestion>> questionFilter =
                questions => questions
                    .OfType<IMultyOptionsQuestion>()
                    .Where(x => !x.LinkedToQuestionId.HasValue && !x.LinkedToRosterId.HasValue)
                    .Where(x => GetRosterScopeIds(x).Length <= rosterScopeIds.Length)
                    .Where(x => GetRosterScopeIds(x).All(rosterScopeIds.Contains))
                    .ToList();

            return this.PrepareGroupedQuestionsListForDropdown(document, questionFilter);
        }

        private List<DropdownQuestionView> GetTextListsQuestionBriefs(QuestionnaireDocument document, Guid[] rosterScopeIds)
        {
            Func<IEnumerable<IQuestion>, IEnumerable<IQuestion>> questionFilter =
                questions => questions
                    .OfType<ITextListQuestion>()
                    .Where(x => GetRosterScopeIds(x).Length <= rosterScopeIds.Length)
                    .Where(x => GetRosterScopeIds(x).All(rosterScopeIds.Contains))
                    .ToList();

            return this.PrepareGroupedQuestionsListForDropdown(document, questionFilter);
        }

        private List<DropdownQuestionView> PrepareGroupedQuestionsListForDropdown(QuestionnaireDocument document, Func<IEnumerable<IQuestion>, IEnumerable<IQuestion>> questionFilter)
        {
            var questions = questionFilter(document.Find<IQuestion>())
                .Select(q => new DropdownQuestionView
                {
                    Id = q.PublicKey.FormatGuid(),
                    Title = q.QuestionText,
                    Breadcrumbs = this.GetBreadcrumbsAsString(document, q),
                    Type = q.QuestionType.ToString().ToLower(),
                    VarName = q.StataExportCaption
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

        private Breadcrumb[] GetBreadcrumbs(QuestionnaireDocument document, IComposite entity)
        {
            List<IGroup> parents = new List<IGroup>();
            var parent = (IGroup)entity.GetParent();
            while (parent != null && parent != document)
            {
                parents.Add(parent);
                parent = (IGroup)parent.GetParent();
            }
            parents.Reverse();

            return parents.Select(x => new Breadcrumb
            {
                Id = x.PublicKey.FormatGuid(),
                Title = x.Title,
                IsRoster = x.IsRoster
            }).ToArray();
        }

        private string GetBreadcrumbsAsString(QuestionnaireDocument document, IComposite question)
        {
            return string.Join(" / ", GetBreadcrumbs(document, question).Select(x => x.Title));
        }

        private Guid[] GetRosterScopeIds(IComposite entity)
        {
            var rosterScopes = new List<Guid>();
            while (entity != null)
            {
                IGroup group = entity as IGroup;
                if (group?.IsRoster ?? false)
                {
                    rosterScopes.Add(@group.RosterSizeSource == RosterSizeSourceType.FixedTitles
                        ? @group.PublicKey
                        : @group.RosterSizeQuestionId.Value);
                }

                entity = (IGroup)entity.GetParent();
            }
            return rosterScopes.ToArray();
        }

        private Guid[] GetParentGroupsIds(IComposite entity)
        {
            List<IGroup> parents = new List<IGroup>();
            var parent = (IGroup)entity.GetParent();
            while (parent != null && !(parent is QuestionnaireDocument))
            {
                parents.Add(parent);
                parent = (IGroup)parent.GetParent();
            }
            //parents.Reverse();
            return parents.Select(x => x.PublicKey).ToArray();
        }
    }
}
