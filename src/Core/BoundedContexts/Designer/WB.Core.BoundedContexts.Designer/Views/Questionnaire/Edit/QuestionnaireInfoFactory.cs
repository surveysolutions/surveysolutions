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
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.SharedKernels.Questionnaire.Documents;
using WB.Core.SharedKernels.QuestionnaireEntities;


namespace WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit
{
    public class QuestionnaireInfoFactory : IQuestionnaireInfoFactory
    {
        public class SelectOption
        {
            public string? Value { get; set; }
            public string? Text { get; set; }
        }

        private readonly IDesignerQuestionnaireStorage questionnaireDocumentReader;
       
        private readonly IExpressionProcessor expressionProcessor;

        private SelectOption[] GetQuestionScopeOptions(QuestionnaireDocument document, IQuestion question)
        {
            if (document.IsCoverPageSupported)
            {
                var parent = question.GetParent();
                if (parent != null && document.IsCoverPage(parent.PublicKey))
                    return Array.Empty<SelectOption>();
                
                return new[]
                {
                    new SelectOption {Value = "Interviewer", Text = QuestionnaireEditor.QuestionScopeInterviewer},
                    new SelectOption {Value = "Supervisor", Text = QuestionnaireEditor.QuestionScopeSupervisor},
                    new SelectOption {Value = "Hidden", Text = QuestionnaireEditor.QuestionScopeHidden},
                };
            }

            return new[]
            {
                new SelectOption {Value = "Interviewer", Text = QuestionnaireEditor.QuestionScopeInterviewer},
                new SelectOption {Value = "Supervisor", Text = QuestionnaireEditor.QuestionScopeSupervisor},
                new SelectOption {Value = "Hidden", Text = QuestionnaireEditor.QuestionScopeHidden},
                new SelectOption {Value = "Identifying", Text = QuestionnaireEditor.QuestionScopeIdentifying}
            };
        } 

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
                Text = QuestionnaireEditor.GeometryTypePolygon
            },
            new SelectOption
            {
                Value = GeometryType.Polyline.ToString(),
                Text = QuestionnaireEditor.GeometryTypePolyline
            },

            new SelectOption
            {
                Value = GeometryType.Point.ToString(),
                Text = QuestionnaireEditor.GeometryTypePoint
            },

