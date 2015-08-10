﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;
using WB.Core.SharedKernels.Enumerator.Entities;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire;
using WB.Core.SharedKernels.Enumerator.Models.Questionnaire.Questions;
using WB.Core.SharedKernels.Enumerator.Properties;
using WB.Core.SharedKernels.Enumerator.Services;

namespace WB.Core.SharedKernels.Enumerator.Implementation.Services
{
    internal class QuestionnaireImportService : IQuestionnaireImportService
    {
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireModelRepository;

        private readonly IPlainQuestionnaireRepository questionnaireRepository;

        private readonly IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor;

        public QuestionnaireImportService(
            IPlainKeyValueStorage<QuestionnaireModel> questionnaireModelRepository, 
            IPlainQuestionnaireRepository questionnaireRepository, 
            IQuestionnaireAssemblyFileAccessor questionnaireAssemblyFileAccessor)
        {
            this.questionnaireModelRepository = questionnaireModelRepository;
            this.questionnaireRepository = questionnaireRepository;
            this.questionnaireAssemblyFileAccessor = questionnaireAssemblyFileAccessor;
        }

        public void ImportQuestionnaire(QuestionnaireDocument questionnaireDocument, string supportingAssembly, long version)
        {
            this.ImportQuestionnaireModel(questionnaireDocument, version);
            this.ImportQuestionnaireDocument(questionnaireDocument.PublicKey, 1, questionnaireDocument);
            this.ImportAssembly(questionnaireDocument.PublicKey, 1, supportingAssembly);
        }

        public void ImportQuestionnaireModel(QuestionnaireDocument questionnaireDocument, long version)
        {
            var questionnaireIdentity = new QuestionnaireIdentity(questionnaireDocument.PublicKey, version);

            questionnaireDocument.ConnectChildrenWithParent();

            var questionnaireModel = new QuestionnaireModel
            {
                Parents = new Dictionary<Guid, List<GroupReferenceModel>>(),
                GroupsParentIdMap = new Dictionary<Guid, Guid?>(),
                GroupsRosterLevelDepth = new Dictionary<Guid, int>(),
                QuestionsNearestRosterIdMap = new Dictionary<Guid, Guid?>()
            };

            questionnaireDocument.Children.Add(new Group(UIResources.Interview_Complete_Screen_Title)
            {
                PublicKey = questionnaireModel.FinishGroupId
            });

            var groups = questionnaireDocument.GetAllGroups().ToList();
            var questions = questionnaireDocument.GetAllQuestions().ToList();
            var staticTexts = questionnaireDocument.GetEntitiesByType<IStaticText>().ToList();
            var entities = questionnaireDocument.GetEntitiesByType<IComposite>().ToList();

            questionnaireModel.Id = questionnaireDocument.PublicKey;
            questionnaireModel.Title = questionnaireDocument.Title;
            questionnaireModel.StaticTexts = staticTexts.ToDictionary(x => x.PublicKey, CreateStaticTextModel);

            var questionIdToRosterLevelDepth = new Dictionary<Guid, int>();
            questionnaireDocument.Children.TreeToEnumerable(x => x.Children)
                .ForEach(x => this.PerformCalculationsBasedOnTreeStructure(questionnaireModel, x, questionIdToRosterLevelDepth));

            questionnaireModel.GroupsHierarchy = questionnaireDocument.Children.Cast<Group>().Select(x => this.BuildGroupsHierarchy(x, 0)).ToList();

            questionnaireModel.Questions = questions.ToDictionary(x => x.PublicKey, x => CreateQuestionModel(x, questionnaireDocument, questionIdToRosterLevelDepth));
            questionnaireModel.EntityReferences = entities.Select(x=>new QuestionnaireReferenceModel { Id = x.PublicKey, ModelType = x.GetType() });
            questionnaireModel.PrefilledQuestionsIds = questions.Where(x => x.Featured)
                .Select(x => questionnaireModel.Questions[x.PublicKey])
                .Select(x => new QuestionnaireReferenceModel { Id = x.Id, ModelType = x.GetType() })
                .ToList();

            questionnaireModel.GroupsWithFirstLevelChildrenAsReferences = groups.ToDictionary(x => x.PublicKey,
                x => CreateGroupModelWithoutNestedChildren(x, questionnaireModel.Questions));

            questionnaireModel.QuestionsByVariableNames = questions.ToDictionary(x => x.StataExportCaption, x => questionnaireModel.Questions[x.PublicKey]);

            this.questionnaireModelRepository.Store(questionnaireModel, questionnaireIdentity.ToString());
        }

        private void ImportQuestionnaireDocument(Guid questionnaireId, int questionnaireVersion, QuestionnaireDocument questionnaireDocument)
        {
            this.questionnaireRepository.StoreQuestionnaire(questionnaireId, questionnaireVersion, questionnaireDocument);
        }

