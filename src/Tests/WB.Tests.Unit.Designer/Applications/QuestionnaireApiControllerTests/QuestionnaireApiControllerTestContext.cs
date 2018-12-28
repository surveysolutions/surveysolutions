﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Main.Core.Documents;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.BoundedContexts.Designer.Implementation.Services.Accounts.Membership;
using WB.Core.BoundedContexts.Designer.Services;
using WB.Core.BoundedContexts.Designer.ValueObjects;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Search;
using WB.UI.Designer.Api;
using WB.UI.Designer.Code;
using WB.UI.Designer.Implementation.Services;
using WB.UI.Designer.Services;

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
            IMembershipUserService userHelper = null,
            IWebTesterService webTesterService = null)
        {
            var questionnaireController = new QuestionnaireController(
                chapterInfoViewFactory ?? Mock.Of<IChapterInfoViewFactory>(),
                questionnaireInfoViewFactory ?? Mock.Of<IQuestionnaireInfoViewFactory>(),
                questionnaireViewFactory ?? Mock.Of<IQuestionnaireViewFactory>(),
                questionnaireVerifier ?? Mock.Of<IQuestionnaireVerifier>(),
                verificationErrorsMapper ?? Mock.Of<IVerificationErrorsMapper>(),
                questionnaireInfoFactory ?? Mock.Of<IQuestionnaireInfoFactory>(),
                userHelper ?? Mock.Of < IMembershipUserService>(),
                Mock.Of<WebTesterSettings>(),
                webTesterService ?? Mock.Of<IWebTesterService>());

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

        public static QuestionnaireDocument CreateQuestionnaireDocument(params IComposite[] questionnaireItems)
        {
            return new QuestionnaireDocument()
            {
                Children = new ReadOnlyCollection<IComposite>(questionnaireItems.ToList())
            };
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
            {
                Id = questionId,
                Type = QuestionType.SingleOption,
                Options = options,
                IsFilteredCombobox = isFilteredCombobox,
            };
        }
    }
}
