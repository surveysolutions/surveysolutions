using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Entities.SubEntities;
using Moq;
using WB.Core.Infrastructure.ReadSide.Repository.Accessors;
using WB.Core.SharedKernels.DataCollection.Views.Questionnaire;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.Factories;
using WB.Core.SharedKernels.SurveyManagement.Views.Reposts.InputModels;

namespace WB.Tests.Unit.SharedKernels.SurveyManagement.QuestionnaireQuestionInfoFactoryTests
{
    internal class QuestionnaireQuestionInfoFactoryTestContext
    {
        protected static IQuestion CreateQuestion(QuestionType questionType, Guid? questionId = null)
        {
            return Mock.Of<IQuestion>(x
                => x.QuestionType == questionType
                   && x.StataExportCaption == questionType.ToString()
                   && x.PublicKey == (questionId.HasValue? questionId.Value : Guid.NewGuid())
                );
        }
        protected static QuestionnaireQuestionInfoFactory CreateQuestionnaireQuestionInfoFactory(IReadSideKeyValueStorage<QuestionnaireDocumentVersioned> questionnaireStore = null)
        {
            return new QuestionnaireQuestionInfoFactory(questionnaireStore ?? Mock.Of<IReadSideKeyValueStorage<QuestionnaireDocumentVersioned>>());
        }

        protected static QuestionnaireQuestionInfoInputModel CreateQuestionnaireQuestionInfoInputModel(Guid questionnaireId, long version = 1, QuestionType? questionType = null)
        {
            return new QuestionnaireQuestionInfoInputModel
            {
                QuestionnaireId = questionnaireId,
                QuestionnaireVersion = version,
                QuestionType = questionType
            };
        }
    }
}