        private void ImportAssembly(Guid questionnaireId, int questionnaireVersion, string supportingAssembly)
        {
            this.questionnaireAssemblyFileAccessor.StoreAssembly(questionnaireId, questionnaireVersion, supportingAssembly);
        }

        private GroupsHierarchyModel BuildGroupsHierarchy(Group currentGroup, int layerIndex)
        {
            var childrenHierarchy = currentGroup.Children.OfType<Group>()
                .Select(x => this.BuildGroupsHierarchy(x, layerIndex + 1))
                .ToList();

            var resultModel = new GroupsHierarchyModel
            {
                Id = currentGroup.PublicKey,
                Title = currentGroup.Title,
                IsRoster = currentGroup.IsRoster,
                ZeroBasedDepth = layerIndex,
                Children = childrenHierarchy
            };
            return resultModel;
        }

        private void PerformCalculationsBasedOnTreeStructure(QuestionnaireModel questionnaireModel, IComposite item, Dictionary<Guid, int> questionIdToRosterLevelDeep)
        {
            var parents = new List<GroupReferenceModel>();

            var parentAsGroup = item.GetParent() as Group;

            var closestParentGroupId = parentAsGroup == null? (Guid?)null : parentAsGroup.PublicKey;

            var countOfRostersToTop = 0;

            while (parentAsGroup != null)
            {
                countOfRostersToTop += parentAsGroup.IsRoster ? 1 : 0;

                var parentPlaceholder = new GroupReferenceModel
                {
                    IsRoster = parentAsGroup.IsRoster,
                    Id = parentAsGroup.PublicKey
                };
                parents.Add(parentPlaceholder);
                parentAsGroup = parentAsGroup.GetParent() as Group;
            }

            var @group = item as Group;
            if (@group != null)
            {
                parents.Reverse();
                questionnaireModel.Parents.Add(item.PublicKey, parents);
                questionnaireModel.GroupsRosterLevelDepth.Add(item.PublicKey, countOfRostersToTop);
                questionnaireModel.GroupsParentIdMap.Add(item.PublicKey, closestParentGroupId);
            }

            if (item is IQuestion)
            {
                var closestRosterReference = parents.FirstOrDefault(x => x.IsRoster);
                questionnaireModel.QuestionsNearestRosterIdMap.Add(
                    item.PublicKey,
                    closestRosterReference == null ? (Guid?)null : closestRosterReference.Id);
                questionIdToRosterLevelDeep.Add(item.PublicKey, countOfRostersToTop);
            }
        }

        private static GroupModel CreateGroupModelWithoutNestedChildren(Group @group, Dictionary<Guid, BaseQuestionModel> questions)
        {
            var groupModel = group.IsRoster ? new RosterModel() : new GroupModel();

            groupModel.Id = group.PublicKey;
            groupModel.Title = group.Title;

            foreach (var child in group.Children)
            {
                var question = child as AbstractQuestion;
                if (question != null)
                {
                    if (question.QuestionScope != QuestionScope.Interviewer || question.Featured)
                        continue;

                    var questionModelPlaceholder = new QuestionnaireReferenceModel { Id = question.PublicKey, ModelType = questions[question.PublicKey].GetType() };
                    groupModel.Children.Add(questionModelPlaceholder);
                    continue;
                }

                var text = child as StaticText;
                if (text != null)
                {
                    var staticTextModel = new QuestionnaireReferenceModel { Id = text.PublicKey, ModelType = typeof(StaticTextModel) };
                    groupModel.Children.Add(staticTextModel);
                    continue;
                }

                var subGroup = child as Group;
                if (subGroup != null)
                {
                    var subGroupPlaceholder = new QuestionnaireReferenceModel
                    {
                        Id = subGroup.PublicKey,
                        ModelType = subGroup.IsRoster ? typeof(RosterModel) : typeof(GroupModel)
                    };
                    groupModel.Children.Add(subGroupPlaceholder);
                }
            }
            return groupModel;
        }

        private static StaticTextModel CreateStaticTextModel(IStaticText staticText)
        {
            var staticTextModel = new StaticTextModel();
            staticTextModel.Title = staticText.Text;
            staticTextModel.Id = staticText.PublicKey;
            return staticTextModel;
        }


