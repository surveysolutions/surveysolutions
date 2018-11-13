using System;
using System.Collections.Generic;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.Designer.Implementation.Services;
using WB.Core.BoundedContexts.Designer.Resources;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.Questionnaire.Documents;
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

        private SelectOption[] AllQuestionScopeOptions => new []
        {
            new SelectOption {Value = "Interviewer", Text = QuestionnaireEditor.QuestionScopeInterviewer},
            new SelectOption {Value = "Supervisor", Text = QuestionnaireEditor.QuestionScopeSupervisor},
            new SelectOption {Value = "Hidden", Text = QuestionnaireEditor.QuestionScopeHidden},
            new SelectOption {Value = "Identifying", Text = QuestionnaireEditor.QuestionScopeIdentifying}
        };

        private static readonly HashSet<QuestionType> QuestionsWhichCanBeUsedAsSourceOfLinkedQuestion = new HashSet<QuestionType>
        { QuestionType.Text, QuestionType.Numeric, QuestionType.DateTime };

        private SelectOption[] VariableTypeOptions => new []
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

        private SelectOption[] GeometryTypeOptions => new[]
        {
            new SelectOption
            {
                Value = GeometryType.Polygon.ToString(),
                Text = "Polygon"
            },
            new SelectOption
            {
                Value = GeometryType.Polyline.ToString(),
                Text = "Polyline"
            },

            new SelectOption
            {
                Value = GeometryType.Point.ToString(),
                Text = "Point"
            },

            new SelectOption
            {
                Value = GeometryType.Multipoint.ToString(),
                Text = "Multipoint"
            }
        };

        private SelectOption[] QuestionTypeOptions => new []
        {
            new SelectOption
            {
                Value = "SingleOption",
                Text = QuestionnaireEditor.QuestionTypeSingleSelect
            },
            new SelectOption
            {
                Value = "MultyOption",
                Text = QuestionnaireEditor.QuestionTypeMultiSelect
            },
            new SelectOption
            {
                Value = "Numeric",
                Text = QuestionnaireEditor.QuestionTypeNumeric
            },
            new SelectOption
            {
                Value = "DateTime",
                Text = QuestionnaireEditor.QuestionTypeDate
            },
            new SelectOption
            {
                Value = "Text",
                Text = QuestionnaireEditor.QuestionTypeText
            },
            new SelectOption
            {
                Value = "GpsCoordinates",
                Text = QuestionnaireEditor.QuestionTypeGPS
            }
            ,
            new SelectOption
            {
                Value = "TextList",
                Text = QuestionnaireEditor.QuestionTypeList
            },
            new SelectOption
            {
                Value = "QRBarcode",
                Text = QuestionnaireEditor.QuestionTypeBarcode
            },
            new SelectOption
            {
                Value = "Multimedia",
                Text = QuestionnaireEditor.QuestionTypePicture
            },
            new SelectOption
            {
                Value = "Audio",
                Text = QuestionnaireEditor.QuestionTypeAudio
            },
            new SelectOption
            {
                Value = "Area",
                Text = QuestionnaireEditor.QuestionTypeGeography
            }
        };

        private readonly string rosterType = "roster";

        private SelectOption[] RosterTypeOptions => new[]
        {
            new SelectOption {Value = RosterType.Fixed.ToString(), Text = Roster.RosterType_Fixed},
            new SelectOption {Value = RosterType.List.ToString(), Text = Roster.RosterType_List},
            new SelectOption {Value = RosterType.Multi.ToString(), Text = Roster.RosterType_Multi},
            new SelectOption {Value = RosterType.Numeric.ToString(), Text = Roster.RosterType_Numeric}
        };

        private static readonly string BreadcrumbSeparator = " / ";

        public QuestionnaireInfoFactory(
            IPlainKeyValueStorage<QuestionnaireDocument> questionnaireDocumentReader,
            IExpressionProcessor expressionProcessor)
        {
            this.questionnaireDocumentReader = questionnaireDocumentReader;
            this.expressionProcessor = expressionProcessor;
        }

        public NewEditGroupView GetGroupEditView(string questionnaireId, Guid groupId)
        {
            var document = this.questionnaireDocumentReader.GetById(questionnaireId);
            var group = document?.Find<IGroup>(groupId);
            if (@group == null)
                return null;

            ReadOnlyQuestionnaireDocument questionnaire = new ReadOnlyQuestionnaireDocument(document);

            var result = new NewEditGroupView
            {
                Group = new GroupDetailsView
                {
                    Id = group.PublicKey,
                    Title = group.Title,
                    EnablementCondition = group.ConditionExpression,
                    HideIfDisabled = group.HideIfDisabled,
                    VariableName = group.VariableName
                },
                Breadcrumbs = this.GetBreadcrumbs(questionnaire, group)
            };
            return result;
        }


        public NewEditRosterView GetRosterEditView(string questionnaireId, Guid rosterId)
        {
            var document = this.questionnaireDocumentReader.GetById(questionnaireId);

            var roster = document?.Find<IGroup>(rosterId);
            if (roster == null)
                return null;

            ReadOnlyQuestionnaireDocument questionnaire = new ReadOnlyQuestionnaireDocument(document);

            RosterType rosterType = this.getRosterType(questionnaire: questionnaire,
                rosterSizeSourceType: roster.RosterSizeSource, rosterSizeQuestionId: roster.RosterSizeQuestionId);

            var rosterScope = questionnaire.GetRosterScope(roster);

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

                NotLinkedMultiOptionQuestions = this.GetNotLinkedMultiOptionQuestionBriefs(questionnaire, rosterScope),
                NumericIntegerQuestions = this.GetNumericIntegerQuestionBriefs(questionnaire, rosterScope),
                NumericIntegerTitles = this.GetNumericIntegerTitles(questionnaire, roster),
                TextListsQuestions = this.GetTextListsQuestionBriefs(questionnaire, rosterScope),
                Breadcrumbs = this.GetBreadcrumbs(questionnaire, roster)
            };

            return result;
        }

        public NewEditQuestionView GetQuestionEditView(string questionnaireId, Guid questionId)
        {
            var document = this.questionnaireDocumentReader.GetById(questionnaireId);

            var question = document?.Find<IQuestion>(questionId);
            if (question == null)
                return null;

            ReadOnlyQuestionnaireDocument questionnaire = new ReadOnlyQuestionnaireDocument(document);

            NewEditQuestionView result = MapQuestionFields(question);
            result.Options = result.Options ?? new CategoricalOption[0];
            result.OptionsCount = result.Options.Length;
            result.Breadcrumbs = this.GetBreadcrumbs(questionnaire, question);
            result.SourceOfLinkedEntities = this.GetSourcesOfLinkedQuestionBriefs(questionnaire, questionId);
            result.SourceOfSingleQuestions = this.GetSourcesOfSingleQuestionBriefs(questionnaire, questionId);
            result.QuestionTypeOptions = QuestionTypeOptions;
            result.AllQuestionScopeOptions = AllQuestionScopeOptions;
            result.GeometryTypeOptions = GeometryTypeOptions;

            this.ReplaceGuidsInValidationAndConditionRules(result, questionnaire, questionnaireId);

            return result;
        }

        public NewEditStaticTextView GetStaticTextEditView(string questionnaireId, Guid staticTextId)
        {
            var document = this.questionnaireDocumentReader.GetById(questionnaireId);

            var staticText = document?.Find<IStaticText>(staticTextId);
            if (staticText == null)
                return null;

            ReadOnlyQuestionnaireDocument questionnaire = new ReadOnlyQuestionnaireDocument(document);

            var result = new NewEditStaticTextView
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

        public List<DropdownEntityView> GetQuestionsEligibleForNumericRosterTitle(string questionnaireId, Guid rosterId, Guid rosterSizeQuestionId)
        {
            var document = this.questionnaireDocumentReader.GetById(questionnaireId);
            if (document == null)
                return null;

            ReadOnlyQuestionnaireDocument questionnaire = new ReadOnlyQuestionnaireDocument(document);

            var roster = questionnaire.GetRoster(rosterId);
            RosterScope rosterScope = questionnaire.GetRosterScope(roster);

            IEnumerable<IQuestion> filteredQuestions = document.Find<IQuestion>().Where(x => x.QuestionType != QuestionType.Multimedia);

            var areTitlesForUnsavedRosterSizeRequested = roster.RosterSizeQuestionId != rosterSizeQuestionId;
            
            if (areTitlesForUnsavedRosterSizeRequested)
            {
                RosterScope prospectiveRosterScopeIds = rosterScope.Shrink(rosterScope.Length - 1).Extend(rosterSizeQuestionId);
                filteredQuestions = filteredQuestions
                    // childs of roster being changed + all questions from future roster scope
                    .Where(x => x.GetParent()?.PublicKey == rosterId || questionnaire.GetRosterScope(x).Equals(prospectiveRosterScopeIds))
                    .ToList();
            }
            else
            {
                filteredQuestions = filteredQuestions
                    // all question from current roster scope
                    .Where(x => questionnaire.GetRosterScope(x).Equals(rosterScope))
                    .ToList();
            }

            return this.PrepareGroupedQuestionsListForDropdown(questionnaire, filteredQuestions);
        }

        public VariableView GetVariableEditView(string questionnaireId, Guid variableId)
        {
            var document = this.questionnaireDocumentReader.GetById(questionnaireId);

            var variable = document?.Find<IVariable>(variableId);
            if (variable == null)
                return null;

            ReadOnlyQuestionnaireDocument questionnaire = new ReadOnlyQuestionnaireDocument(document);

            VariableView result = new VariableView
            {
                Id = variable.PublicKey,
                ItemId = variable.PublicKey.FormatGuid(),
                VariableData = new VariableData(variable.Type, variable.Name, variable.Expression, variable.Label),
                TypeOptions = VariableTypeOptions,
                Breadcrumbs = this.GetBreadcrumbs(questionnaire, variable),
            };

            return result;
        }

        public List<QuestionnaireItemLink> GetAllBrokenGroupDependencies(string questionnaireId, Guid id)
        {
            var questionnaireDocument = this.questionnaireDocumentReader.GetById(questionnaireId);
            if (questionnaireDocument == null)
                return null;

            ReadOnlyQuestionnaireDocument questionnaire = new ReadOnlyQuestionnaireDocument(questionnaireDocument);

            var variablesToBeDeleted = questionnaire.Find<IQuestion>()
                .Where(x => questionnaire.GetParentGroupsIds(x).Contains(id))
                .Select(x => x.StataExportCaption)
                .ToList();

            var allReferencedQuestionsByExpressions = questionnaire.Find<IQuestion>()
                .Where(x => !questionnaire.GetParentGroupsIds(x).Contains(id))
                .Where(x => !string.IsNullOrEmpty(x.ConditionExpression) || x.ValidationConditions.Any(q => !string.IsNullOrEmpty(q.Expression)))
                .Where(x => this.expressionProcessor.GetIdentifiersUsedInExpression(x.ConditionExpression).Any(v => variablesToBeDeleted.Contains(v))
                         || x.ValidationConditions.SelectMany(q => this.expressionProcessor.GetIdentifiersUsedInExpression(q.Expression)).Any(v => variablesToBeDeleted.Contains(v)));

            var singleQuestionIdsToBeDeleted = questionnaire.Find<IQuestion>()
                .Where(x => questionnaire.GetParentGroupsIds(x).Contains(id))
                .Where(x => x.QuestionType == QuestionType.SingleOption)
                .Select(x => x.PublicKey)
                .ToList();

            var allCascadingDependentOutsideQuestions = questionnaire.Find<IQuestion>()
                .Where(x => x.QuestionType == QuestionType.SingleOption)
                .Where(x => !questionnaire.GetParentGroupsIds(x).Contains(id))
                .Where(x => x.CascadeFromQuestionId != null && singleQuestionIdsToBeDeleted.Contains(x.CascadeFromQuestionId.Value));


            var allQuestions = allReferencedQuestionsByExpressions.Concat(allCascadingDependentOutsideQuestions);
            return allQuestions.Select(x => new QuestionnaireItemLink
            {
                Id = x.PublicKey.FormatGuid(),
                ChapterId = questionnaire.GetParentGroupsIds(x).Last().FormatGuid(),
                Title = x.QuestionText
            }).ToList();
        }

        private RosterType getRosterType(ReadOnlyQuestionnaireDocument questionnaire,
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

        private void ReplaceGuidsInValidationAndConditionRules(NewEditQuestionView model, ReadOnlyQuestionnaireDocument questionnaire, string questionnaireKey)
        {
            var expressionReplacer = new ExpressionReplacer(questionnaire.Questionnaire);
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
                DefaultDate = question.Properties?.DefaultDate,
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
                GeometryType = question.Properties?.GeometryType ?? GeometryType.Polygon
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
                    questionView.Options = CreateCategoricalOptions(numericQuestion.Answers);
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
                    questionView.DefaultDate = dateTimeQuestion.Properties.DefaultDate;
                    return questionView;
                case QuestionType.Multimedia:
                    var multimediaQuestion = (MultimediaQuestion) question;
                    questionView.IsSignature = multimediaQuestion.IsSignature;
                    return questionView;
                case QuestionType.Audio:
                case QuestionType.QRBarcode:
                case QuestionType.GpsCoordinates:
                case QuestionType.Area:
                    return questionView;
            }
            return null;
        }

        private static CategoricalOption[] CreateCategoricalOptions(List<Answer> answers)
        {
            if (answers == null)
                return new CategoricalOption[0];
            
            return answers?.Select(x =>
            {
                var option = new CategoricalOption();
                option.Title = x.AnswerText;
                if (decimal.TryParse(x.AnswerValue, out decimal answerValue))
                {
                    option.Value = answerValue;
                }
                option.ParentValue = string.IsNullOrWhiteSpace(x.ParentValue) || !x.ParentValue.IsDecimal() ? (decimal?)null : Convert.ToDecimal(x.ParentValue);
                return option;
            }).ToArray();
        }


        private List<DropdownEntityView> GetSourcesOfSingleQuestionBriefs(ReadOnlyQuestionnaireDocument document, Guid questionId)
        {
            IEnumerable<IQuestion> filteredQuestions = 
                document.Find<SingleQuestion>()
                .Where(q => q.PublicKey != questionId)
                .Where(q => !q.LinkedToRosterId.HasValue && !q.LinkedToQuestionId.HasValue)
                .ToList();

            var result = this.PrepareGroupedQuestionsListForDropdown(document, filteredQuestions);

            return result;
        }

        private List<DropdownEntityView> GetSourcesOfLinkedQuestionBriefs(ReadOnlyQuestionnaireDocument document, Guid questionId)
        {
            var result = new List<DropdownEntityView>();

            var targetRosterScope = document.GetRosterScope(questionId);
            var entities = document.Find<IComposite>(x => x.PublicKey != questionId);

            foreach (var entity in entities)
            {
                if (document.IsRoster(entity))
                {
                    var roster = entity as Group;
                    var rosterTitlePlaceholder = this.CreateRosterDropdownView(roster, document);
                    result.Add(rosterTitlePlaceholder);
                }

                var question = entity as IQuestion;
                if (question == null) continue;

                var rosterScope = document.GetRosterScope(question.PublicKey);
                if (QuestionsWhichCanBeUsedAsSourceOfLinkedQuestion.Contains(question.QuestionType))
                {
                    if (rosterScope.Length == 0)
                        continue;

                    result.Add(this.CreateQuestionDropdownView(document, question));
                }

                if (QuestionType.TextList == question.QuestionType && targetRosterScope.IsSameOrChildScopeFor(rosterScope))
                {
                    result.Add(this.CreateQuestionDropdownView(document, question));
                }
            }

            return CreateGroupedList(result);
        }

        private DropdownEntityView CreateRosterDropdownView(IGroup roster, ReadOnlyQuestionnaireDocument document)
        {
            var rosterTitlePlaceholder = new DropdownEntityView
            {
                Title = string.Format(Roster.RosterTitle, roster.Title),
                Id = roster.PublicKey.FormatGuid(),
                IsSectionPlaceHolder = false,
                Breadcrumbs = this.GetBreadcrumbsAsString(document, roster) + BreadcrumbSeparator + roster.Title,
                Type = this.rosterType,
                VarName = roster.VariableName,
                QuestionType = document.GetRosterSourceType(roster, document)
            };
            return rosterTitlePlaceholder;
        }

        private DropdownEntityView CreateQuestionDropdownView(ReadOnlyQuestionnaireDocument document, IQuestion question)
        {
            var rosterPlaceholder = new DropdownEntityView
            {
                IsSectionPlaceHolder = false,
                Id = question.PublicKey.FormatGuid(),
                Title = question.QuestionText,
                Breadcrumbs = GetBreadcrumbsAsString(document, question),
                Type = question.QuestionType.ToString().ToLower(),
                VarName = question.StataExportCaption
            };
            return rosterPlaceholder;
        }

        private List<DropdownEntityView> GetNumericIntegerTitles(ReadOnlyQuestionnaireDocument document, IGroup roster)
        {
            var rosterSizeQuestion = roster.RosterSizeQuestionId;

            IEnumerable<IQuestion> filteredQuestions;

            if (rosterSizeQuestion.HasValue && document.IsQuestionIsNumeric(rosterSizeQuestion.Value))
            {
                var rosterScope = document.GetRosterScope(roster);
                filteredQuestions = document.Find<IQuestion>()
                    .Where(x => x.QuestionType != QuestionType.Multimedia && x.QuestionType != QuestionType.GpsCoordinates)
                    .Where(x => document.GetRosterScope(x).Equals(rosterScope))
                    .ToList();
            }
            else
            {
                filteredQuestions = document.Find<IQuestion>()
                    .Where(x => x.QuestionType != QuestionType.Multimedia && x.QuestionType != QuestionType.GpsCoordinates)
                    .Where(x => x.GetParent()?.PublicKey == roster.PublicKey)
                    .ToList();
            }

            return this.PrepareGroupedQuestionsListForDropdown(document, filteredQuestions);
        }

        private List<DropdownEntityView> GetNumericIntegerQuestionBriefs(ReadOnlyQuestionnaireDocument document, RosterScope rosterScopeIds)
        {
            IEnumerable<IQuestion> filteredQuestions =
                    document.Find<INumericQuestion>()
                    .Where(x => x.IsInteger)
                    .Where(x => document.GetRosterScope(x).IsParentScopeFor(rosterScopeIds))
                    .ToList();

            return this.PrepareGroupedQuestionsListForDropdown(document, filteredQuestions);
        }

        private List<DropdownEntityView> GetNotLinkedMultiOptionQuestionBriefs(ReadOnlyQuestionnaireDocument document, RosterScope rosterScopeIds)
        {
            IEnumerable<IQuestion> filteredQuestions = document.Find<IMultyOptionsQuestion>()
                    .Where(x => !x.LinkedToQuestionId.HasValue && !x.LinkedToRosterId.HasValue)
                    .Where(x => document.GetRosterScope(x).IsParentScopeFor(rosterScopeIds))
                    .ToList();

            return this.PrepareGroupedQuestionsListForDropdown(document, filteredQuestions);
        }

        private List<DropdownEntityView> GetTextListsQuestionBriefs(ReadOnlyQuestionnaireDocument document, RosterScope rosterScopeIds)
        {
            IEnumerable<IQuestion> filteredQuestions =
                    document.Find<ITextListQuestion>()
                    .Where(x => document.GetRosterScope(x).IsParentScopeFor(rosterScopeIds))
                    .ToList();

            return this.PrepareGroupedQuestionsListForDropdown(document, filteredQuestions);
        }

        private List<DropdownEntityView> PrepareGroupedQuestionsListForDropdown(ReadOnlyQuestionnaireDocument document, IEnumerable<IQuestion> filteredQuestions)
        {
            var questions = filteredQuestions
                .Select(q => new DropdownEntityView
                {
                    Id = q.PublicKey.FormatGuid(),
                    Title = q.QuestionText,
                    Breadcrumbs = this.GetBreadcrumbsAsString(document, q),
                    Type = q.QuestionType.ToString().ToLower(),
                    VarName = q.StataExportCaption
                }).ToArray();

            return CreateGroupedList(questions);
        }

        private static List<DropdownEntityView> CreateGroupedList(IEnumerable<DropdownEntityView> dropdownItems)
        {
            var groupedList = dropdownItems.GroupBy(x => x.Breadcrumbs);
            var result = new List<DropdownEntityView>();

            foreach (var brief in groupedList)
            {
                var sectionPlaceholder = new DropdownEntityView
                {
                    Title = brief.Key,
                    IsSectionPlaceHolder = true
                };

                result.Add(sectionPlaceholder);
                result.AddRange(brief.Select(question => new DropdownEntityView
                {
                    Title = question.Title,
                    Id = question.Id,
                    IsSectionPlaceHolder = false,
                    Breadcrumbs = brief.Key,
                    Type = question.Type,
                    VarName = question.VarName,
                    QuestionType = question.QuestionType
                }));
            }
            return result;
        }

        private Breadcrumb[] GetBreadcrumbs(ReadOnlyQuestionnaireDocument document, IComposite entity)
        {
            List<IGroup> parents = new List<IGroup>();
            var parent = (IGroup)entity.GetParent();
            while (parent != null && parent != document.Questionnaire)
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

        private string GetBreadcrumbsAsString(ReadOnlyQuestionnaireDocument document, IComposite question)
        {
            return string.Join(BreadcrumbSeparator, GetBreadcrumbs(document, question).Select(x => x.Title));
        }
    }
}