            new SelectOption
            {
                Value = GeometryType.Multipoint.ToString(),
                Text = QuestionnaireEditor.GeometryTypeMultipoint
            }
        };

        private SelectOption[] GetQuestionTypeOptions(QuestionnaireDocument document, IQuestion question)
        {
            var isQuestionOnCover = document.IsCoverPage(question.GetParent()!.PublicKey);
            
            List<SelectOption> list = new List<SelectOption>();
            list.Add(new SelectOption
            {
                Value = "SingleOption",
                Text = QuestionnaireEditor.QuestionTypeSingleSelect
            });
            if (!isQuestionOnCover)
                list.Add(new SelectOption
                {
                    Value = "MultyOption",
                    Text = QuestionnaireEditor.QuestionTypeMultiSelect
                });
            list.Add(new SelectOption
            {
                Value = "Numeric",
                Text = QuestionnaireEditor.QuestionTypeNumeric
            });
            list.Add(new SelectOption
            {
                Value = "DateTime",
                Text = QuestionnaireEditor.QuestionTypeDate
            });
            list.Add(new SelectOption
            {
                Value = "Text",
                Text = QuestionnaireEditor.QuestionTypeText
            });
            list.Add(new SelectOption
            {
                Value = "GpsCoordinates",
                Text = QuestionnaireEditor.QuestionTypeGPS
            });
            if (!isQuestionOnCover)
            {
                list.Add(new SelectOption
                {
                    Value = "TextList",
                    Text = QuestionnaireEditor.QuestionTypeList
                });
                list.Add(new SelectOption
                {
                    Value = "QRBarcode",
                    Text = QuestionnaireEditor.QuestionTypeBarcode
                });
                list.Add(new SelectOption
                {
                    Value = "Multimedia",
                    Text = QuestionnaireEditor.QuestionTypePicture
                });
                list.Add(new SelectOption
                {
                    Value = "Audio",
                    Text = QuestionnaireEditor.QuestionTypeAudio
                });
                list.Add(new SelectOption
                {
                    Value = "Area",
                    Text = QuestionnaireEditor.QuestionTypeGeography
                });
            }

            return list.ToArray();
        }

        private readonly string rosterTypeName = "roster";

        private SelectOption[] RosterTypeOptions => new[]
        {
            new SelectOption {Value = RosterType.Fixed.ToString(), Text = Roster.RosterType_Fixed},
            new SelectOption {Value = RosterType.List.ToString(), Text = Roster.RosterType_List},
            new SelectOption {Value = RosterType.Multi.ToString(), Text = Roster.RosterType_Multi},
            new SelectOption {Value = RosterType.Numeric.ToString(), Text = Roster.RosterType_Numeric}
        };

        private static readonly string BreadcrumbSeparator = " / ";

        public QuestionnaireInfoFactory(
            IDesignerQuestionnaireStorage questionnaireDocumentReader,
            IExpressionProcessor expressionProcessor)
        {
            this.questionnaireDocumentReader = questionnaireDocumentReader;
            this.expressionProcessor = expressionProcessor;
        }

        public CategoriesView? GetCategoriesView(QuestionnaireRevision questionnaireId, Guid entityid)
        {
            var document = this.questionnaireDocumentReader.Get(questionnaireId);
            if (document == null)
                throw new InvalidOperationException($"Questionnaire was not found ({questionnaireId}).");

            var categories = document.Categories.FirstOrDefault(x => x.Id == entityid);

            return categories == null
                ? null
                : new CategoriesView(categoriesId: categories.Id.FormatGuid(), categories.Name);
        }


        public Guid GetSectionIdForItem(QuestionnaireRevision questionnaireId, Guid? entityid)
        {
            var document = this.questionnaireDocumentReader.Get(questionnaireId);
            if(document == null)
                throw new InvalidOperationException($"Questionnaire was not found ({questionnaireId}).");
            document.ConnectChildrenWithParent();
            var firstSectionId = document.Children.First().PublicKey;
            if (entityid == null)
                return firstSectionId;

            var entity = document.Find<IComposite>(entityid.Value);
            if (entity == null)
                return firstSectionId;

            List<IGroup> parents = new List<IGroup>();
            var parent = entity.GetParent() as IGroup;
            while (parent != null && !(parent is QuestionnaireDocument))
            {
                parents.Add(parent);
                parent = parent.GetParent() as IGroup;
            }
            var sectionId = parents.Select(x => x.PublicKey).LastOrDefault();

            return sectionId == Guid.Empty ? entity.PublicKey : sectionId;
        }

        public NewEditGroupView? GetGroupEditView(QuestionnaireRevision questionnaireId, Guid groupId)
        {
            var document = this.questionnaireDocumentReader.Get(questionnaireId);
            if (document == null)
                return null;

            var group = document.Find<IGroup>(groupId);
            if (@group == null)
            {
                if (document.IsCoverPage(groupId) && !document.IsCoverPageSupported)
                {
                    return new NewEditGroupView
                    (
                        group: new GroupDetailsView
                        { 
                            Id = document.CoverPageSectionId,
                            Title = QuestionnaireEditor.CoverPageSection,
                            EnablementCondition = null,
                            HideIfDisabled = false,
                            VariableName = null,
                            DisplayMode = RosterDisplayMode.SubSection
                        },
                        breadcrumbs: new[] { new Breadcrumb()
                        {
                            Id = document.CoverPageSectionId.FormatGuid(),
                            Title = QuestionnaireEditor.CoverPageSection
                        }}
                    );
                }
                return null;
            }

            ReadOnlyQuestionnaireDocument questionnaire = new ReadOnlyQuestionnaireDocument(document);

            var result = new NewEditGroupView
            (
                group : new GroupDetailsView
                {
                    Id = group.PublicKey,
                    Title = group.Title,
                    EnablementCondition = group.ConditionExpression,
                    HideIfDisabled = group.HideIfDisabled,
                    VariableName = group.VariableName,
                    DisplayMode = group.DisplayMode
                },
                breadcrumbs : this.GetBreadcrumbs(questionnaire, group)
            );
            return result;
        }


        public NewEditRosterView? GetRosterEditView(QuestionnaireRevision questionnaireId, Guid rosterId)
        {
            var document = this.questionnaireDocumentReader.Get(questionnaireId);
            if (document == null)
                return null;
            ReadOnlyQuestionnaireDocument questionnaire = new ReadOnlyQuestionnaireDocument(document);

            var roster = document?.Find<IGroup>(rosterId);
            if (roster == null)
                return null;
            
            var rosterType = this.getRosterType(questionnaire: questionnaire,
                rosterSizeSourceType: roster.RosterSizeSource, 
                rosterSizeQuestionId: roster.RosterSizeQuestionId);

            var rosterScope = questionnaire.GetRosterScope(roster);

            var rosterTitle = !roster.CustomRosterTitle
                ? roster.Title + " - %rostertitle%"
                : roster.Title;

            var result = new NewEditRosterView(
                displayMode: roster.DisplayMode,
                displayModes : Enum.GetValues(typeof(RosterDisplayMode)).Cast<RosterDisplayMode>().ToArray())
            {
                ItemId = roster.PublicKey.FormatGuid(),
                Title = rosterTitle,
                EnablementCondition = roster.ConditionExpression,
                HideIfDisabled = roster.HideIfDisabled,
                VariableName = roster.VariableName,
                DisplayMode = roster.DisplayMode,

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

        public NewEditQuestionView? GetQuestionEditView(QuestionnaireRevision questionnaireId, Guid questionId)
        {
            var document = this.questionnaireDocumentReader.Get(questionnaireId);

            var question = document?.Find<IQuestion>(questionId);
            if (question == null)
                return null;

            if (document == null)
                return null;
            ReadOnlyQuestionnaireDocument questionnaire = new ReadOnlyQuestionnaireDocument(document);

            NewEditQuestionView? result = MapQuestionFields(question);
            if (result != null)
            {
                result.Options ??= new CategoricalOption[0];
                result.OptionsCount = result.Options.Length;
                result.Breadcrumbs = this.GetBreadcrumbs(questionnaire, question);
                result.SourceOfLinkedEntities = this.GetSourcesOfLinkedQuestionBriefs(questionnaire, questionId);
                result.SourceOfSingleQuestions = this.GetSourcesOfSingleQuestionBriefs(questionnaire, questionId);
                result.QuestionTypeOptions = GetQuestionTypeOptions(document, question);
                result.AllQuestionScopeOptions = GetQuestionScopeOptions(document, question);
                result.GeometryTypeOptions = GeometryTypeOptions;
                result.ChapterId = questionnaire.GetParentGroupsIds(question).LastOrDefault();

                this.ReplaceGuidsInValidationAndConditionRules(result, questionnaire, questionnaireId);
            }

            return result;
        }

        public NewEditStaticTextView? GetStaticTextEditView(QuestionnaireRevision questionnaireId, Guid staticTextId)
        {
            var document = this.questionnaireDocumentReader.Get(questionnaireId);
            if (document == null)
                return null;
            ReadOnlyQuestionnaireDocument questionnaire = new ReadOnlyQuestionnaireDocument(document);

            var staticText = document.Find<IStaticText>(staticTextId);
            if (staticText == null)
                return null;
            
            var result = new NewEditStaticTextView
            (
                id : staticText.PublicKey,
                text : staticText.Text,
                attachmentName : staticText.AttachmentName,
                enablementCondition : staticText.ConditionExpression,
                hideIfDisabled : staticText.HideIfDisabled,
                breadcrumbs: this.GetBreadcrumbs(questionnaire, staticText),
                validationCondition: staticText.ValidationConditions.ToList()
            );

            return result;
        }

        public List<DropdownEntityView>? GetQuestionsEligibleForNumericRosterTitle(QuestionnaireRevision questionnaireId, Guid rosterId, Guid rosterSizeQuestionId)
        {
            var document = this.questionnaireDocumentReader.Get(questionnaireId);
            if (document == null)
                return null;

            ReadOnlyQuestionnaireDocument questionnaire = new ReadOnlyQuestionnaireDocument(document);

            var roster = questionnaire.GetRoster(rosterId);
            if (roster == null)
                return null;
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

        public VariableView? GetVariableEditView(QuestionnaireRevision questionnaireId, Guid variableId)
        {
            var document = this.questionnaireDocumentReader.Get(questionnaireId);
            if (document == null)
                return null;
            ReadOnlyQuestionnaireDocument questionnaire = new ReadOnlyQuestionnaireDocument(document);
            
            var variable = document.Find<IVariable>(variableId);
            if (variable == null)
                return null;
            
            VariableView result = new VariableView(
                variable.PublicKey,
                variable.PublicKey.FormatGuid(),
                new VariableData(variable.Type, variable.Name, variable.Expression, variable.Label,variable.DoNotExport),
                this.GetBreadcrumbs(questionnaire, variable),
                VariableTypeOptions);

            return result;
        }

        public List<QuestionnaireItemLink> GetAllBrokenGroupDependencies(QuestionnaireRevision questionnaireId, Guid id)
        {
            var questionnaireDocument = this.questionnaireDocumentReader.Get(questionnaireId);
            if (questionnaireDocument == null)
                return new List<QuestionnaireItemLink>();

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

        private void ReplaceGuidsInValidationAndConditionRules(NewEditQuestionView model,
            ReadOnlyQuestionnaireDocument questionnaire, QuestionnaireRevision questionnaireKey)
        {
            var expressionReplacer = new ExpressionReplacer(questionnaire.Questionnaire);

            model.EnablementCondition = expressionReplacer.ReplaceGuidsWithStataCaptions(
                model.EnablementCondition, questionnaireKey.QuestionnaireId);

            foreach (var validationExpression in model.ValidationConditions)
            {
                validationExpression.Expression = expressionReplacer.ReplaceGuidsWithStataCaptions(
                    validationExpression.Expression, questionnaireKey.QuestionnaireId) ?? String.Empty;
            }
        }

        private static NewEditQuestionView? MapQuestionFields(IQuestion question)
        {
            var questionView = new NewEditQuestionView
            (
                title : question.QuestionText,
                type : question.QuestionType,
                variableLabel : question.VariableLabel,
                id : question.PublicKey,
                parentGroupId : (question.GetParent() ?? throw new InvalidOperationException("Parent was not found.")).PublicKey,
                instructions : question.Instructions,
                enablementCondition : question.ConditionExpression,
                isPreFilled : question.Featured,
                isTimestamp : question.IsTimestamp,
                defaultDate : question.Properties?.DefaultDate,
                isFilteredCombobox : question.IsFilteredCombobox,
                questionScope : question.QuestionScope,
                hideIfDisabled : question.HideIfDisabled,
                cascadeFromQuestionId : question.CascadeFromQuestionId?.FormatGuid(),
                linkedFilterExpression : question.LinkedFilterExpression,
                hideInstructions : question.Properties?.HideInstructions ?? false,
                useFormatting : question.Properties?.UseFormatting ?? false,
                optionsFilterExpression : question.Properties?.OptionsFilterExpression,
                variableName : question.StataExportCaption,
                linkedToEntityId : (question.LinkedToQuestionId ?? question.LinkedToRosterId)?.FormatGuid(),
                questionTypeOptions : question.Answers
                    .Select(a => new SelectOption() {Text = a.AnswerText, Value = a.AnswerValue}).ToArray(),
                geometryType : question.Properties?.GeometryType ?? GeometryType.Polygon,
                geometryInputMode: question.Properties?.GeometryInputMode ?? GeometryInputMode.Manual,
                geometryOverlapDetection: question.Properties?.GeometryOverlapDetection
            );
            questionView.ValidationConditions.AddRange(question.ValidationConditions);

            questionView.RosterScopeIds = new Guid[0];
            questionView.ParentGroupsIds = new Guid[0];

            switch (question.QuestionType)
            {
                case QuestionType.MultyOption:
                    var multyOptionsQuestion = (IMultyOptionsQuestion)question;
                    if(multyOptionsQuestion == null)
                        throw new InvalidOperationException("Question has incorrect type.");
                    questionView.YesNoView = multyOptionsQuestion.YesNoView;
                    questionView.AreAnswersOrdered = multyOptionsQuestion.AreAnswersOrdered;
                    questionView.MaxAllowedAnswers = multyOptionsQuestion.MaxAllowedAnswers;
                    questionView.LinkedToEntityId = multyOptionsQuestion.LinkedToQuestionId?.FormatGuid() ?? multyOptionsQuestion.LinkedToRosterId?.FormatGuid();
                    questionView.LinkedFilterExpression = multyOptionsQuestion.LinkedFilterExpression;
                    questionView.Options = CreateCategoricalOptions(multyOptionsQuestion.Answers);
                    questionView.OptionsFilterExpression = multyOptionsQuestion.Properties?.OptionsFilterExpression;
                    questionView.CategoriesId = multyOptionsQuestion.CategoriesId.FormatGuid();
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
                    questionView.OptionsFilterExpression = singleoptionQuestion.Properties?.OptionsFilterExpression;
                    questionView.ShowAsList = singleoptionQuestion.ShowAsList;
                    questionView.ShowAsListThreshold = singleoptionQuestion.ShowAsListThreshold;
                    questionView.CategoriesId = singleoptionQuestion.CategoriesId.FormatGuid();
                    return questionView;
                case QuestionType.Text:
                    var textQuestion = (TextQuestion)question;
                    questionView.Mask = textQuestion.Mask;
                    return questionView;
                case QuestionType.DateTime:
                    var dateTimeQuestion = (DateTimeQuestion)question;
                    questionView.IsTimestamp = dateTimeQuestion.IsTimestamp;
                    questionView.DefaultDate = dateTimeQuestion.Properties?.DefaultDate;
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
            return answers?.Select(x =>
            {
                var option = new CategoricalOption();
                option.Title = x.AnswerText;
                if (decimal.TryParse(x.AnswerValue, out decimal answerValue))
                {
                    option.Value = answerValue;
                }
                option.ParentValue = string.IsNullOrWhiteSpace(x.ParentValue) || !x.ParentValue.IsDecimal() ? (decimal?)null : Convert.ToDecimal(x.ParentValue);
                option.AttachmentName = x.AttachmentName;
                
                return option;
            }).ToArray()
                   ?? new CategoricalOption[0];
        }


        private List<DropdownEntityView> GetSourcesOfSingleQuestionBriefs(ReadOnlyQuestionnaireDocument document, Guid questionId)
        {
            var questionRosters = document.GetRosterScope(questionId);

            IEnumerable<IQuestion> filteredQuestions =
                document.Find<SingleQuestion>()
                    .Where(q => q.PublicKey != questionId)
                    .Where(q => !q.LinkedToRosterId.HasValue && !q.LinkedToQuestionId.HasValue)
                    .Where(q =>
                    {
                        var parentRosters = document.GetRosterScope(q);

                        return parentRosters.Length <= questionRosters.Length &&
                               !parentRosters.Where((parentGuid, i) => questionRosters[i] != parentGuid).Any();
                    })
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
                    if(roster == null) continue;
                    
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
                Type = this.rosterTypeName,
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
                    .Where(x => x.QuestionType == QuestionType.SingleOption 
                                || x.QuestionType == QuestionType.Numeric
                                || x.QuestionType == QuestionType.Text
                                || x.QuestionType == QuestionType.DateTime
                                || x.QuestionType == QuestionType.QRBarcode)
                    .Where(x => document.GetRosterScope(x).Equals(rosterScope))
                    .ToList();
            }
            else
            {
                filteredQuestions = document.Find<IQuestion>()
                    .Where(x => x.QuestionType == QuestionType.SingleOption
                                || x.QuestionType == QuestionType.Numeric
                                || x.QuestionType == QuestionType.Text
                                || x.QuestionType == QuestionType.DateTime
                                || x.QuestionType == QuestionType.QRBarcode)
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
            var parent = entity.GetParent() as IGroup;
            while (parent != null && parent != document.Questionnaire)
            {
                parents.Add(parent);
                parent = parent.GetParent() as IGroup;
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
