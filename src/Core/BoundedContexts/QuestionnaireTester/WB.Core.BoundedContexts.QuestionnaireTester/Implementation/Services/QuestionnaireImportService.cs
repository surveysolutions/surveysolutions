using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities;
using WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Entities.QuestionModels;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Accessors;
using WB.Core.SharedKernels.DataCollection.Repositories;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Services
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

        public void ImportQuestionnaire(QuestionnaireDocument questionnaireDocument, string supportingAssembly)
        {
            questionnaireDocument.ConnectChildrenWithParent();
            
            var questionnaireModel = new QuestionnaireModel
            {
                Parents = new Dictionary<Guid, List<GroupReferenceModel>>(),
                GroupsParentIdMap = new Dictionary<Guid, Guid?>(),
                GroupsRosterLevelDepth = new Dictionary<Guid, int>(),
                QuestionsNearestRosterIdMap = new Dictionary<Guid, Guid?>()
            };

            var groups = questionnaireDocument.GetAllGroups().ToList();
            var questions = questionnaireDocument.GetAllQuestions().ToList();
            var staticTexts = questionnaireDocument.GetEntitiesByType<IStaticText>().ToList();

            questionnaireModel.Id = questionnaireDocument.PublicKey;
            questionnaireModel.Title = questionnaireDocument.Title;
            questionnaireModel.StaticTexts = staticTexts.ToDictionary(x => x.PublicKey, CreateStaticTextModel);

            var questionIdToRosterLevelDepth = new Dictionary<Guid, int>();
            questionnaireDocument.Children.TreeToEnumerable(x => x.Children).ForEach(x => this.PerformCalculationsBasedOnTreeStructure(questionnaireModel, x, questionIdToRosterLevelDepth));

            questionnaireModel.GroupsHierarchy = questionnaireDocument.Children.Cast<Group>().Select(this.BuildGroupsHierarchy).ToList();
            
            questionnaireModel.Questions = questions.ToDictionary(x => x.PublicKey, x => CreateQuestionModel(x, questionnaireDocument, questionIdToRosterLevelDepth));
            questionnaireModel.PrefilledQuestionsIds = questions.Where(x => x.Featured)
                .Select(x => questionnaireModel.Questions[x.PublicKey])
                .Select(x => new QuestionnaireReferenceModel { Id = x.Id, ModelType = x.GetType() })
                .ToList();

            questionnaireModel.GroupsWithFirstLevelChildrenAsReferences = groups.ToDictionary(x => x.PublicKey, x => CreateGroupModelWithoutNestedChildren(x, questionnaireModel.Questions));

            questionnaireModel.QuestionsByVariableNames = questions.ToDictionary(x => x.StataExportCaption, x => questionnaireModel.Questions[x.PublicKey]);

            questionnaireModelRepository.Store(questionnaireModel, questionnaireDocument.PublicKey.FormatGuid());

            questionnaireAssemblyFileAccessor.StoreAssembly(questionnaireDocument.PublicKey, 1, supportingAssembly);
            questionnaireRepository.StoreQuestionnaire(questionnaireDocument.PublicKey, 1, questionnaireDocument);
        }

        private GroupsHierarchyModel BuildGroupsHierarchy(Group currentGroup)
        {
            var childrenHierarchy = currentGroup.Children.OfType<Group>()
                .Select(this.BuildGroupsHierarchy)
                .ToList();

            return new GroupsHierarchyModel
            {
                Id = currentGroup.PublicKey,
                Title = currentGroup.Title,
                IsRoster = currentGroup.IsRoster,
                Children = childrenHierarchy
            };
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
                parents.Add(new GroupReferenceModel{ Id = @group.PublicKey, IsRoster = @group.IsRoster});
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
                                    RosterLevelDepthOfParentQuestion = 0,
                                    Options = singleQuestion.Answers.Select(ToCascadingOptionModel).ToList(),

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
                        questionModel = new IntegerNumericQuestionModel { IsRosterSizeQuestion = isRosterSizeQuestion };
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
