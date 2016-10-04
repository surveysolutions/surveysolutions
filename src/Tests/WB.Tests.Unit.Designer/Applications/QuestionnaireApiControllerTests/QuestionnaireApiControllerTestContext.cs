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
using WB.Core.Infrastructure.ReadSide;
using WB.UI.Designer.Api;
using WB.UI.Designer.Code;
using WB.UI.Shared.Web.Membership;

namespace WB.Tests.Unit.Designer.Applications.QuestionnaireApiControllerTests
{
    internal class QuestionnaireApiControllerTestContext
    {
        public static QuestionnaireController CreateQuestionnaireController(IChapterInfoViewFactory chapterInfoViewFactory = null,
            IQuestionnaireInfoViewFactory questionnaireInfoViewFactory = null,
            IQuestionnaireViewFactory questionnaireViewFactory = null,
            IQuestionnaireVerifier questionnaireVerifier = null,
            IVerificationErrorsMapper verificationErrorsMapper = null,
            IQuestionnaireInfoFactory questionnaireInfoFactory = null,
            IMembershipUserService userHelper = null)
        {
            var questionnaireController = new QuestionnaireController(
                chapterInfoViewFactory ?? Mock.Of<IChapterInfoViewFactory>(),
                questionnaireInfoViewFactory ?? Mock.Of<IQuestionnaireInfoViewFactory>(),
                questionnaireViewFactory ?? Mock.Of<IQuestionnaireViewFactory>(),
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

        public static NewChapterView CreateChapterInfoView()
        {
            return new NewChapterView() {Chapter = new GroupInfoView()};
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

        internal static QuestionnaireVerificationMessage[] CreateQuestionnaireVerificationErrors(IEnumerable<IComposite> questionnaireItems)
        {
            return
                questionnaireItems.Select(
                    questionnaireItem =>
                        Create.VerificationError("aaa", "aaaa",
                            new QuestionnaireNodeReference[1]
                            {
                                new QuestionnaireNodeReference(QuestionnaireVerificationReferenceType.Question,
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
    }
}
