using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Microsoft.Extensions.Options;
using Moq;
using WB.Core.BoundedContexts.Designer.DataAccess;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.ChangeHistory;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.UI.Designer.Code;
using WB.UI.Designer.Controllers.Api.Designer;
using WB.UI.Designer.Services;

namespace WB.Tests.Unit.Designer.Applications.QuestionnaireApiControllerTests
{
    internal class QuestionnaireApiControllerTestContext
    {
        public static QuestionnaireApiController CreateQuestionnaireController(
            IChapterInfoViewFactory chapterInfoViewFactory = null,
            IQuestionnaireInfoViewFactory questionnaireInfoViewFactory = null,
            IQuestionnaireViewFactory questionnaireViewFactory = null,
            IQuestionnaireVerifier questionnaireVerifier = null,
            IVerificationErrorsMapper verificationErrorsMapper = null,
            IQuestionnaireInfoFactory questionnaireInfoFactory = null,
            IWebTesterService webTesterService = null)
        {
            var questionnaireController = new QuestionnaireApiController(
                chapterInfoViewFactory ?? Mock.Of<IChapterInfoViewFactory>(),
                questionnaireInfoViewFactory ?? Mock.Of<IQuestionnaireInfoViewFactory>(),
                questionnaireViewFactory ?? Mock.Of<IQuestionnaireViewFactory>(),
                questionnaireVerifier ?? Mock.Of<IQuestionnaireVerifier>(),
                verificationErrorsMapper ?? Mock.Of<IVerificationErrorsMapper>(),
                questionnaireInfoFactory ?? Mock.Of<IQuestionnaireInfoFactory>(),
                Mock.Of<IOptions<WebTesterSettings>>(),
                webTesterService ?? Mock.Of<IWebTesterService>(),
                Mock.Of<DesignerDbContext>());

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
            return Create.QuestionnaireView(questionnaireDocument1);
        }

        public static NewChapterView CreateChapterInfoView()
        {
            return new NewChapterView(chapter : new GroupInfoView());
        }

        public static QuestionnaireDocument CreateQuestionnaireDocument()
        {
            return new QuestionnaireDocument();
        }

        public static QuestionnaireDocument CreateQuestionnaireDocument(params IComposite[] questionnaireItems)
        {
            return new QuestionnaireDocument()
            {
                Children = new ReadOnlyCollection<IComposite>(questionnaireItems.ToList())
            };
        }

       
        internal static QuestionnaireVerificationMessage[] CreateQuestionnaireVerificationErrors(IEnumerable<IComposite> questionnaireItems)
        {
            return
                questionnaireItems.Select(
                    questionnaireItem =>
                        Create.VerificationError("aaa", "aaaa",
                            new QuestionnaireEntityReference[1]
                            {
                                new QuestionnaireEntityReference(QuestionnaireVerificationReferenceType.Question,
                                    questionnaireItem.PublicKey)
                            })).ToArray();
        }

        internal static NewEditQuestionView CreateSingleoptionFilteredCombobox(Guid questionId, int optionsCount = 3, bool isFilteredCombobox = false)
        {
            var options = Enumerable.Range(1, optionsCount).Select(x => new CategoricalOption { Title = x.ToString(CultureInfo.InvariantCulture), Value = x }).ToArray();
            
            return new NewEditQuestionView
            (
                id : questionId,
                type : QuestionType.SingleOption,
                options : options,
                isFilteredCombobox : isFilteredCombobox,
                parentGroupId: Guid.Parse("11111111111111111111111111111111"),
                questionTypeOptions: new QuestionnaireInfoFactory.SelectOption[0] ,
                isPreFilled:false,
                questionScope:QuestionScope.Interviewer

            );
        }

        protected static QuestionnaireRevision questionnaireId = Create.QuestionnaireRevision("22222222222222222222222222222222");
    }
}
