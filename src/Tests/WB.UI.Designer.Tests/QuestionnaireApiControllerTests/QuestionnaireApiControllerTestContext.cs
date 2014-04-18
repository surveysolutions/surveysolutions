using System;
using System.Collections.Generic;
using System.Web.Http;
using Main.Core.Documents;
using Main.Core.View;
using Moq;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.ChapterInfo;
using WB.Core.BoundedContexts.Designer.Views.Questionnaire.Edit.QuestionnaireInfo;
using WB.Core.SharedKernels.QuestionnaireVerification.Services;
using WB.Core.SharedKernels.QuestionnaireVerification.ValueObjects;
using WB.UI.Designer.Api;
using WB.UI.Designer.Code;
using WB.UI.Designer.Models;

namespace WB.UI.Designer.Tests.QuestionnaireApiControllerTests
{
    internal class QuestionnaireApiControllerTestContext
    {
        public static QuestionnaireController CreateQuestionnaireController(IViewFactory<ChapterInfoViewInputModel, IQuestionnaireItem> chapterInfoViewFactory = null,
            IViewFactory<QuestionnaireInfoViewInputModel, QuestionnaireInfoView> questionnaireInfoViewFactory = null,
            IViewFactory<QuestionnaireViewInputModel, QuestionnaireView> questionnaireViewFactory = null,
            IQuestionnaireVerifier questionnaireVerifier = null,
            IVerificationErrorsMapper verificationErrorsMapper = null)
        {
            return new QuestionnaireController(
                chapterInfoViewFactory ?? Mock.Of<IViewFactory<ChapterInfoViewInputModel, IQuestionnaireItem>>(),
                questionnaireInfoViewFactory ?? Mock.Of<IViewFactory<QuestionnaireInfoViewInputModel, QuestionnaireInfoView>>(),
                questionnaireViewFactory ?? Mock.Of<IViewFactory<QuestionnaireViewInputModel, QuestionnaireView>>(),
                questionnaireVerifier ?? Mock.Of<IQuestionnaireVerifier>(),
                verificationErrorsMapper ?? Mock.Of<IVerificationErrorsMapper>()
                );
        }

        public static QuestionnaireView CreateQuestionnaireView(QuestionnaireDocument questionnaireDocument1)
        {
            return new QuestionnaireView(questionnaireDocument1);
        }

        public static QuestionnaireDocument CreateQuestionnaireDocument()
        {
            return new QuestionnaireDocument();
        }

        internal static QuestionnaireVerificationError[] CreateQuestionnaireVerificationErrors()
        {
            return new QuestionnaireVerificationError[2]
            {
                new QuestionnaireVerificationError("aaa","aaaa", new QuestionnaireVerificationReference[1]{ new QuestionnaireVerificationReference( QuestionnaireVerificationReferenceType.Question, Guid.NewGuid())}), 
                new QuestionnaireVerificationError("bbb","bbbb", new QuestionnaireVerificationReference[1]{ new QuestionnaireVerificationReference( QuestionnaireVerificationReferenceType.Group, Guid.NewGuid())}), 
            };
        }


        internal static VerificationError[] CreateVerificationErrors()
        {
            return new VerificationError[2]
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
                            Id = Guid.NewGuid(),
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
                            Id = Guid.NewGuid(),
                            Title = "aaaaaaaaaaaaaaaaaaaaaa"
                        }
                    }
                }
            };
        }
    }
}
