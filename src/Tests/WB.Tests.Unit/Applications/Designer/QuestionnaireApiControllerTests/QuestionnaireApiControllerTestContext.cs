using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.GenericSubdomains.Utils;
using WB.Core.Infrastructure.ReadSide;
using WB.UI.Designer.Api;
using WB.UI.Designer.Code;
using WB.UI.Designer.Models;
using WB.UI.Shared.Web.Membership;

namespace WB.Tests.Unit.Applications.Designer.QuestionnaireApiControllerTests
{
    internal class QuestionnaireApiControllerTestContext
    {
        public static QuestionnaireController CreateQuestionnaireController(IChapterInfoViewFactory chapterInfoViewFactory = null,
            IQuestionnaireInfoViewFactory questionnaireInfoViewFactory = null,
            IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory = null,
            IQuestionnaireVerifier questionnaireVerifier = null,
            IVerificationErrorsMapper verificationErrorsMapper = null,
            IQuestionnaireInfoFactory questionnaireInfoFactory = null,
            IMembershipUserService userHelper = null)
        {
            var questionnaireController = new QuestionnaireController(
                chapterInfoViewFactory ?? Mock.Of<IChapterInfoViewFactory>(),
                questionnaireInfoViewFactory ?? Mock.Of<IQuestionnaireInfoViewFactory>(),
                questionnaireViewFactory ?? Mock.Of<IViewFactory<QuestionnaireViewInputModel, QuestionnaireView>>(),
                questionnaireVerifier ?? Mock.Of<IQuestionnaireVerifier>(),
                verificationErrorsMapper ?? Mock.Of<IVerificationErrorsMapper>(),
                questionnaireInfoFactory ?? Mock.Of<IQuestionnaireInfoFactory>(),
                userHelper ?? Mock.Of < IMembershipUserService>());

            return questionnaireController;
        }


        public static QuestionnaireItemLink CreateQuestionnaireItemLink()
        {
            return new QuestionnaireItemLink
                   {
                       Id = "questionId",
                       ChapterId = "chapterId",
                       Title = "some title"
                   };
        }

        public static QuestionnaireView CreateQuestionnaireView(QuestionnaireDocument questionnaireDocument1)
        {
            return new QuestionnaireView(questionnaireDocument1);
        }

        public static QuestionnaireInfoView CreateQuestionnaireInfoView()
        {
            return new QuestionnaireInfoView();
        }

        public static GroupInfoView CreateChapterInfoView()
        {
            return new GroupInfoView();
        }

        public static QuestionnaireDocument CreateQuestionnaireDocument()
        {
            return new QuestionnaireDocument();
        }

        public static QuestionnaireDocument CreateQuestionnaireDocument(IEnumerable<IComposite> questionnaireItems)
        {
            return new QuestionnaireDocument() {Children = new List<IComposite>(questionnaireItems)};
        }

        public static NewEditStaticTextView CreateStaticTextView()
        {
            return new NewEditStaticTextView();
        }

        internal static QuestionnaireVerificationError[] CreateQuestionnaireVerificationErrors()
        {
            return new QuestionnaireVerificationError[2]
            {
                new QuestionnaireVerificationError("aaa","aaaa", new QuestionnaireVerificationReference[1]{ new QuestionnaireVerificationReference( QuestionnaireVerificationReferenceType.Question, Guid.NewGuid())}), 
                new QuestionnaireVerificationError("bbb","bbbb", new QuestionnaireVerificationReference[1]{ new QuestionnaireVerificationReference( QuestionnaireVerificationReferenceType.Group, Guid.NewGuid())}), 
            };
        }

        internal static QuestionnaireVerificationError[] CreateQuestionnaireVerificationErrors(IEnumerable<IComposite> questionnaireItems)
        {
            return
                questionnaireItems.Select(
                    questionnaireItem =>
                        new QuestionnaireVerificationError("aaa", "aaaa",
                            new QuestionnaireVerificationReference[1]
                            {
                                new QuestionnaireVerificationReference(QuestionnaireVerificationReferenceType.Question,
                                    questionnaireItem.PublicKey)
                            })).ToArray();
        }

        internal static NewEditQuestionView CreateSingleoptionFilteredCombobox(Guid questionId, int optionsCount = 3, bool isFilteredCombobox = false)
        {
            var options = Enumerable.Range(1, optionsCount).Select(x => new CategoricalOption { Title = x.ToString(CultureInfo.InvariantCulture), Value = x }).ToArray();
            
            return new NewEditQuestionView
            {
                Id = questionId,
                Type = QuestionType.SingleOption,
                Options = options,
                IsFilteredCombobox = isFilteredCombobox,
            };
        }
        internal static VerificationError[] CreateVerificationErrors()
        {
            return new VerificationError[3]
            {
                new VerificationError
                {
                    Code = "aaa",
                    Message = "aaaa",
                    References = new List<VerificationReference>
                    {
                        new VerificationReference
                        {
                            Type = QuestionnaireVerificationReferenceType.Question,
                            ItemId = Guid.NewGuid().FormatGuid(),
                            Title = "aaaaaaaaaaaaaaaaaaaaaa"
                        }
                    }
                }
                ,
                new VerificationError
                {
                    Code = "aaa",
                    Message = "aaaa",
                    References = new List<VerificationReference>
                    {
                        new VerificationReference
                        {
                            Type = QuestionnaireVerificationReferenceType.Question,
                            ItemId = Guid.NewGuid().FormatGuid(),
                            Title = "aaaaaaaaaaaaaaaaaaaaaa"
                        }
                    }
                }
                ,
                new VerificationError
                {
                    Code = "ccc",
                    Message = "ccccc",
                    References = new List<VerificationReference>
                    {
                        new VerificationReference
                        {
                            Type = QuestionnaireVerificationReferenceType.Question,
                            ItemId = Guid.NewGuid().FormatGuid(),
                            Title = "ccccccccccccccccc"
                        }
                    }
                }
            };
        }
    }
}