        private static BaseQuestionModel CreateQuestionModel(IQuestion question, QuestionnaireDocument questionnaireDocument, Dictionary<Guid, int> questionIdToRosterLevelDeep)
        {
            BaseQuestionModel questionModel;
            
            var isRosterSizeQuestion = questionnaireDocument.Find<Group>(g => g.IsRoster && g.RosterSizeQuestionId == question.PublicKey).Any();

            switch (question.QuestionType)
            {
                case QuestionType.SingleOption:
                    var singleQuestion = question as SingleQuestion;
                    if (singleQuestion.LinkedToQuestionId.HasValue)
                    {
                        questionModel = new LinkedSingleOptionQuestionModel
                        {
                            LinkedToQuestionId = singleQuestion.LinkedToQuestionId.Value
                        };
                    }
                    else
                    {
                        var isFilteredCombobox = singleQuestion.IsFilteredCombobox.GetValueOrDefault();
                        if (isFilteredCombobox)
                        {
                            questionModel = new FilteredSingleOptionQuestionModel
                            {
                                Options = singleQuestion.Answers.Select(ToOptionModel).ToList(),
                            };
                        }
                        else 
                        {
                            if (singleQuestion.CascadeFromQuestionId.HasValue)
                            {
                                questionModel = new CascadingSingleOptionQuestionModel
                                {
                                    CascadeFromQuestionId = singleQuestion.CascadeFromQuestionId.Value,
                                    Options = singleQuestion.Answers.Select(ToCascadingOptionModel).ToList(),
                                    RosterLevelDepthOfParentQuestion = questionIdToRosterLevelDeep[singleQuestion.CascadeFromQuestionId.Value],
                                };
                            }
                            else
                            {
                                questionModel = new SingleOptionQuestionModel
                                {
                                    Options = singleQuestion.Answers.Select(ToOptionModel).ToList(),
                                };
                            }
                        }
                    }
                    break;
                case QuestionType.MultyOption:
                    var multiQuestion = question as MultyOptionsQuestion;
                    if (multiQuestion.LinkedToQuestionId.HasValue)
                    {
                        questionModel = new LinkedMultiOptionQuestionModel
                        {
                            LinkedToQuestionId = multiQuestion.LinkedToQuestionId.Value,
                            MaxAllowedAnswers = multiQuestion.MaxAllowedAnswers,
                            AreAnswersOrdered = multiQuestion.AreAnswersOrdered
                        };
                    }
                    else
                    {
                        questionModel = new MultiOptionQuestionModel
                        {
                            AreAnswersOrdered = multiQuestion.AreAnswersOrdered,
                            MaxAllowedAnswers = multiQuestion.MaxAllowedAnswers,
                            Options = question.Answers.Select(ToOptionModel).ToList(),
                            IsRosterSizeQuestion = isRosterSizeQuestion
                        };
                    }
                    break;
                case QuestionType.Numeric:
                    var numericQuestion = question as INumericQuestion;
                    if (numericQuestion.IsInteger)
                    {
                        questionModel = new IntegerNumericQuestionModel
                                        {
                                            IsRosterSizeQuestion = isRosterSizeQuestion
                                        };
                    }
                    else
                    {
                        questionModel = new RealNumericQuestionModel { CountOfDecimalPlaces = numericQuestion.CountOfDecimalPlaces };
                    }
                    break;
                case QuestionType.DateTime:
                    questionModel = new DateTimeQuestionModel();
                    break;
                case QuestionType.GpsCoordinates:
                    questionModel = new GpsCoordinatesQuestionModel();
                    break;
                case QuestionType.Text:
                    questionModel = new TextQuestionModel { Mask = (question as TextQuestion).Mask };
                    break;
                case QuestionType.TextList:
                    var listQuestion = question as TextListQuestion;
                    questionModel = new TextListQuestionModel
                                    {
                                        IsRosterSizeQuestion = isRosterSizeQuestion,
                                        MaxAnswerCount = listQuestion.MaxAnswerCount
                                    };
                    break;
                case QuestionType.QRBarcode:
                    questionModel = new QRBarcodeQuestionModel();
                    break;
                case QuestionType.Multimedia:
                    questionModel = new MultimediaQuestionModel();
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            questionModel.Id = question.PublicKey;
            questionModel.Title = question.QuestionText;
            questionModel.IsMandatory = question.Mandatory;
            questionModel.IsPrefilled = question.Featured;
            questionModel.ValidationMessage = question.ValidationMessage;
            questionModel.Instructions = question.Instructions;
            questionModel.Variable = question.StataExportCaption;

            return questionModel;
        }

        private static OptionModel ToOptionModel(Answer answer)
        {
            return new OptionModel
            {
                Value = decimal.Parse(answer.AnswerValue, CultureInfo.InvariantCulture),
                Title = answer.AnswerText,
            };
        }

        private static CascadingOptionModel ToCascadingOptionModel(Answer answer)
        {
            return new CascadingOptionModel
            {
                Value = decimal.Parse(answer.AnswerValue, CultureInfo.InvariantCulture),
                Title = answer.AnswerText,
                ParentValue = decimal.Parse(answer.ParentValue, CultureInfo.InvariantCulture),
            };
        }
    }
}
