using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.SubEntities;
using Main.Core.Entities.SubEntities.Question;
using WB.Core.BoundedContexts.QuestionnaireTester.Services;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.PlainStorage;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities;
using WB.Core.SharedKernels.DataCollection.Implementation.Entities.QuestionModels;
using WB.Core.SharedKernels.DataCollection.Utils;

namespace WB.Core.BoundedContexts.QuestionnaireTester.Implementation.Services
{
    internal class QuestionnaireImportService : IQuestionnaireImportService
    {
        private readonly IPlainKeyValueStorage<QuestionnaireModel> questionnaireModelRepository;

        public QuestionnaireImportService(IPlainKeyValueStorage<QuestionnaireModel> questionnaireModelRepository)
        {
            this.questionnaireModelRepository = questionnaireModelRepository;
        }

        public void ImportQuestionnaire(QuestionnaireDocument questionnaireDocument)
        {
            questionnaireDocument.ConnectChildrenWithParent();
            
            var questionnaireModel = new QuestionnaireModel();

            var groups = questionnaireDocument.GetAllGroups().ToList();
            var questions = questionnaireDocument.GetAllQuestions().ToList();
            var staticTexts = questionnaireDocument.GetEntitiesByType<IStaticText>().ToList();

            questionnaireModel.Id = questionnaireDocument.PublicKey;
            questionnaireModel.Title = questionnaireDocument.Title;
            questionnaireModel.StaticTexts = staticTexts.ToDictionary(x => x.PublicKey, CreateStaticTextModel);
            questionnaireModel.Questions = questions.ToDictionary(x => x.PublicKey, CreateQuestionModel);
            questionnaireModel.PrefilledQuestionsIds = questions.Where(x => x.Featured)
                .Select(x => questionnaireModel.Questions[x.PublicKey])
                .Select(x => new QuestionnaireReferenceModel { Id = x.Id, ModelType = x.GetType() })
                .ToList();
            questionnaireModel.GroupsWithoutNestedChildren = groups.ToDictionary(x => x.PublicKey, x => CreateGroupModelWithoutNestedChildren(x, questionnaireModel.Questions));
            questionnaireModel.GroupParents = groups.ToDictionary(x => x.PublicKey, x => this.BuildParentsList(x, questionnaireDocument.PublicKey));
            questionnaireModel.GroupsHierarchy = questionnaireDocument.Children.Cast<Group>().Select(this.BuildGroupsHierarchy).ToList();

            questionnaireModelRepository.Store(questionnaireModel, questionnaireDocument.PublicKey.FormatGuid());
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

        private List<QuestionnaireReferenceModel> BuildParentsList(Group group, Guid questionnaireId)
        {
            var parents = new List<QuestionnaireReferenceModel>();

            var parent = group.GetParent() as Group;
            while (parent != null && parent.PublicKey != questionnaireId )
            {
                var parentPlaceholder = new QuestionnaireReferenceModel
                {
                    ModelType = parent.IsRoster ? typeof(RosterModel) : typeof(GroupModel),
                    Id = parent.PublicKey
                };
                parents.Add(parentPlaceholder);

                parent = parent.GetParent() as Group;
            }
            return parents;
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


        private static BaseQuestionModel CreateQuestionModel(IQuestion question)
        {
            BaseQuestionModel questionModel;
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
                        questionModel = new SingleOptionQuestionModel
                        {
                            CascadeFromQuestionId = singleQuestion.CascadeFromQuestionId,
                            IsFilteredCombobox = singleQuestion.IsFilteredCombobox,
                            Options = singleQuestion.Answers.Select(ToOptionModel).ToList()
                        };
                    }
                    break;
                case QuestionType.MultyOption:
                    var multiQuestion = question as MultyOptionsQuestion;
                    if (multiQuestion.LinkedToQuestionId.HasValue)
                    {
                        questionModel = new LinkedMultiOptionQuestionModel
                        {
                            LinkedToQuestionId = multiQuestion.LinkedToQuestionId.Value
                        };
                    }
                    else
                    {
                        questionModel = new MultiOptionQuestionModel
                        {
                            AreAnswersOrdered = multiQuestion.AreAnswersOrdered,
                            MaxAllowedAnswers = multiQuestion.MaxAllowedAnswers,
                            Options = question.Answers.Select(ToOptionModel).ToList()
                        };
                    }
                    break;
                case QuestionType.Numeric:
                    var numericQuestion = question as INumericQuestion;
                    if (numericQuestion.IsInteger)
                    {
                        questionModel = new IntegerNumericQuestionModel { MaxValue = numericQuestion.MaxValue };
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
                    questionModel = new MaskedTextQuestionModel { Mask = (question as TextQuestion).Mask };
                    break;
                case QuestionType.TextList:
                    questionModel = new TextListQuestionModel();
                    break;
                case QuestionType.QRBarcode:
                    questionModel = new QrBarcodeQuestionModel();
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
    }
}
